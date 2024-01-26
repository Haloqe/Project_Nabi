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
    protected bool _isAttached;
    
    public virtual void Reset()
    {
        _damageBase = _damageInitBase;
        ResetVFXs();
    }

    public virtual void ResetVFXs()
    {
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

    public virtual void IsActiveBound()
    {

    }

    public virtual void BindLegacy()
    {

    }
}