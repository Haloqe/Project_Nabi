using System.Linq;
using UnityEngine;

public abstract class ActiveLegacySO : LegacySO
{
    protected PlayerDamageDealer _playerDamageDealer;
    protected Transform _playerTransform;
    protected float _spawnScaleMultiplier = 1.0f;

    [NamedArray(typeof(ELegacyPreservation))] public float[] damageMultipliers = new float[4];        // 공격의 기본 데미지 multiplier
    [NamedArray(typeof(ELegacyPreservation))] public SDamageInfo[] extraDamages = new SDamageInfo[4]; // 공격의 기본 데미지 외에 추가로 가할 데미지

    [NamedArray(typeof(ELegacyPreservation))] public StatusEffectInfo[] StatusEffects;
    [NamedArray(typeof(ELegacyPreservation))] public StatusEffectInfo[] ExtraStatusEffects;
    
    public override void SetWarrior(EWarrior warrior)
    {
        this.warrior = warrior;
    }
    
    public virtual void Init(Transform playerTransform)
    {
        _playerTransform = playerTransform;
        _spawnScaleMultiplier = 1.0f;
        _playerDamageDealer = PlayerController.Instance.playerDamageDealer;
    }
    
    public void UpdateSpawnSize(EIncreaseMethod method, float increaseAmount)
    {
        _spawnScaleMultiplier = Utility.GetChangedValue(_spawnScaleMultiplier, increaseAmount, method);
    }
}
