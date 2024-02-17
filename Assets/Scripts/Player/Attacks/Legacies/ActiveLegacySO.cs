using UnityEngine;
using UnityEngine.Serialization;

public abstract class ActiveLegacySO : LegacySO
{
    protected ELegacyPreservation _preservation;
    protected Transform _playerTransform;
    protected float _spawnScaleMultiplier = 1.0f;

    [Space(10)][Header("Damage")] 
    public EIncreaseMethod DamageIncreaseMethod;
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        AdditionalDamage = new float[4]{1,1,1,1};
    
    [Space(10)][Header("Default Status Effect")]
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        StatusEffectDurations = new float[4]{0,0,0,0};
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        StatusEffectStrengths = new float[4]{0,0,0,0};
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        StatusEffectProbabilites = new float[4]{0,0,0,0};
    
    [Space(10)][Header("Additional Status Effect")]
    public EStatusEffect ExtraStatusEffect;
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        ExtraStatusEffectDurations = new float[4]{0,0,0,0};
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        ExtraStatusEffectStrengths = new float[4]{0,0,0,0};
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        ExtraStatusEffectProbabilites = new float[4]{0,0,0,0};
    
    public virtual void Init(Transform playerTransform) { }
    public abstract void OnUpdateStatusEffect(EStatusEffect newEffect);
    public void SetPreservation(ELegacyPreservation preservation)
    {
        _preservation = preservation;
    }
    
    public void UpdateSpawnSize(EIncreaseMethod method, float increaseAmount)
    {
        _spawnScaleMultiplier = Utility.GetChangedValue(_spawnScaleMultiplier, increaseAmount, method);
    }
}