using System.Collections.Generic;
using UnityEngine;

public class AttackBase_Melee : AttackBase
{
    // VFX
    public GameObject comboEffector;
    public ParticleSystemRenderer comboPSRenderer { get; private set; }
    private Material _defaultVFXComboMaterial;
    [SerializeField] [NamedArray(typeof(EWarrior))] private GameObject[] comboHitEffects;
    private GameObject currComboHitEffect;

    // Collider
    private List<int> _affectedEnemies;
    private Collider2D _colliderBase;
    private Collider2D _colliderCombo;

    // Combo attack
    private readonly float _comboTimeLimit = 1.3f;
    private float _comboTimer;
    private int _comboStack = 0;
    private float _comboDelay;
    private float _baseDelay;

    // Damage
    private AttackInfo _attackComboInfo = new AttackInfo();
    private AttackInfo _attackComboInit = new AttackInfo();
    private SDamageInfo _damageComboInfo;
    private SDamageInfo _damageInfoComboInit;
    
    // Animator variables
    private readonly static int IsMeleeCombo = Animator.StringToHash("IsMeleeCombo");

    public override void Start()
    {
        base.Start();
        _attackType = ELegacyType.Melee;
        _baseDelay = 0.0f;
        _comboDelay = 0.0f; 
        comboPSRenderer = comboEffector.GetComponent<ParticleSystemRenderer>();
        _defaultVFXComboMaterial = comboPSRenderer.sharedMaterial;
        _colliderBase = baseEffector.GetComponent<BoxCollider2D>();
        _colliderCombo = comboEffector.GetComponent<BoxCollider2D>();
        _affectedEnemies = new List<int>();
        AudioSourceBase = baseEffector.GetComponent<AudioSource>();

        // Damage
        _damageInfoInit = new SDamageInfo { BaseDamage = 3.0f, RelativeDamage = 1.0f, };
        _damageInfoComboInit = new SDamageInfo { BaseDamage = 5.0f, RelativeDamage = 2.0f };
        _attackInfoInit.Damage.TotalAmount = _damageInfoInit.BaseDamage + _playerController.Strength * _damageInfoInit.RelativeDamage;
        _attackComboInit.Damage.TotalAmount = _damageInfoComboInit.BaseDamage + _playerController.Strength * _damageInfoComboInit.RelativeDamage;
        _attackInfoInit.CanBeDarkAttack = true;
        _attackComboInit.CanBeDarkAttack = true;
        _attackInfoInit.ShouldUpdateTension = true;
        _attackComboInit.ShouldUpdateTension = true;
        
        Reset();
    }

    protected override void Reset()
    {
        base.Reset();
        
        // Combo
        _comboTimer = 0.0f;
        _comboStack = 0;
        
        // Damage
        _damageComboInfo = _damageInfoComboInit;
        _attackComboInfo = _attackComboInit.Clone(cloneDamage:true, cloneStatusEffect:true);
        _affectedEnemies.Clear();

        // VFX
        comboPSRenderer.sharedMaterial = _defaultVFXComboMaterial;
        currComboHitEffect = comboHitEffects[(int)EWarrior.MAX];
    }
    
    protected override void OnCombatSceneChanged()
    {
        _comboTimer = 0.0f;
        _comboStack = 0;
        _affectedEnemies.Clear();
    }

    private void Update()
    {
        // Return if not attacking
        if (_comboStack is 0 or 3) return;

        // Update combo timer if attacking
        _comboTimer += Time.unscaledDeltaTime;
        
        // Reset combo stack if time limit reached
        if (_comboTimer > _comboTimeLimit)
        {
            _comboStack = 0;
            _comboTimer = 0.0f;
        }        
    }

