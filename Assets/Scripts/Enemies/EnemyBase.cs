using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable, IDamageDealer
{
    protected Rigidbody2D _rigidbody2D;
    protected Animator _animator;
    protected SEnemyData _data;

    protected virtual void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    public void TakeDamage(SDamageInfo damageInfo)
    {
        Utility.PrintDamageInfo(gameObject.name, damageInfo);
    }
}
