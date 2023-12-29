using System.Collections;
using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    protected PlayerAttack _playerAttack;
    protected Animator _animator;
    private float _attackDelay;
    private float _attackSpeedMultiplier;
    private SDamageInfo _baseDamage_Init;
    private SDamageInfo _baseDamage;
    protected float _attackPostDelay;

    protected ELegacyType _attackType;
    protected GameObject _vfxObject;
    protected bool _isAttached;

    
    public virtual void Initialise()
    {
        Reset();
    }

    public virtual void Reset()
    {
        _baseDamage = _baseDamage_Init;
    }

    public virtual void Start()
    {
        Initialise();
        _animator = GetComponent<Animator>();
        _playerAttack = GetComponent<PlayerAttack>();
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
        _playerAttack.OnAttackEnd_PostDelay();
    }

    public virtual void IsActiveBound()
    {

    }

    public virtual void BindLegacy()
    {

    }
}