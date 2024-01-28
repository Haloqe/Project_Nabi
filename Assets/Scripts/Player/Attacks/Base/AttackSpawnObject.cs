using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AttackSpawnObject : MonoBehaviour
{
    public PlayerDamageDealer PlayerDamageDealer;
    private SDamageInfo _damageInfo;
    
    public EDamageType DamageType;
    public float TotalAmount;
    public float DamageDuration; // zero if one-shot damage
    
    public EStatusEffect StatusEffect;
    public float StatusEffectDuration;
    public float Strength;

    private void Awake()
    {
        _damageInfo = new SDamageInfo
        {
            Damages = new List<SDamage> { new SDamage(DamageType, TotalAmount, DamageDuration) },
            StatusEffects = new List<SStatusEffect> {new SStatusEffect(StatusEffect, Strength, StatusEffectDuration)},
        };
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target = other.gameObject.GetComponent<IDamageable>();
        if (target != null)
        {
            Debug.Log("damage");
            PlayerDamageDealer.DealDamage(target, _damageInfo);
        }
    }
}
