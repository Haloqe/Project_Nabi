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
    protected ActiveLegacySO _activeLegacy;
    protected ELegacyPreservation _activeLegacyPreservation;
    
    public virtual void Reset()
    {
        _attackInfo = _attackInfoInit.Clone();
        _damageInfo = _damageInfoInit;
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
        UpdateLegacyPreservation(preservation);
    }

    public void UpdateSpawnSize(float increaseAmount, EIncreaseMethod method)
    {
        if (_activeLegacy) _activeLegacy.UpdateSpawnSize(method, increaseAmount);
    }

    protected virtual void UpdateLegacyDamage()
    {
        if (_activeLegacy == null) return;
        _damageInfo.BaseDamage = _activeLegacy.DamageInfos[(int)_activeLegacyPreservation].BaseDamage;
        _damageInfo.RelativeDamage = _activeLegacy.DamageInfos[(int)_activeLegacyPreservation].RelativeDamage;
        _attackInfo.Damage.TotalAmount = _damageInfo.BaseDamage + PlayerController.Instance.Strength * _damageInfo.RelativeDamage;
    }
    
    protected virtual void UpdateLegacyStatusEffect()
    {
        if (_activeLegacy == null) return;
        
        // Get status effect
        EStatusEffect warriorSpecificEffect = PlayerAttackManager.Instance
            .GetWarriorStatusEffect(_activeLegacy.Warrior,
                _damageDealer.GetStatusEffectLevel(_activeLegacy.Warrior));
        
        // Update status effect of base damage
        var newStatusEffectsBase = _attackInfo.StatusEffects;
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
                _activeLegacy.ExtraStatusEffects[(int)_activeLegacyPreservation].Chance);
            newStatusEffectsBase.Add(newEffect);
        }
        
        // Update status effect of objects spawned from legacy
        _attackInfo.StatusEffects = newStatusEffectsBase;
        _activeLegacy.OnUpdateStatusEffect(warriorSpecificEffect);
    }

    public virtual void UpdateLegacyPreservation(ELegacyPreservation preservation)
    {
        if (_activeLegacy == null) return;
        Debug.Log(PlayerAttackManager.Instance.GetBoundActiveLegacyName(ELegacyType.Ranged) + " updated to " + preservation);
        _activeLegacy.SetPreservation(preservation);
        UpdateLegacyDamage();
        UpdateLegacyStatusEffect();
    }
}