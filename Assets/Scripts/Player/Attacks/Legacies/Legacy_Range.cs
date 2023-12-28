using UnityEngine;

[CreateAssetMenu(fileName = "LegacyData_Range", menuName = "ScriptableObjects/LegacyData_Range")]
public class Legacy_Range : LegacyData
{
    public GameObject AttackVFX;
    public GameObject HitVFX;
    public float BulletSpeedMultiplier;

    private void OnFire()
    {

    }

    private void OnHit()
    {

    }
}