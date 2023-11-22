public static class Define
{
    public static float HoldInteractionTime = 2.0f;
    public static float DefaultDamageTick = 0.5f;
    // Cumulative
    public static float[] AbilityOccurrenceByRarity = new float[]
    {
        0.6f,           // Common
        0.6f + 0.2f,    // Rare
        0.8f + 0.15f,   // Epic
        0.95f + 0.05f,  // Legendary
    };
    public static int[] AbilityPriceByRarity = new int[]
    {
        1,      // Common
        2,      // Rare
        3,      // Epic
        4,      // Legendary
    };
}