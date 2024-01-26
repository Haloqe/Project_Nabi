using UnityEngine;

public abstract class LegacySO : ScriptableObject
{
    protected Transform _playerTransform;
    public int LegacyID;
    [NamedArray(typeof(ELegacyPreservation))] public float[] BaseDamageMultiplier = new float[4];
    [NamedArray(typeof(ELegacyPreservation))] public float[] StatusEffectDuration = new float[4];
    [NamedArray(typeof(ELegacyPreservation))] public float[] StatusEffectStrength = new float[4];
}