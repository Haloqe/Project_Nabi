using Player.Abilities.Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidasTouch : ActiveAbilityBase
{
    private List<int> _affectedEnemies;
    private int _maxTargetNum;

    protected override void Initialise()
    {
        _affectedEnemies = new List<int>();
        _maxTargetNum = 1;
    }

    protected override void ActivateAbility()
    {
        _affectedEnemies.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") == false)
        {
            Debug.LogError(_data.Name_EN + " trigger with non-enemy");
            return;
        }
        if (_affectedEnemies.Count >= _maxTargetNum) return;
        if (Utility.IsObjectInList(collision.gameObject, _affectedEnemies)) return;

        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target == null) return;

        _affectedEnemies.Add(collision.gameObject.GetInstanceID());
        //상태이상 후 상태이상 풀릴때 데미지 들어가는 스킬
        //TO-DO: 상태이상 구현
        //상태이상 후 데미지
        _owner.DealDamage(target, _data.DamageInfo);
    }
}
