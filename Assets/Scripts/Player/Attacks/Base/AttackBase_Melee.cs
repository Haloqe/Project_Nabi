using System.Collections.Generic;
using UnityEngine;

public class AttackBase_Melee : AttackBase
{
    // VFX
    public GameObject VFXObjCombo;
    private Material _defaultVFXComboMaterial;

    // Collider
    private List<int> _affectedEnemies;
    private Collider2D _colliderBase;
    private Collider2D _colliderCombo;
    private float animLength;

    // Combo attack
    private float _comboTimeLimit = 1.3f;
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
    private readonly static int AttackIndex = Animator.StringToHash("AttackIndex");

    public override void Start()
    {
        base.Start();
        
        _baseDelay = 0.0f;
        _defaultVFXComboMaterial = VFXObjCombo.GetComponent<ParticleSystemRenderer>().sharedMaterial;
        _colliderBase = VFXObject.GetComponent<BoxCollider2D>();
        _colliderCombo = VFXObjCombo.GetComponent<BoxCollider2D>();
        _affectedEnemies = new List<int>();

        // Damage
        _damageInfoInit = new SDamageInfo { BaseDamage = 3.0f, RelativeDamage = 1.0f, };
        _damageInfoComboInit = new SDamageInfo { BaseDamage = 5.0f, RelativeDamage = 2.0f };
        _attackInfoInit.Damage.TotalAmount = _damageInfoInit.BaseDamage + _playerController.Strength * _damageInfoInit.RelativeDamage;
        _attackComboInit.Damage.TotalAmount = _damageInfoComboInit.BaseDamage + _playerController.Strength * _damageInfoComboInit.RelativeDamage;
        _attackInfoInit.CanBeDarkAttack = true;
        _attackComboInit.CanBeDarkAttack = true;
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        
        // Combo
        _comboDelay = 0;// .2f;
        _comboTimer = 0.0f;
        _comboStack = 0;
        
        // Damage
        activeLegacy = null;
        _damageComboInfo = _damageInfoComboInit;
        _attackComboInfo = _attackComboInit.Clone();
        _affectedEnemies.Clear();

        // VFX
        VFXObjCombo.GetComponent<ParticleSystemRenderer>().sharedMaterial = _defaultVFXComboMaterial;
    }

    private void Update()
    {
        // Return if not attacking
        if (_comboStack is 0 or 3) return;

        // Update combo timer if attacking
        _comboTimer += Time.deltaTime;
        
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
        Debug.Log("MELEE ATTACK, Stack: " + _comboStack);

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
            VFXObjCombo.GetComponent<ParticleSystemRenderer>().flip = (dir < 0 ? Vector3.right : Vector3.zero);
            VFXObjCombo.SetActive(true);
        }
        // Base Attack
        else
        {
            _animator.SetBool(IsMeleeCombo, false);
            _attackPostDelay = _baseDelay;
            VFXObject.GetComponent<ParticleSystemRenderer>().flip = (dir < 0 ? Vector3.right : Vector3.zero);
            VFXObject.SetActive(true);
            if (activeLegacy) ((Legacy_Melee)activeLegacy).OnAttack_Base();
        }      

        _animator.SetInteger(AttackIndex, (int)_attackType);
    }

    public void OnComboHit()
    {
        FindObjectOfType<TestCameraShake>().OnComboAttack();
        if (activeLegacy) ((Legacy_Melee)activeLegacy).OnAttack_Combo();
    }

    public void ToggleCollider(bool onOff)
    {
        // Combo Attack
        if (_comboStack == 0)
        {
            _colliderCombo.enabled = onOff;
        }
        // Base Attack
        else
        {
            _colliderBase.enabled = onOff;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the hit target is valid
        if (collision.gameObject.CompareTag("Enemy") == false) return;
        if (Utility.IsObjectInList(collision.gameObject, _affectedEnemies)) return;
        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target == null) return;

        // Recalculate damage based on current player strength
        _attackComboInfo.Damage.TotalAmount = _damageComboInfo.BaseDamage + _playerController.Strength * _damageComboInfo.RelativeDamage;
        _attackInfo.Damage.TotalAmount = _damageInfo.BaseDamage + _playerController.Strength * _damageInfo.RelativeDamage;
        
        // Do damage
        if (_comboStack == 0)
        {
            int i = 0;
        }
        _affectedEnemies.Add(collision.gameObject.GetInstanceID());
        _damageDealer.DealDamage(target, _comboStack == 0 ? _attackComboInfo : _attackInfo);
    }

    protected override void OnAttackEnd_PreDelay()
    {
        base.OnAttackEnd_PreDelay();
        _affectedEnemies.Clear();
    }

    public override void RecalculateDamage()
    {
        base.RecalculateDamage();
        
        // Combo - Calculate the damage with the newest player strength and damage information
        _attackComboInfo.Damage.TotalAmount = _damageComboInfo.BaseDamage + _playerController.Strength * _damageComboInfo.RelativeDamage;
        
        // Combo - Damage multiplier and additional damage from active legacy
        if (activeLegacy != null)
        {
            var legacyPreservation = (int)activeLegacy.preservation;
            _attackComboInfo.Damage.TotalAmount *= activeLegacy.damageMultipliers[legacyPreservation];
            var extra = activeLegacy.extraDamages[legacyPreservation];
            _attackComboInfo.Damage.TotalAmount += (extra.BaseDamage + _playerController.Strength * extra.RelativeDamage);
        }
    }

    public override void UpdateLegacyStatusEffect()
    {
        if (!activeLegacy) return;
        base.UpdateLegacyStatusEffect();
        
        // Combo attack
        var legacyPreservation = (int)activeLegacy.preservation;
        var newStatusEffectsCombo = _attackComboInfo.StatusEffects;
        EStatusEffect warriorSpecificEffect = PlayerAttackManager.Instance.GetWarriorStatusEffect(activeLegacy.warrior, _damageDealer.GetStatusEffectLevel(activeLegacy.warrior));
        var newEffect = new StatusEffectInfo(warriorSpecificEffect,
            activeLegacy.StatusEffects[legacyPreservation].Strength,
            activeLegacy.StatusEffects[legacyPreservation].Duration,
            activeLegacy.StatusEffects[legacyPreservation].Chance);
        newStatusEffectsCombo.Add(newEffect);
        _attackComboInfo.StatusEffects = newStatusEffectsCombo;
    }
}
