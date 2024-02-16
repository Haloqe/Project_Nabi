using System.Collections;
using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    protected PlayerDamageDealer _damageDealer;
    protected PlayerMovement _playerMovement;
    protected Animator _animator;
    
    private float _attackDelay;
    private float _attackSpeedMultiplier;
    protected SDamageInfo _damageInitBase;
    protected SDamageInfo _damageBase;
    protected float _attackPostDelay;
    protected ELegacyType _attackType;
    
    public GameObject VFXObject;
    private Material _defaultVFXMaterial;
    
    // Legacy
    protected ActiveLegacySO _activeLegacy;
    protected ELegacyPreservation _activeLegacyPreservation;
    
    public virtual void Reset()
    {
        _damageBase = _damageInitBase;
        VFXObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = _defaultVFXMaterial;
    }
    
    public virtual void Start()
    {
        _defaultVFXMaterial = VFXObject.GetComponent<ParticleSystemRenderer>().sharedMaterial;
        _animator = GetComponent<Animator>();
        _damageDealer = GetComponent<PlayerDamageDealer>();
        _playerMovement = GetComponent<PlayerMovement>();
        Reset();
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
        return _activeLegacy == null;
    }

    public void BindActiveLegacy(ActiveLegacySO legacyAsset, ELegacyPreservation preservation)
    {
        _activeLegacyPreservation = preservation;
        _activeLegacy = legacyAsset;
        _activeLegacy.Init(gameObject.transform);
        OnUpdateLegacyStatusEffect();
        OnUpdateLegacyPreservation();
    }

    public virtual void OnUpdateLegacyStatusEffect()
    {
        if (_activeLegacy == null) return;
        
        // Get status effect
        EStatusEffect warriorSpecificEffect = PlayerAttackManager.Instance
            .GetWarriorStatusEffect(_activeLegacy.Warrior,
                _damageDealer.GetStatusEffectLevel(_activeLegacy.Warrior));
        
        // Update status effect of base damage
        var newStatusEffectsBase = _damageBase.StatusEffects;
        var newEffect = new SStatusEffect(warriorSpecificEffect,
            _activeLegacy.StatusEffectStrengths[(int)_activeLegacyPreservation],
            _activeLegacy.StatusEffectDurations[(int)_activeLegacyPreservation]);
        newStatusEffectsBase.Add(newEffect);
        
        // Additional status effect
        if (_activeLegacy.ExtraStatusEffect != EStatusEffect.None)
        {
            newEffect = new SStatusEffect(_activeLegacy.ExtraStatusEffect,
                _activeLegacy.ExtraStatusEffectStrengths[(int)_activeLegacyPreservation],
                _activeLegacy.ExtraStatusEffectDurations[(int)_activeLegacyPreservation]);
            newStatusEffectsBase.Add(newEffect);
        }
        
        // Update status effect of objects spawned from legacy
        _damageBase.StatusEffects = newStatusEffectsBase;
        _activeLegacy.OnUpdateStatusEffect(warriorSpecificEffect);
    }

    protected virtual void OnUpdateLegacyPreservation()
    {
        if (_activeLegacy == null) return;
        if (_damageInitBase.Damages.Count == 0) return;
        
        var newBaseDamage = _damageInitBase.Damages[0];
        newBaseDamage.TotalAmount *= _activeLegacy.BaseDamageMultipliers[(int)_activeLegacyPreservation];
        _damageBase.Damages[0] = newBaseDamage;
    }
}