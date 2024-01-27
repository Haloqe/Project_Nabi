using UnityEngine;
using UnityEngine.Serialization;

public abstract class LegacySO : ScriptableObject
{
    public EWarrior Warrior;
    public Transform PlayerTransform;
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        BaseDamageMultiplier = new float[4]{1,1,1,1};
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        StatusEffectDuration = new float[4]{0,0,0,0};
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        StatusEffectStrength = new float[4]{0,0,0,0};
}