using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MansaMusa : ActiveAbilityBase
{
    private List<int> _affectedEnemies = new List<int>();
    private int _maxTargetNum = 5;

    protected override void Initialise()
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
        _owner.DealDamage(target, _data.DamageInfo);
    }
}
