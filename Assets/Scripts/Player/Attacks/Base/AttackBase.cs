using System.Collections;
using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    // References
    protected Transform _player;
    protected PlayerController _playerController;
    protected PlayerDamageDealer _damageDealer;
    protected PlayerMovement _playerMovement;
    protected Animator _animator;
    
    // Effectors
    public GameObject baseEffector;
    public ParticleSystemRenderer BasePSRenderer { get; private set; }
    
    // Variables
    protected ELegacyType _attackType;
    protected float _attackPostDelay;
    
    // Damage
    protected AttackInfo _attackInfo = new AttackInfo();
    protected AttackInfo _attackInfoInit = new AttackInfo();
    protected SDamageInfo _damageInfo = new SDamageInfo();
    protected SDamageInfo _damageInfoInit = new SDamageInfo();
    
    // Legacy
    public ActiveLegacySO ActiveLegacy { get; protected set; }
    
    // Sound
    protected AudioSource AudioSourceBase;

    // Others
    protected readonly static int AttackIndex = Animator.StringToHash("AttackIndex");
    private Material _defaultVFXMaterial;

    public virtual void FlipVFX()
    {
        BasePSRenderer.flip = Mathf.Sign(_player.localScale.x) < 0 ? Vector3.right : Vector3.zero;
    }
    
    public virtual void Start()
    {
        if (gameObject.GetComponentInParent<Singleton<PlayerController>>().IsToBeDestroyed) return;
        _player = transform.parent;
        _playerController = PlayerController.Instance;
        BasePSRenderer = baseEffector.GetComponent<ParticleSystemRenderer>();
        _defaultVFXMaterial = BasePSRenderer.sharedMaterial;
        _animator = _player.GetComponent<Animator>();
        _damageDealer = _player.GetComponent<PlayerDamageDealer>();
        _playerMovement = _player.GetComponent<PlayerMovement>();
        
        PlayerEvents.StrengthChanged += RecalculateDamage;
        GameEvents.Restarted += Reset;
        GameEvents.CombatSceneChanged += OnCombatSceneChanged;
    }

    protected virtual void OnDestroy()
    {
        PlayerEvents.StrengthChanged -= RecalculateDamage;
        GameEvents.Restarted -= Reset;
        GameEvents.CombatSceneChanged -= OnCombatSceneChanged;
    }

    protected virtual void Reset()
    {
        // Remove bound legacy
        ActiveLegacy = null;
        
        // Reset attack info
        _attackInfo = _attackInfoInit.Clone(cloneDamage:true, cloneStatusEffect:true);
        _damageInfo = _damageInfoInit;
        
        // Reset VFX material
        if (_attackType == ELegacyType.Area) return;
        BasePSRenderer.sharedMaterial = _defaultVFXMaterial;
    }

    protected abstract void OnCombatSceneChanged();
    
    public abstract void Attack();

    protected virtual void OnAttackEnd_PreDelay()
    {

    }

    protected virtual void OnAttackEnd_PostDelay()
    {

    }

    public IEnumerator AttackPostDelayCoroutine()
    {
        OnAttackEnd_PreDelay();
        yield return new WaitForSecondsRealtime(_attackPostDelay);
        OnAttackEnd_PostDelay();
        _damageDealer.OnAttackEnd_PostDelay();
    }

    public virtual bool IsActiveBound()
    {
        return ActiveLegacy == null;
    }

    public void BindActiveLegacy(ActiveLegacySO legacyAsset, ELegacyPreservation preservation)
    {
        ActiveLegacy = legacyAsset;
        ActiveLegacy.Init(_player);
        UpdateActiveLegacyPreservation(preservation);
    }

    public void UpdateSpawnSize(float increaseAmount, EIncreaseMethod method)
    {
        if (ActiveLegacy) ActiveLegacy.UpdateSpawnSize(method, increaseAmount);
    }

    protected virtual void RecalculateDamage()
    {
        // Calculate the damage with the newest player strength and damage information
        _attackInfo.Damage.TotalAmount = _damageInfoInit.BaseDamage + _playerController.Strength * _damageInfoInit.RelativeDamage;
        
        // Damage multiplier and additional damage from active legacy
        if (ActiveLegacy)
        {
            var legacyPreservation = (int)ActiveLegacy.preservation;
            _attackInfo.Damage.TotalAmount *= ActiveLegacy.damageMultipliers[legacyPreservation];
            var extra = ActiveLegacy.extraDamages[legacyPreservation];
            _attackInfo.Damage.TotalAmount += extra.BaseDamage + _playerController.Strength * extra.RelativeDamage;
        }
    }

    public void UpdateLegacyStatusEffectSpecificWarrior(EWarrior warriorToUpdate)
    {
        if (ActiveLegacy == null) return;
        if (ActiveLegacy.warrior != warriorToUpdate) return;
        UpdateLegacyStatusEffect();
    }
    
    protected virtual void UpdateLegacyStatusEffect()
    {
        var legacyPreservation = (int)ActiveLegacy.preservation;
        
        // Get the newest status effect
        EStatusEffect warriorSpecificEffect = PlayerAttackManager.Instance
            .GetWarriorStatusEffect(ActiveLegacy.warrior, _damageDealer.GetStatusEffectLevel(ActiveLegacy.warrior));
        
        // Update status effect of base damage
        var newStatusEffectsBase = _attackInfoInit.GetClonedStatusEffect();
        var newEffect = new StatusEffectInfo(warriorSpecificEffect,
            ActiveLegacy.StatusEffects[legacyPreservation].Strength,
            ActiveLegacy.StatusEffects[legacyPreservation].Duration,
            ActiveLegacy.StatusEffects[legacyPreservation].Chance);
        newStatusEffectsBase.Add(newEffect);
        
        // Additional status effect
        if (ActiveLegacy.ExtraStatusEffects != null && ActiveLegacy.ExtraStatusEffects.Length > 0)
        {
            var extraCC = ActiveLegacy.ExtraStatusEffects[legacyPreservation];
            if (extraCC is { Effect: not (EStatusEffect.None or EStatusEffect.BuffButterfly) })
            {
                var extraEffect = new StatusEffectInfo(extraCC.Effect, extraCC.Strength, extraCC.Duration, extraCC.Chance);
                newStatusEffectsBase.Add(extraEffect);
            }
        }

        // Update status effect of objects spawned from legacy
        _attackInfo.StatusEffects = newStatusEffectsBase;
    }

    public void UpdateActiveLegacyPreservation(ELegacyPreservation preservation)
    {
        ActiveLegacy.preservation = preservation;
        RecalculateDamage();
        UpdateLegacyStatusEffect();
    }
}