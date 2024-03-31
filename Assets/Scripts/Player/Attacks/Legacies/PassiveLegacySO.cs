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
    [NamedArray(typeof(ELegacyPreservation))]
    public float[] Stats = new float[5]{0,0,0,0,0};
    public bool SavedInDefine = false;
    
    public virtual void Init() { }
}