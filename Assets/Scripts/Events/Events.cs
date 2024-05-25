using UnityEngine.Events;

public static class PlayerEvents
{
    public static UnityAction<float, float, float> HpChanged;
    public static UnityAction Defeated;
    public static UnityAction Spawned;
    public static UnityAction GoldChanged;
    public static UnityAction SoulShardChanged;
    public static UnityAction<ECondition, float> ValueChanged;
    public static UnityAction StrengthChanged;
}

public static class InGameEvents
{
    public static UnityAction<EnemyBase> EnemySlayed;
    public static UnityAction TimeSlowDown;
    public static UnityAction TimeRevertNormal;
}

public static class GameEvents
{
    public static UnityAction MainMenuLoaded;
    public static UnityAction Restarted;
    public static UnityAction GameLoadStarted;
    public static UnityAction GameLoadEnded;
    public static UnityAction MapLoaded;
    public static UnityAction LanguageChanged;
}
