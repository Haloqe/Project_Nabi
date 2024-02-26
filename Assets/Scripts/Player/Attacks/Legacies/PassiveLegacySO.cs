using UnityEngine;
using UnityEngine.Serialization;

public class PassiveLegacySO : LegacySO
{
    public EBuffType BuffType;
    
    // If BuffType == StatusEffectUpgrade
    public SLegacyStatusEffectUpgradeData[] StatusEffectUpgrades = new SLegacyStatusEffectUpgradeData[1]{new SLegacyStatusEffectUpgradeData()};
    
    // If BuffType == StatUpgrade
    public SLegacyStatUpgradeData[] StatUpgrades = new SLegacyStatUpgradeData[1]{new SLegacyStatUpgradeData()};
    
    // If BuffType == EnemyItemDropRate || EnemyGoldDropRate || SpawnAreaIncrease || HealEfficiency_Food
    public EIncreaseMethod BuffIncreaseMethod;
    [NamedArray(typeof(ELegacyPreservation))]
    public float[] BuffIncreaseAmounts = new float[4]{0,0,0,0};
    
    public virtual void Init() { }
}