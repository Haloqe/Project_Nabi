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
    //public EIncreaseMethod IncreaseMethod;
    [NamedArray(typeof(ELegacyPreservation))] public float[] IncreaseAmounts;

    public bool isMultiplier;
    public bool HasUpdateCondition;
    public SCondition StatUpdateCondition;          // 스탯이 업데이트 되는 조건
    
    public bool HasApplyCondition;
    public SCondition UpgradeApplyCondition;        // 업데이트된 스탯이 적용되는 조건
    public bool UndoUpgradeWhenConditionNotMet;     // 조건에 부합하지 않을 경우 적용된 업그레이드를 캔슬할 것인가?
}

[Serializable]
public struct SCondition
{
    public EUpgradeFrequency updateFrequency;
    public ECondition condition;
    public EComparator comparator;
    public float targetValue;
    public bool isValueRatio;
}

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
    public float DefaultDamage;
    public float DefaultArmour;
    public float DefaultArmourPenetration;
    public string DamageType;
    public float IdleProbability;
    public float IdleAverageDuration;
    public float WalkAverageDuration;
    public float ChasePlayerDuration;
    public float DetectRangeX;
    public float DetectRangeY;
    public float AttackRangeX;
    public float AttackRangeY;
    public int MinGoldRange;
    public int MaxGoldRange;
    public int SoulShardDropAmount;
    public float SoulShardDropChance;
}
#endregion Enemy

#region LevelGeneration
// public struct SDoorInfo
// {
//     public EConnectionType ConnectionType;
//     public Vector3Int BLPosition_World; // The bottom left world position of the door
//     public EDoorDirection Direction;
//     public int MinSize;
//     public int MaxSize;
//
//     public SDoorInfo(EConnectionType type, EDoorDirection dir, Vector3Int pos, int minSize, int maxSize)
//     {
//         ConnectionType = type;
//         Direction = dir;
//         BLPosition_World = pos;
//         MinSize = minSize;
//         MaxSize = maxSize;
//     }
// }

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

public struct SFlowerInfo
{
    public string Name;
    public string Description;
    public int SpriteIndex;
}

public struct SBombInfo
{
    public string Name;
    public string Description;
    public int SpriteIndex;
}
#endregion
