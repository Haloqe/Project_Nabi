using System.Collections.Generic;
using UnityEngine;

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
    public float TotalAmount;
    public float Duration; // zero if one-shot damage
    public float Tick; // the dot damage tick

    public SDamage(EDamageType type, float amount)
    {
        Type = type;
        TotalAmount = amount;
        Duration = 0;
        Tick = 0;
    }
    public SDamage(EDamageType type, float amount, float duration)
    {
        Type = type;
        TotalAmount = amount;
        Duration = 0;
        Tick = Define.DefaultDamageTick;
    }
    public SDamage(EDamageType type, float amount, float duration, float interval)
    {
        Type = type;
        TotalAmount = amount;
        Duration = duration;
        Tick = interval;
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
    public int IconIndex;
    public string PrefabPath;
    public bool IsAttached;
    public EInteraction Interaction; // 이후 slowtap duration, hold duration 등 필요해지면 string 또는 separated field 변경

    public EAbilityType Type;
    public EAbilityMetalType MetalType;
    public float CooldownTime;
    public float LifeTime;
    public SDamageInfo DamageInfo;
}

public struct SRelicData
{
    public int AbilityId;
    public string Name_EN;
    public string Name_KO;
    public string Des_EN;
    public string Des_KO;
    public int SpriteIndex;
    public EAbilityObtainMethod ObtainMethod;
    public EAbilityRarity Rarity;
    public int[] Locations;
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

#region LevelGeneration
public struct SDoorInfo
{
    public EConnectionType ConnectionType;
    /// <summary>
    /// The bottom left world position of the door
    /// </summary>
    public Vector3Int Position;
    public EDoorDirection Direction;

    public SDoorInfo(EConnectionType type, EDoorDirection dir, Vector3Int pos)
    {
        ConnectionType = type;
        Direction = dir;
        Position = pos;
    }
}

public struct SRoomInfo
{
    public int PrevRoomID;
    public int RoomID; 

    public SRoomInfo(int prevRoomID, int roomID)
    {
        PrevRoomID = prevRoomID;
        RoomID = roomID;
    }
}
#endregion LevelGeneration