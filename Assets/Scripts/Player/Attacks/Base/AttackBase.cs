using System.Collections;
using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    protected PlayerDamageDealer _damageDealer;
    protected Animator _animator;
    private float _attackDelay;
    private float _attackSpeedMultiplier;
    protected SDamageInfo _damageInitBase;
    protected SDamageInfo _damageBase;
    protected float _attackPostDelay;

    protected ELegacyType _attackType;
    [SerializeField] protected GameObject VFXObject;
    protected bool _isAttached;

    
    public virtual void Initialise()
    {
    }

    public virtual void Reset()
    {
        _damageBase = _damageInitBase;
    }

    public virtual void Start()
    {
        Initialise();
        Reset();
        _animator = GetComponent<Animator>();
        _damageDealer = GetComponent<PlayerDamageDealer>();
    }

    public abstract void Attack();

    protected virtual void OnAttackEnd_PreDelay()
    {

    }

    protected virtual void OnAttackEnd_PostDelay()
    {

    }

    public IEnumerator AttackPostDelayCorountine()
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