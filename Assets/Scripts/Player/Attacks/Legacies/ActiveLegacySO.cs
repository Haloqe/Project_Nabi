using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class ActiveLegacySO : LegacySO
{
    protected ELegacyPreservation _preservation;
    protected Transform _playerTransform;
    protected float _spawnScaleMultiplier = 1.0f;

    [Space(10)][Header("Damage")] 
    [NamedArray(typeof(ELegacyPreservation))] public SDamageInfo[] 
        DamageInfos = new SDamageInfo[4];

    [Space(10)][Header("Status Effects")] 
    [NamedArray(typeof(ELegacyPreservation))]
    public StatusEffectInfo[] StatusEffects;
    [NamedArray(typeof(ELegacyPreservation))]
    public StatusEffectInfo[] ExtraStatusEffects;
    
    public override void SetWarrior(EWarrior warrior)
    {
        Warrior = warrior;
        var effect = PlayerAttackManager.Instance.GetWarriorStatusEffect(Warrior, 0);
        StatusEffects = Enumerable.Repeat(new StatusEffectInfo(effect), 4).ToArray();
        ExtraStatusEffects = new StatusEffectInfo[4];
    }
    
    public virtual void Init(Transform playerTransform)
    {
        _playerTransform = playerTransform;
        _spawnScaleMultiplier = 1.0f;
    }
    
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
