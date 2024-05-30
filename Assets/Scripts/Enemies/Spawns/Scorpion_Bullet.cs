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
        Rigidbody2D rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.velocity = direction.normalized * _speed;
        rigidBody.AddTorque(3f);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject target = collision.gameObject;
        DestroySelf(target);
    }
    
    private void DestroySelf(GameObject target)
    {
        Destroy(gameObject);
        if (target.CompareTag("Player"))
        {
            target.GetComponentInParent<IDamageable>().TakeDamage(_bulletAttackInfo);
            Instantiate(Resources.Load<GameObject>("Prefabs/Effects/ScorpionVFX/BulletHitPlayerVFX"),
                transform.position, Quaternion.identity);
            return;
        }

        Vector3 hitGroundAngle = transform.position.y > -6.5f
            ? new Vector3(0, 0, Mathf.Sign(transform.position.x) * 90f)
            : new Vector3(0, 0, 0);
        Vector3 hitGroundPosition = transform.position.y > -6.5f
            ? new Vector3(-Mathf.Sign(transform.position.x) * 1.8f, 0, 0)
            : new Vector3(0, 1.5f, 0);
        
        Instantiate(Resources.Load<GameObject>("Prefabs/Effects/ScorpionVFX/BulletHitGroundVFX"),
            transform.position + hitGroundPosition, 
            Quaternion.Euler(hitGroundAngle));
    }
}
