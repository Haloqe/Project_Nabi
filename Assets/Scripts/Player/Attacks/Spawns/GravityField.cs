using System.Collections.Generic;
using UnityEngine;

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
        // Check if the hit target is valid
        var rootEnemyDamageable = collision.GetComponentInParent<IDamageable>();
        if (rootEnemyDamageable == null || Utility.IsObjectInList(rootEnemyDamageable.GetGameObject(), _affectedEnemies)) return;

        // Do damage
        Owner.DealDamage(rootEnemyDamageable, false);
        _affectedEnemies.Add(rootEnemyDamageable.GetGameObject().GetInstanceID());
    }

}
