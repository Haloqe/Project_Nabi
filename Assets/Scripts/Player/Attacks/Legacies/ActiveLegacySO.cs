using System.Linq;
using UnityEngine;

public abstract class ActiveLegacySO : LegacySO
{
    protected Transform _playerTransform;
    protected float _spawnScaleMultiplier = 1.0f;

    [Space(10)] [Header("Damage")] 
    [NamedArray(typeof(ELegacyPreservation))] public float[] damageMultipliers = new float[4];        // 공격의 기본 데미지 multiplier
    [NamedArray(typeof(ELegacyPreservation))] public SDamageInfo[] extraDamages = new SDamageInfo[4]; // 공격의 기본 데미지 외에 추가로 가할 데미지

    [Space(10)][Header("Status Effects")] 
    [NamedArray(typeof(ELegacyPreservation))] public StatusEffectInfo[] StatusEffects;
    [NamedArray(typeof(ELegacyPreservation))] public StatusEffectInfo[] ExtraStatusEffects;
    
    public override void SetWarrior(EWarrior warrior)
    {
        this.warrior = warrior;
        var effect = PlayerAttackManager.Instance.GetWarriorStatusEffect(warrior, 0);
        StatusEffects = Enumerable.Repeat(new StatusEffectInfo(effect), 4).ToArray();
        ExtraStatusEffects = new StatusEffectInfo[4];
    }
    
    public virtual void Init(Transform playerTransform)
    {
        _playerTransform = playerTransform;
        _spawnScaleMultiplier = 1.0f;
    }
    
    public void UpdateSpawnSize(EIncreaseMethod method, float increaseAmount)
    {
        _spawnScaleMultiplier = Utility.GetChangedValue(_spawnScaleMultiplier, increaseAmount, method);
    }
}
