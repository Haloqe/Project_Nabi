using System.Collections;
using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    protected PlayerController _playerController;
    protected PlayerDamageDealer _damageDealer;
    protected PlayerMovement _playerMovement;
    protected Animator _animator;
    
    private float _attackDelay;
    private float _attackSpeedMultiplier;
    protected float _attackPostDelay;
    protected ELegacyType _attackType;
    
    public GameObject VFXObject;
    private Material _defaultVFXMaterial;
    
    // Damage
    protected AttackInfo _attackInfo = new AttackInfo();
    protected AttackInfo _attackInfoInit = new AttackInfo();
    protected SDamageInfo _damageInfo = new SDamageInfo();
    protected SDamageInfo _damageInfoInit = new SDamageInfo();
    
    // Legacy
    public EWarrior activeWarrior;
    public ActiveLegacySO activeLegacy { get; protected set; }

    public virtual void Reset()
    {
        _attackInfo = _attackInfoInit.Clone();
        _damageInfo = _damageInfoInit;
        VFXObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = _defaultVFXMaterial;
    }
    
    public virtual void Start()
    {
        _playerController = PlayerController.Instance;
        _defaultVFXMaterial = VFXObject.GetComponent<ParticleSystemRenderer>().sharedMaterial;
        _animator = GetComponent<Animator>();
        _damageDealer = GetComponent<PlayerDamageDealer>();
        _playerMovement = GetComponent<PlayerMovement>();
    }
    
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
        yield return new WaitForSeconds(_attackPostDelay);
        OnAttackEnd_PostDelay();
        _damageDealer.OnAttackEnd_PostDelay();
    }

    public virtual bool IsActiveBound()
    {
        return activeLegacy == null;
    }

    public void BindActiveLegacy(ActiveLegacySO legacyAsset, ELegacyPreservation preservation)
    {
        activeWarrior = legacyAsset.warrior;
        activeLegacy = legacyAsset;
        activeLegacy.preservation = preservation;
        activeLegacy.Init(gameObject.transform);
        UpdateActiveLegacyPreservation(preservation);
    }

    public void UpdateSpawnSize(float increaseAmount, EIncreaseMethod method)
    {
        if (activeLegacy) activeLegacy.UpdateSpawnSize(method, increaseAmount);
    }

    public virtual void RecalculateDamage()
    {
        // Calculate the damage with the newest player strength and damage information
        _attackInfo.Damage.TotalAmount = _damageInfo.BaseDamage + _playerController.Strength * _damageInfo.RelativeDamage;
        
        // Damage multiplier and additional damage from active legacy
        if (activeLegacy)
        {
            var legacyPreservation = (int)activeLegacy.preservation;
            _attackInfo.Damage.TotalAmount *= activeLegacy.damageMultipliers[legacyPreservation];
            var extra = activeLegacy.extraDamages[legacyPreservation];
            _attackInfo.Damage.TotalAmount += extra.BaseDamage + _playerController.Strength * extra.RelativeDamage;
        }
    }
    
    public virtual void UpdateLegacyStatusEffect()
    {
        if (activeLegacy == null) return;
        var legacyPreservation = (int)activeLegacy.preservation;
        
        // Get the newest status effect
        EStatusEffect warriorSpecificEffect = PlayerAttackManager.Instance
            .GetWarriorStatusEffect(activeLegacy.warrior, _damageDealer.GetStatusEffectLevel(activeLegacy.warrior));
        
        // Update status effect of base damage)
        var newStatusEffectsBase = _attackInfo.StatusEffects;
        var newEffect = new StatusEffectInfo(warriorSpecificEffect,
            activeLegacy.StatusEffects[legacyPreservation].Strength,
            activeLegacy.StatusEffects[legacyPreservation].Duration,
            activeLegacy.StatusEffects[legacyPreservation].Chance);
        newStatusEffectsBase.Add(newEffect);    
        
        // Additional status effect
        if (activeLegacy.ExtraStatusEffects != null && activeLegacy.ExtraStatusEffects.Length > 0)
        {
            var extraCC = activeLegacy.ExtraStatusEffects[legacyPreservation];
            if (extraCC is { Effect: not (EStatusEffect.None or EStatusEffect.BuffButterfly) })
            {
                var extraEffect = new StatusEffectInfo(extraCC.Effect, extraCC.Strength, extraCC.Duration, extraCC.Chance);
                newStatusEffectsBase.Add(extraEffect);
            }
        }

        // Update status effect of objects spawned from legacy
        _attackInfo.StatusEffects = newStatusEffectsBase;
    }

    public virtual void UpdateActiveLegacyPreservation(ELegacyPreservation preservation)
    {
        if (activeLegacy == null) return;
        RecalculateDamage();
        UpdateLegacyStatusEffect();
    }
}