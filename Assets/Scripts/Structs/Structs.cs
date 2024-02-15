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
    public List<SDamage> Damages;
    public List<SStatusEffect> StatusEffects;
}
#endregion Common

#region Player
public struct SWarrior
{
    public List<string> Names;
    public EStatusEffect[] Effects;
}
public struct SLegacyData
{
    public int ID;
    public List<string> Names;
    public List<string> Descs;
    public int IconIndex;
    public EWarrior Warrior;
    public ELegacyType Type;
    public int[] PrerequisiteIDs;
    //EItemObtainMethod ObtainMethod;
}

//public struct SRelicData
//{
//    public int AbilityId;
//    public string Name_EN;
//    public string Name_KO;
//    public string Des_EN;
//    public string Des_KO;
//    public int SpriteIndex;
//    public EItemObtainMethod ObtainMethod;
//    public EAbilityRarity Rarity;
//    public int[] Locations;
//}
#endregion Player

#region Enemy
public struct SEnemyData
{
    public int ID;
    public string Name;
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

#region Item
public struct SFoodInfo
{
    public string Name;
    public string Description;
    public float HealthPoint;
    public float Price;
    public int SpriteIndex;
}
#endregion
