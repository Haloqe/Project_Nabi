using UnityEngine.Events;

public static class PlayerEvents
{
    public static UnityAction<float, float, float> HpChanged;
    public static UnityAction Defeated;
    public static UnityAction StartResurrect;
    public static UnityAction EndResurrect;
    public static UnityAction SpawnedFirstTime;
    public static UnityAction Spawned;
    public static UnityAction GoldChanged;
    public static UnityAction SoulShardChanged;
    public static UnityAction<ECondition, float> ValueChanged;
    public static UnityAction StrengthChanged;
}

public static class InGameEvents
{
    public static UnityAction TimeSlowDown;
    public static UnityAction TimeRevertNormal;
    public static UnityAction<EnemyBase> EnemySlayed;
    public static UnityAction MidBossSlayed;
    public static UnityAction BossSlayed;
}

public static class GameEvents
{
    public static UnityAction MainMenuLoaded;
    public static UnityAction Restarted;
    public static UnityAction InGameFirstLoadStarted;
    public static UnityAction GameLoadEnded;
    public static UnityAction CombatSceneChanged;
    public static UnityAction MapLoaded;
    public static UnityAction LanguageChanged;
}
