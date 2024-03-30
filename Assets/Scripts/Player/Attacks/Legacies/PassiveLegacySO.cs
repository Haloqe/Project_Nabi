using UnityEngine;
using UnityEngine.Serialization;

public class PassiveLegacySO : LegacySO
{
    public EBuffType BuffType;
    
    // If BuffType == StatusEffectUpgrade
    // public SLegacyStatusEffectUpgradeData[] StatusEffectUpgrade = new SLegacyStatusEffectUpgradeData[1]{new SLegacyStatusEffectUpgradeData()};
    
    // If BuffType == StatUpgrade
    public SLegacyStatUpgradeData StatUpgradeData;
    
    // If BuffType == EnemyItemDropRate || EnemyGoldDropRate || SpawnAreaIncrease || FoodHealEfficiency
    public EIncreaseMethod BuffIncreaseMethod;
    [NamedArray(typeof(ELegacyPreservation))]
    public float[] BuffIncreaseAmounts = new float[4]{0,0,0,0};
    
    public virtual void Init() { }
}