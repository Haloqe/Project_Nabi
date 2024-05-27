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
        Debug.Log("collided with enemies");
        IDamageable target = collision.gameObject.GetComponentInParent<IDamageable>();
        if (Utility.IsObjectInList(target.GetGameObject(), _affectedEnemies)) return;

        if (target != null)
        {
            Owner.DealDamage(target, true);
            _affectedEnemies.Add(collision.gameObject.GetInstanceID());
        }
    }

}
