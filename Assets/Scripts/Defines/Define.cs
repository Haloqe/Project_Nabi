using System.Collections.Generic;

public static class Define
{
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
        { "닳은", "빛바랜", "온전한", "완벽한" },
        { "Weathered", "Tarnished", "Intact", "Pristine" }
    };
}