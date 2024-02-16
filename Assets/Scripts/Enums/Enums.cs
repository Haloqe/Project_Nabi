using System;
using UnityEngine;

#region Settings
public enum ELocalisation
{
    ENG,
    KOR,
    MAX,
}
#endregion Settings

#region Common
public enum EDamageType
{
    Base,
    Light,
    Fire,
    Poison,
    Stench, // TODO change name
    Electric,
    MAX,
}

public enum EStatusEffect
{
    None,
    
    // Base
    Sommer,
    Drowsiness,
    Poison,
    Drugged,
    Swarm,
    Evade,

    // Upgraded
    Sleep,
    Leech,
    Indoctrinated,
    Cloud,
    Camouflage,

    // Others
    Blind,
    Stun,
    Slow,
    Airborne,
    Silence,
    Root,
    Pull,

    MAX,
}
#endregion Common

#region Legacy
public enum ELegacyType
{
    Melee,
    Ranged,
    Dash,
    [InspectorName(null)]Area,
    [InspectorName(null)]Passive,
    [InspectorName(null)]MAX,
}

public enum EPlayerAttackType
{
    Melee_Base,
    Melee_Combo,
    Ranged,
    Dash,
    MAX,
}

public enum EAttackState
{
    Ready,
    Active,
    Cooldown,
} 

public enum EInteraction
{
    Press,
    Hold,
    None,
}

public enum ELegacyPreservation
{
    Weathered,  // 닳은
    Tarnished,  // 빛바랜
    Intact,     // 온전한
    Pristine,   // 완벽한
    
    [InspectorName(null)]MAX,
}

public enum EBuffType
{
    None,
    StatusEffectUpgrade,    
    EnemyItemDropRate,
    EnemyGoldDropRate,
    SpawnAreaIncrease,
    StatUpgrade,
}

public enum EStatusEffectUpgradeType
{
    Duration,
    Strength,
    Probability,
}

public enum EIncreaseMethod
{
    Constant,
    Percent,
    PercentPoint,
}

public enum EStat
{
    Health,
    Strength,
    CriticalChance,
    Armour,
    ArmourPenetration,
}

public enum EWarrior
{
    Sommer,
    Euphoria,
    Turbela,
    Vernon,
    Shade,
    
    [InspectorName(null)]MAX,
}

public enum ETiming
{
    Before,
    After,
    With,
}

[Flags] 
public enum EItemObtainMethod
{
    Field   = 1,
    Store   = 2,
    Monster = 4,
    Boss    = 8,
}
#endregion Legacy

#region Item
public enum ERarity
{
    Undefined,
    Common, 
    Uncommon,
    Rare,
    Epic,
    Legendary,
}
#endregion

#region Enemy
public enum EEnemyMoveType
{
    None,
    FollowStraightPath,
    LinearPath,
}
#endregion Enemy

#region Interaction
public enum EPortalType
{
    GeneralStore,
    FoodStore,
    NextLevel,
}
#endregion Interaction

#region LevelGeneration
public enum ERoomType
{
    Normal,
    Corridor,
    Entrance,
    Teleport,
    Treasure,
    Shop,
    MAX,
}

public enum EConnectionType
{
    Horizontal,
    Vertical,
}

public enum ECellType
{
    Empty,
    Filled,
    ToBeFilled,
}

public enum ETilemapType
{
    Wall,
    Collideable,
    Background,
    Other1,
    Other2,
    Other3,
    MAX,
}

public enum EDoorDirection
{
    Up,
    Down,
    Left,
    Right,
}
#endregion