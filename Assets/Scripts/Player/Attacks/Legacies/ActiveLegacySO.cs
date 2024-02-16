using UnityEngine;
using UnityEngine.Serialization;

public abstract class ActiveLegacySO : LegacySO
{
    protected Transform _playerTransform;
    
    [Header("Warrior Base Damage")]
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        BaseDamageMultipliers = new float[4]{1,1,1,1};
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        StatusEffectDurations = new float[4]{0,0,0,0};
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        StatusEffectStrengths = new float[4]{0,0,0,0};
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        StatusEffectProbabilites = new float[4]{0,0,0,0};
    
    [Header("Additional Status Effect")]
    public EStatusEffect ExtraStatusEffect;
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        ExtraStatusEffectDurations = new float[4]{0,0,0,0};
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        ExtraStatusEffectStrengths = new float[4]{0,0,0,0};
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        ExtraStatusEffectProbabilites = new float[4]{0,0,0,0};
    
    public virtual void Init(Transform playerTransform) { }
    public abstract void OnUpdateStatusEffect(EStatusEffect newEffect);
}