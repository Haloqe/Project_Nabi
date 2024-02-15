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
        1,      // Weathered
        4,      // Tarnished
        8,      // Intact
        10,     // Pristine
    };
    public static string[,] LegacyPreservationNames = new string[,]
    {
        { "Weathered", "Tarnished", "Intact", "Pristine" },
        { "닳은", "빛바랜", "온전한", "완벽한" },
    };
    public static Color[] LegacyPreservationColors = new Color[]
    {
        new Color(0.6981132f, 0.4484543f, 0.05049241f, 1f),
        new Color(0.5848541f, 0.764151f, 0.1057315f, 1f),
        new Color(0.4980392f, 0.772549f, 1.0f, 1f),
        new Color(1.0f, 0.8561112f, 1.0f, 1f),
    };
}