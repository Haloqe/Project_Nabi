using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorpion_Bullet : MonoBehaviour
{
    private float _speed = 20f;
    private static float _bulletDamage = 5f;
    private AttackInfo _bulletAttackInfo = new(new DamageInfo(EDamageType.Base, _bulletDamage));
    
    public void Shoot(Vector3 direction)
    {
        GetComponent<Rigidbody2D>().velocity = direction.normalized * _speed;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.gameObject.GetComponentInParent<IDamageable>();
        DestroySelf(target);
    }
    
    private void DestroySelf(IDamageable target)
    {
        Destroy(gameObject);
        target?.TakeDamage(_bulletAttackInfo);
    }
}
