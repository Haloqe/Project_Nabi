using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageDealer
{
    GameObject gameObject { get; }

    public virtual void DealDamage(IDamageable target, SDamageInfo damageInfo)
    {
        damageInfo.DamageSource = gameObject.GetInstanceID();
        target.TakeDamage(damageInfo);
    }
}
