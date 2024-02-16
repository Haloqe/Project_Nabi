using UnityEngine;
using UnityEngine.Serialization;

public class PassiveLegacySO : LegacySO
{
    public EBuffType BuffType;
    
    // If BuffType == StatusEffectUpgrade
    public SLegacyStatusEffectUpgradeData[] StatusEffectUpgrades;
    
    // If BuffType == StatUpgrade
    public SLegacyStatUpgradeData[] StatUpgrades;
    
    // If BuffType == EnemyItemDropRate || EnemyGoldDropRate || SpawnAreaIncrease
    public EIncreaseMethod IncreaseMethod;
    [NamedArray(typeof(ELegacyPreservation))] 
    public float[] IncreaseAmounts = new float[4];
    
    public virtual void Init() { }
}