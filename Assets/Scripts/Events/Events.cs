using UnityEngine;
using UnityEngine.Events;

public class PlayerEvents
{
    public static UnityAction<float, float> HPChanged;
    public static UnityAction defeated;
    public static UnityAction spawned;
}

public class GameEvents
{
    public static UnityAction restarted;
    public static UnityAction gameLoadStarted;
    public static UnityAction gameLoadEnded;
}
