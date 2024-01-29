using UnityEngine;
using UnityEngine.Events;

public class PlayerEvents
{
    public static UnityAction<float, float> HPChanged;
    public static UnityAction defeated;
    public static UnityAction playerAddedToScene;
}