    public override void Attack()
    {
        // Add combo stack
        _comboStack++;
        _comboTimer = 0.0f;

        // Dir: Positive if left, negative if right
        float dir = Mathf.Sign(_player.localScale.x);
        _attackInfo.SetAttackDirToMyFront(_damageDealer.gameObject);
        _attackComboInfo.SetAttackDirToMyFront(_damageDealer.gameObject);

        // Combo Attack
        if (_comboStack == 3)
        {
            _animator.SetBool(IsMeleeCombo, true);
            _attackPostDelay = _comboDelay;
            _comboStack = 0; // Reset combo stack
            comboPSRenderer.flip = dir < 0 ? Vector3.right : Vector3.zero;
            comboEffector.SetActive(true);
        }
        // Base Attack
        else
        {
            _animator.SetBool(IsMeleeCombo, false);
            _attackPostDelay = _baseDelay;
            basePSRenderer.flip = dir < 0 ? Vector3.right : Vector3.zero;
            AudioSourceBase.pitch = Random.Range(0.85f, 1.08f);
            baseEffector.SetActive(true);
            if (ActiveLegacy) ((Legacy_Melee)ActiveLegacy).OnAttack_Base();
        }      

        _animator.SetInteger(AttackIndex, (int)_attackType);
    }

    public void OnComboHit()
    {
        FindObjectOfType<TestCameraShake>().OnComboAttack();
        Vector3 vfxPosition = new Vector3(_player.position.x - Mathf.Sign(_player.localScale.x), _player.position.y, 0);
        Instantiate(currComboHitEffect, vfxPosition, Quaternion.identity);
        if (ActiveLegacy) ((Legacy_Melee)ActiveLegacy).OnAttack_Combo();
    }

    public void ToggleCollider(bool onOff)
    {
        if (_comboStack == 0) _colliderCombo.enabled = onOff;
        else _colliderBase.enabled = onOff;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the hit target is valid
        var rootEnemy = collision.transform.root.gameObject;
        if (Utility.IsObjectInList(rootEnemy, _affectedEnemies)) return;
        IDamageable target = rootEnemy.GetComponent<IDamageable>();
        if (target == null) return;
        if (Utility.IsObjectInList(rootEnemy, _affectedEnemies)) return;

        // Recalculate damage based on current player strength
        _attackComboInfo.Damage.TotalAmount = _damageComboInfo.BaseDamage + _playerController.Strength * _damageComboInfo.RelativeDamage;
        _attackInfo.Damage.TotalAmount = _damageInfo.BaseDamage + _playerController.Strength * _damageInfo.RelativeDamage;
        
        // Do damage
        _affectedEnemies.Add(rootEnemy.GetInstanceID());
        _damageDealer.DealDamage(target, _comboStack == 0 ? _attackComboInfo : _attackInfo);
    }

    protected override void OnAttackEnd_PreDelay()
    {
        base.OnAttackEnd_PreDelay();
        _affectedEnemies.Clear();
    }

    protected override void RecalculateDamage()
    {
        base.RecalculateDamage();
        
        // Combo - Calculate the damage with the newest player strength and damage information
        _attackComboInfo.Damage.TotalAmount = _damageComboInfo.BaseDamage + _playerController.Strength * _damageComboInfo.RelativeDamage;
        if (ActiveLegacy == null) return;
        
        // Combo - Damage multiplier and additional damage from active legacy
        var legacyPreservation = (int)ActiveLegacy.preservation;
        _attackComboInfo.Damage.TotalAmount *= ActiveLegacy.damageMultipliers[legacyPreservation];
        var extra = ActiveLegacy.extraDamages[legacyPreservation];
        _attackComboInfo.Damage.TotalAmount += (extra.BaseDamage + _playerController.Strength * extra.RelativeDamage);
    }

    protected override void UpdateLegacyStatusEffect()
    {
        base.UpdateLegacyStatusEffect();
        
        // Combo attack
        var legacyPreservation = (int)ActiveLegacy.preservation;
        var newStatusEffectsCombo = _attackComboInfo.GetClonedStatusEffect();
        EStatusEffect warriorSpecificEffect = PlayerAttackManager.Instance
            .GetWarriorStatusEffect(ActiveLegacy.warrior, _damageDealer.GetStatusEffectLevel(ActiveLegacy.warrior));
        var newEffect = new StatusEffectInfo(warriorSpecificEffect,
            ActiveLegacy.StatusEffects[legacyPreservation].Strength,
            ActiveLegacy.StatusEffects[legacyPreservation].Duration,
            ActiveLegacy.StatusEffects[legacyPreservation].Chance);
        newStatusEffectsCombo.Add(newEffect);
        _attackComboInfo.StatusEffects = newStatusEffectsCombo;
    }

    public void UpdateHitVFX()
    {
        currComboHitEffect = comboHitEffects[(int)ActiveLegacy.warrior];
    }
}
