using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GravityField : MonoBehaviour
{
    public AttackBase_Area Owner;
    public int pullDelay = 0;
    public int pullDuration = 5;

    // Collider 
    private List<int> _affectedEnemies;

    private void Start()
    {
        _affectedEnemies = new List<int>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        var enemyRoot = collision.transform.root.gameObject;
        IDamageable target = enemyRoot.GetComponent<IDamageable>();
        if (Utility.IsObjectInList(enemyRoot, _affectedEnemies)) return;

        if (target != null)
        {
            Owner.DealDamage(target, true);
            _affectedEnemies.Add(enemyRoot.GetInstanceID());
        }
    }

}
