using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageDealer
{
    GameObject gameObject { get; }

    public abstract void DealDamage(IDamageable target, SDamageInfo damageInfo);
}
