using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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

[Serializable]
public struct SLegacyStatusEffectUpgradeData
{
    public EStatusEffectUpgradeType UpgradeType;
    public EIncreaseMethod IncreaseMethod;
    public float[] IncreaseAmounts;
}

[Serializable]
public struct SLegacyStatUpgradeData
{
    public EStat Stat;
    public EIncreaseMethod IncreaseMethod;
    public float[] IncreaseAmounts;
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

[Serializable]
public struct SDamageInfo
{
    public float BaseDamage;
    [FormerlySerializedAs("StrengthRelativeDamage")] public float RelativeDamage;
}
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
    public int Price;
    public int SpriteIndex;
}
#endregion
