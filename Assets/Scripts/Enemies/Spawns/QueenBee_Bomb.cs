using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenBee_Bomb : MonoBehaviour
{
    private static float _baseDamage = 10f;
    private static float _poisonStrength = 2f;
    private static float _poisonDuration = 15f;
    private static DamageInfo _bombDamageInfo = new(EDamageType.Base, _baseDamage);
    private static List<StatusEffectInfo> _bombStatusEffectInfo =
        new() {
            new StatusEffectInfo(EStatusEffect.Poison, _poisonStrength, _poisonDuration)
        };
    private AttackInfo _bombAttackInfo = new(_bombDamageInfo, _bombStatusEffectInfo);

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.gameObject.GetComponentInParent<IDamageable>();
        target?.TakeDamage(_bombAttackInfo);
    }
}