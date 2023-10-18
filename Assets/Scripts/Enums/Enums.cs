using System;

#region Common
public enum EDamageType
{
    Base,
    Light,
    Fire,
}

public enum EStatusEffect
{
    None,
    Blind,
    Stun,
    Slow,
    Airborne,
    Silence,
    Root,
}
#endregion Common

#region Ability
public enum EAbilityMetalType
{
    Copper,
    Gold,
    None,
}
public enum EAbilityType // AbilityTypeÀÌ Á» ¸ğÈ£ÇÑµíµµ ÇÑµ¥ ±¦ÂúÀº ÀÌ¸§ ÀÖÀ¸¸é ¹Ù²ãÁà.. ±¦ÂúÀ¸¸é ³ÀµÎ°í.
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
