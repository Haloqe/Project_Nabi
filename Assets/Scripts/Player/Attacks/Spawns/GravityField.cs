using System.Collections.Generic;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    public AttackBase_Area Owner;
    public Vector3 GravCorePos;
    private AttackInfo _attackInfo;

    // Collider 
    private List<int> _affectedEnemies;

    private void Start()
    {
        _affectedEnemies = new List<int>();
        _attackInfo = new AttackInfo
        {
            Damage = null,
            StatusEffects = { new StatusEffectInfo(EStatusEffect.GravityPull, 75, 5) },
            ShouldUpdateTension = false,
            GravCorePosition = GravCorePos,
        };
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the hit target is valid
        var rootEnemyDamageable = collision.GetComponentInParent<IDamageable>();
        if (rootEnemyDamageable == null || Utility.IsObjectInList(rootEnemyDamageable.GetGameObject(), _affectedEnemies)) return;

        // Do damage
        Owner.DealDamage(rootEnemyDamageable, _attackInfo);
        _affectedEnemies.Add(rootEnemyDamageable.GetGameObject().GetInstanceID());
    }

}
