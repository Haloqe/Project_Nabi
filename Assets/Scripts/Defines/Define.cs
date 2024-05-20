using System.Collections.Generic;
using UnityEngine;

public static class Define
{
    // TODO 외부 파일로 뺄 것
    public static ELocalisation Localisation = ELocalisation.KOR;
    
    public static float GameOverDelayTime = 3.7f;
    public static float HoldInteractionTime = 2.0f;
    public static float DefaultDamageTick = 0.5f;
    // Cumulative
    public static float[] LegacyAppearanceByPreservation = new float[]
    {
        0.6f,           // Weathered
        0.6f + 0.2f,    // Tarnished
        0.8f + 0.15f,   // Intact
        0.95f + 0.05f,  // Pristine
    };
    public static int[] LegacyPriceByRarity = new int[]
    {
        1,              // Weathered
        4,              // Tarnished
        8,              // Intact
        10,             // Pristine
    };
    
    public static string[,] WarriorNames = new string[,]
    {
        { "Sommer", "Euphoria", "Turbela", "NightShade" },
        { "소머", "유포리아", "투르벨라", "나이트셰이드" },
    };
    
    public static EStatusEffect[,] StatusEffectByWarrior =
    {
        { EStatusEffect.Sommer, EStatusEffect.Sommer },   // 소머
        { EStatusEffect.Ecstasy, EStatusEffect.Ecstasy }, // 유포리아
        { EStatusEffect.Swarm, EStatusEffect.Cloud },     // 투르벨라
        //{ EStatusEffect.Poison, EStatusEffect.Leech },  // 버논
        { EStatusEffect.Evade, EStatusEffect.Evade },     // 나이트셰이드
    };

    // Enemy Commons
    public static float SleepDuration = 5f;

    public static Color[] WarriorMainColours = new Color[]
    {
        new Color(0.3686275f, 0.5727299f, 0.5960785f, 1f),
        new Color(0.8962264f, 0.1070962f, 0.3534198f, 1f),
        new Color(0.5811201f, 0.3677168f, 0.6320754f, 1f),
        new Color(0.0868f, 0.0468f, 0.390f, 1f),
    };
    public static string[,] AttackTypeNames = new string[,]
    {
        { "Melee attack", "Ranged attack", "Dash attack", "Area attack" },
        { "근접 공격", "원거리 공격", "대시 공격", "폭탄 공격" },
    };
    public static string[,] LegacyPreservationNames = new string[,]
    {
        { "Weathered", "Tarnished", "Intact", "Pristine" },
        { "닳은", "빛바랜", "온전한", "완벽한" },
    };
    public static Color[] LegacyPreservationColors = new Color[]
    {
        new Color(0.6981132f, 0.4484543f, 0.05049241f, 1f),
        new Color(0.5499584f, 0.581f, 0.584f, 1f),
        new Color(0.8584906f, 0.73f, 0.37f, 1f),
        new Color(0.663f, 0.89f, 0.941f, 1f),
    };
    public static string[,] StatNames =
    {
        {"Health", "Strength", "Critical Chance", "Armour", "Armour Penetration", "Evasion Rate", "Heal Efficiency"},
        {"체력", "공격력", "치명타 확률", "방어력", "방어 관통력", "회피율", "회복 효율"},
    };

    // Legacy related
    public static float[] EuphoriaEnemyGoldDropBuffStats = new float[5];
    public static float[] SommerHypHallucinationStats = new float[5];
    public static float[] SommerSleepArmourReduceAmounts = new float[5];
    public static float[] TurbelaMaxButterflyStats = { 6, 6, 6, 6, 6 };
    public static float[] TurbelaDoubleSpawnStats = new float[5];
    public static float[] TurbelaExtraDamageStats = new float[5];
    public static float[] TurbelaButterflyCritStats = new float[5];
    public static float[] NightShadeLeechStats = new float[5];
    public static float[][] EcstasyBuffStats =
    {
        new[] {0.5f, 0.75f, 1.0f, 1.25f}, // 사마귀 - 방어력 % 
        new[] {0.05f, 0.1f, 0.15f, 0.25f}, // 식충 - 원거리 공격의 공격력
    };
    public static float[][] EcstasyUpgradedBuffStats =
    {
        new[] {0.5f, 0.75f, 1.0f, 1.25f},  // 사마귀 - 방어력 % 
        new[] {0.05f, 0.1f, 0.15f, 0.25f}, // 식충 - 원거리 공격의 공격력
    };
}