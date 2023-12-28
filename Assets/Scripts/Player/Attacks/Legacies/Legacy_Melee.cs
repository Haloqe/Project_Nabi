using UnityEngine;

[CreateAssetMenu(fileName = "LegacyData_Melee", menuName = "ScriptableObjects/LegacyData_Melee")]
public class Legacy_Melee : LegacyData
{
    public GameObject StrikeVFX;
    public GameObject ComboStrikeVFX;
    public GameObject HitSpawnObject;
    public SDamageInfo Damage_Combo;

    private void OnStrike()
    {
        
    }

    private void OnStrike_Combo()
    {

    }

    private void OnHit()
    {

    }

    private void OnHit_Combo()
    {

    }
}