using UnityEngine;

[CreateAssetMenu(fileName = "LegacyData", menuName = "ScriptableObjects/LegacyData")]
public class LegacyData : ScriptableObject
{
    public SLegacyData Data;
    public ELegacyPreservation Preservation;
    public SDamageInfo Damage_Base;

    // 전진
    public ETiming MoveTiming;
    public float MoveDistance;
    public float MoveSpeed;
}