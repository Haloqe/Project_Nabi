using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageDealer
{
    public abstract void DealDamage(IDamageable target, SDamageInfo damageInfo);
}
