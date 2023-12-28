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

    public virtual void Attack()
    {
        _animator.SetInteger("AttackIndex", (int)_attackType);

        // Play VFX
        if (_vfxObject == null) return;
        if (_isAttached)
        {

        }
        else
        {
            float dir = Mathf.Sign(gameObject.transform.localScale.x);
            Vector3 playerPos = gameObject.transform.position;
            Vector3 vfxPos = _vfxObject.transform.position;
            Vector3 position = new Vector3(playerPos.x + dir * (vfxPos.x), playerPos.y + vfxPos.y, playerPos.z + vfxPos.z);

            _vfxObject.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
            Instantiate(_vfxObject, position, Quaternion.identity);
        }
    }

    public IEnumerator AttackPostDelayCorountine()
    {
        yield return new WaitForSeconds(_attackPostDelay);
        _playerAttack.OnAttackEnd_Post();
    }

    public virtual void IsActiveBound()
    {

    }

    public virtual void BindLegacy()
    {

    }
}