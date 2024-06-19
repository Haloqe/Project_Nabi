using System;
using UnityEngine;

#region Settings
public enum ELocalisation
{
    ENG,
    KOR,
    MAX,
}

public enum EAudioType
{
    Master,
    BGM,
    Enemy,
    Others,
    UI,
}
#endregion Settings

//--------------------------------------------------------------------------------

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

public enum EBuffs
{
    Fast,
    Heal,
    MAX,
}

public enum EStatusEffect
{
    None,
    
    // Base
    Sommer,
    Poison,
    Ecstasy,
    Swarm,
    Evade,

    // Upgraded
    [InspectorName(null)] Leech,
    [InspectorName(null)] Cloud,
    Camouflage,

    // Others
    Sleep,
    Blind,
    Stun,
    Slow,
    Airborne,
    Silence,
    Root,
    Pull,
    BuffButterfly,
    Weakness,
    GravityPull,

    [InspectorName(null)] MAX,
}
#endregion Common

//--------------------------------------------------------------------------------

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
    
    StatUpgrade,
    SpawnAreaIncrease,
    BindingSkillUpgrade,
    
    EuphoriaEnemyGoldDropBuff,      // 유포리아 - 사랑의 착취
    EuphoriaEcstasyUpgrade,         // 유포리아 - 뇌쇄술
            
    EnemyItemDropRate,              // 소머 - 무기력한 자장가
    SommerHypHallucination,         // 소머 - 입면 환각
            
    TurbelaMaxButterfly,            // 투르벨라 - 군락지 소환
    TurbelaDoubleSpawn,             // 투르벨라 - 집단 폭행
    TurbelaButterflyCrit,           // 투르벨라 - 탄막 폭격
    
    NightShadeFastChase,            // 나이트셰이드 - 완벽한 잠행
    NightShadeShadeBonus,           // 나이트셰이드 - 한밤의 신기루
    
    AttackDamageMultiply,           // 공격 타입별 데미지 추가
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

public enum EUpgradeFrequency
{
    OnceWhenConditionMet,
    RepeatedlyWhenConditionMet,
}

public enum ECondition
{
    None,
    SlayedEnemiesCount,
    PlayerHealth,
}

public enum EComparator
{
    IsLessThan,
    IsLessOrEqualTo,
    IsEqualTo,
    IsGreaterOrEqualTo,
    IsGreaterThan,
    IncreasesBy,
}

public enum EWarrior
{
    Sommer,
    Euphoria,
    Turbela,
    //Vernon,
    NightShade,
    
    [InspectorName(null)]MAX,
}

public enum ETiming
{
    Before,
    After,
    With,
}
#endregion Legacy

//--------------------------------------------------------------------------------

#region Player
public enum EPlayerAttackType
{
    Melee_Base,
    Melee_Combo,
    Ranged,
    Dash,
    Area,
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

public enum EStat
{
    Health,
    Strength,
    CriticalRate,
    Armour,
    ArmourPenetration,
    EvasionRate,
    HealEfficiency,
}

public enum EArborType
{
    Default,
    Curiosity,
    Despair,
    Serenity,
    Regret,
    Paranoia,
}

public enum ETensionState
{
    Innate,
    Overheated,
    Overloaded,
    Recovery,
}

public enum EMetaUpgrade
{
    BetterLegacyPreserv,
    HealthAddition,
    CritRateAddition,
    Resurrection,
    StrengthAddition,
}
#endregion

//--------------------------------------------------------------------------------

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

[Flags] 
public enum EItemObtainMethod
{
    Field   = 1,
    Store   = 2,
    Monster = 4,
    Boss    = 8,
}
#endregion

//--------------------------------------------------------------------------------
#region Enemy
public enum EEnemyName
{
    VoidMantis,
    Insectivore,
    Bee,
    SpiderA,
    QueenBee,
    Scorpion,
}
public enum EEnemyMoveType
{
    None,
    FollowStraightPath,
    LinearPath,
    Stationary,
    Flight,
    SpiderA,
    QueenBee,
    Scorpion,
}

public enum EEnemyState
{
    Idle,
    Walk,
    Chase,
    Telegraph,
    AttackSequence,
    AttackEnd,
}
#endregion Enemy

#region Interaction
public enum EPortalType
{
    CombatToSecret,
    SecretToCombat,
    MetaToCombat,
    CombatToBoss,
    CombatToMidBoss,
    MidBossToCombat,
}

public enum EFlowerType
{
    NectarFlower,       // 회복
    IncendiaryFlower,   // 화염
    StickyFlower,       // 끈적
    BlizzardFlower,     // 한랭
    GravityFlower,      // 중력
    MAX,
};
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
    MidBoss,
    Boss,
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

public enum EEnemyType
{
    VoidMantis,     // 0
    Insectivore,    // 1
    Bee,            // 2
    Spider,         // 3
    QueenBee,       // 4
}

public enum ESceneType
{
    MainMenu,
    CombatMap0,
    CombatMap1,
    DebugCombatMap,
    MidBoss,
    Boss,
    Tutorial,
}
#endregion

public enum ECutSceneType
{
    IntroPreTutorial,
    IntroPostTutorial,
}