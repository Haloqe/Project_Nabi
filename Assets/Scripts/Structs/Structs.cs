using System.Collections.Generic;

#region Common
public struct SStatusEffect
{
    public EStatusEffect Effect;
    public float Strength;
    public float Duration; // zero if continuous, auto-removed skill (e.g. 장판스킬)

    public SStatusEffect(EStatusEffect type)
    {
        Effect = type;
        Strength = 1;
        Duration = 0;
    }
    public SStatusEffect(EStatusEffect type, float strength)
    {
        Effect = type;
        Strength = strength;
        Duration = 0;
    }
    public SStatusEffect(EStatusEffect type, float strength, float duration)
    {
        Effect = type;
        Strength = strength;
        Duration = duration;
    }
}

public struct SDamage
{
    public EDamageType Type;
    public float Amount;
    public float Duration; // zero if one-shot damage
    // public float Interval; // 너무 복잡해져서 도트뎀은 0.5초 tick으로 통일

    public SDamage(EDamageType type, float amount)
    {
        Type = type;
        Amount = amount;
        Duration = 0;
    }
    public SDamage(EDamageType type, float amount, float duration)
    {
        Type = type;
        Amount = amount;
        Duration = duration;
    }
}

public struct SDamageInfo
{
    public int DamageSource;
    public EAbilityMetalType AbilityMetalType; // only for player's damage
    public List<SDamage> Damages;
    public List<SStatusEffect> StatusEffects;
}
#endregion Common

#region Player
public struct SAbilityData
{
    public bool ShouldOverride; // TEMP DEV PURPOSE ONLY

    public int Id;
    public string ClassName;
    public string Name_EN;
    public string Name_KO;
    public string Des_EN;
    public string Des_KO;
    public string IconPath;
    public string PrefabPath;
    public bool IsAttached;
    public EInteraction Interaction; // 이후 slowtap duration, hold duration 등 필요해지면 string 또는 separated field 변경

    public EAbilityType Type;
    public EAbilityMetalType MetalType;
    public float CoolDownTime;
    public float LifeTime;
    public SDamageInfo DamageInfo;
}
#endregion Player

#region Enemy
public struct SEnemyData
{
    public int Id;
    public string Name_EN;
    public string Name_KO;
    public string PrefabPath;
    public float DefaultMoveSpeed;
    public float MaxHealth;
    // amount of gold to drop
    // percentage of drop
    // rank
}
#endregion Enemy
