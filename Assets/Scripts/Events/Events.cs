using UnityEngine;
using UnityEngine.Events;

public class PlayerEvents
{
    public static UnityAction<float, float, float> HPChanged;
    public static UnityAction defeated;
    public static UnityAction spawned;
    public static UnityAction goldChanged;
    public static UnityAction<ECondition, float> ValueChanged;
    public static UnityAction strengthChanged;
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
    public static UnityAction mapLoaded;
    public static UnityAction languageChanged;
}
