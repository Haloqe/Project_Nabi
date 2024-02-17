using System.Collections.Generic;
using UnityEngine;

public class AttackSpawnObject : MonoBehaviour
{
    public PlayerDamageDealer PlayerDamageDealer;
    private SDamageInfo _damageInfo;
    public bool IsAttached;
    public bool ShouldManuallyDestroy;

    [Space(10)] [Header("Damage")] 
    public bool ShouldInflictDamage;
    public EDamageType DamageType;
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        DamageAmounts = new float[4]{0,0,0,0};
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        DamageDurations = new float[4]{0,0,0,0}; // zero if one-shot damage

    [Space(10)] [Header("Status Effect")] 
    public bool ShouldInflictStatusEffect;
    public EStatusEffect StatusEffect;
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        StatusEffectDurations = new float[4]{0,0,0,0};
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        StatusEffectStrengths = new float[4]{0,0,0,0};

    public void SetPreservation(ELegacyPreservation preservation)
    {
        _damageInfo = new SDamageInfo()
        {
            Damages = new List<SDamage>(), StatusEffects = new List<SStatusEffect>()
        };
        
        if (ShouldInflictDamage)
        {
            _damageInfo.Damages = new List<SDamage>
            {
                new SDamage(DamageType, DamageAmounts[(int)preservation], DamageDurations[(int)preservation])
            };
        }
        if (ShouldInflictStatusEffect)
        {
            _damageInfo.StatusEffects = new List<SStatusEffect>
            {
                new SStatusEffect(StatusEffect, StatusEffectStrengths[(int)preservation], StatusEffectDurations[(int)preservation])
            };
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target = other.gameObject.GetComponent<IDamageable>();
        if (target != null && (ShouldInflictDamage || ShouldInflictStatusEffect))
        {
            PlayerDamageDealer.DealDamage(target, _damageInfo);
        }
    }
}
