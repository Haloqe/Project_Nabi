using UnityEngine;

public class AttackBase_Range : AttackBase
{
    private Legacy_Range _activeLegacy;
    private SDamageInfo _comboDamage;
    private SDamageInfo _comboDamage_Init;
    [SerializeField] private GameObject _bullet;

    public override void Reset()
    {
        base.Reset();
        _activeLegacy = null;
        _comboDamage = _comboDamage_Init;
    }

    public override void Attack()
    {
        Debug.Log("AttackBase_Range");
        _animator.SetInteger("AttackIndex", (int)ELegacyType.Range);
    }
}