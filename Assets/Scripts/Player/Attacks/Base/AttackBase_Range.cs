using UnityEngine;

public class AttackBase_Range : AttackBase
{
    private Legacy_Range _activeLegacy;
    private SDamageInfo _comboDamage;
    private SDamageInfo _comboDamage_Init;
    [SerializeField] private Transform _firePos;
    [SerializeField] private GameObject _bullet;


    public override void Reset()
    {
        base.Reset();
        _activeLegacy = null;
        _comboDamage = _comboDamage_Init;
    }

    public override void Attack()
    {
        _animator.SetInteger("AttackIndex", (int)ELegacyType.Range);
    }

    public void Fire()
    {
        var bullet = Instantiate(_bullet, _firePos.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().Direction = -Mathf.Sign(gameObject.transform.localScale.x);
    }
}