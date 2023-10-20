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
        Initialise();
    }

    protected abstract void Initialise();


    public virtual void DealDamage(IDamageable target, SDamageInfo damageInfo)
    {
        damageInfo.DamageSource = gameObject.GetInstanceID();
        target.TakeDamage(damageInfo);
    }

    //-------------------------------------------------------------
    // Received Damage Handling
    public void TakeDamage(SDamageInfo damageInfo)
    {
        Utility.PrintDamageInfo(gameObject.name, damageInfo);
    }


}
