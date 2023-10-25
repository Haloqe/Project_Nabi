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
#endregion Ability

#region Enemy
public enum EEnemyMoveType
{
    None,
    FollowStraightPath,
}
#endregion Enemy