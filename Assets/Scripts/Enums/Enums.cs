using System;

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

#region Ability
public enum EAbilityMetalType
{
    Copper,
    Gold,
    None,
}

public enum EAbilityType // AbilityType이 좀 모호한듯도 한데 괜찮은 이름 있으면 바꿔줘.. 괜찮으면 냅두고.
{
    Passive,
    Active,
    None,
}

public enum EAbilityState
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

public enum EAbilityRarity
{
    Common,
    Rare,
    Epic,
    Legendary,
}

[Flags] 
public enum EAbilityObtainMethod
{
    None    = 0,
    Field   = 1,
    Store   = 2,
    Monster = 4,
}
#endregion Ability

#region Enemy
public enum EEnemyMoveType
{
    None,
    FollowStraightPath,
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