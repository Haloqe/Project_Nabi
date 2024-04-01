using UnityEngine;
using UnityEngine.Events;

public class PlayerEvents
{
    public static UnityAction<float, float> HPChanged;
    public static UnityAction defeated;
    public static UnityAction spawned;
    public static UnityAction<float> goldChanged;
    public static UnityAction<ECondition, float> ValueChanged;
}

public class InGameEvents
{
    public static UnityAction<EnemyBase> EnemySlayed;
}

public class GameEvents
{
    public static UnityAction restarted;
    public static UnityAction gameLoadStarted;
    public static UnityAction gameLoadEnded;
    public static UnityAction languageChanged;
}
