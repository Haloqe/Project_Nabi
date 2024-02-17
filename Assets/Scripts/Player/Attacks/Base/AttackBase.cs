using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class AttackBase : MonoBehaviour
{
    protected PlayerDamageDealer _damageDealer;
    protected PlayerMovement _playerMovement;
    protected Animator _animator;
    
    private float _attackDelay;
    private float _attackSpeedMultiplier;
    protected AttackInfo _damageInitBase;
    protected AttackInfo _damageBase;
    protected float _attackPostDelay;
    protected ELegacyType _attackType;
    
    public GameObject VFXObject;
    private Material _defaultVFXMaterial;
    
    // Legacy
    public EWarrior activeWarrior;
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
        activeWarrior = legacyAsset.Warrior;
        _activeLegacyPreservation = preservation;
        _activeLegacy = legacyAsset;
        _activeLegacy.Init(gameObject.transform);
        _activeLegacy.SetPreservation(preservation);
        OnUpdateLegacyDamage();
        OnUpdateLegacyStatusEffect();
        OnUpdateLegacyPreservation();
    }

    public void UpdateSpawnSize(float increaseAmount, EIncreaseMethod method)
    {
        if (_activeLegacy) _activeLegacy.UpdateSpawnSize(method, increaseAmount);
    }

    protected virtual void OnUpdateLegacyDamage()
    {
        if (_activeLegacy == null) return;
        var changeAmount = _activeLegacy.AdditionalDamage[(int)_activeLegacyPreservation];
        
        if (changeAmount != 0)
        {
            var method = _activeLegacy.DamageIncreaseMethod;
            foreach (var damage in _damageBase.Damages)
            {
                damage.TotalAmount = Utility.GetChangedValue(damage.TotalAmount, changeAmount, method);
            }
        }
    }
    
    protected virtual void OnUpdateLegacyStatusEffect()
    {
        if (_activeLegacy == null) return;
        
        // Get status effect
        EStatusEffect warriorSpecificEffect = PlayerAttackManager.Instance
            .GetWarriorStatusEffect(_activeLegacy.Warrior,
                _damageDealer.GetStatusEffectLevel(_activeLegacy.Warrior));
        
        // Update status effect of base damage
        var newStatusEffectsBase = _damageBase.StatusEffects;
        var newEffect = new StatusEffectInfo(warriorSpecificEffect,
            _activeLegacy.StatusEffects[(int)_activeLegacyPreservation].Strength,
            _activeLegacy.StatusEffects[(int)_activeLegacyPreservation].Duration,
            _activeLegacy.StatusEffects[(int)_activeLegacyPreservation].Chance);
        newStatusEffectsBase.Add(newEffect);
        
        // Additional status effect
        if (_activeLegacy.ExtraStatusEffects[(int)_activeLegacyPreservation].Effect != EStatusEffect.None)
        {
            newEffect = new StatusEffectInfo(_activeLegacy.ExtraStatusEffects[(int)_activeLegacyPreservation].Effect,
                _activeLegacy.ExtraStatusEffects[(int)_activeLegacyPreservation].Strength,
                _activeLegacy.ExtraStatusEffects[(int)_activeLegacyPreservation].Duration,
                _activeLegacy.StatusEffects[(int)_activeLegacyPreservation].Chance);
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
    }
}