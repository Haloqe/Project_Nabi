using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    abstract void TakeDamage(SDamageInfo damageInfo);
}
