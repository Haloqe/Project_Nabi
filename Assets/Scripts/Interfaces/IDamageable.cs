using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    abstract void TakeDamage(AttackInfo damageInfo);

    public static float CalculateRoughDamage(List<DamageInfo> damages)
    {
        if (damages == null) return 0;

        float total = 0; float damageCount = 0;
        foreach (var damage in damages)
        {
            damageCount = damage.Duration == 0 ? 1 : (damage.Duration / damage.Tick + 1);
            total += (damage.TotalAmount * damageCount);
        }

        return total;
    }
}
