using UnityEngine;

[CreateAssetMenu(fileName = "LegacyMelee", menuName = "LegacyData/LegacyData_Melee")]
public class Legacy_Melee : LegacySO
{
    // Prefab object to spawn upon combo hit attack
    public GameObject ComboHitSpawnObject;
    [NamedArray(typeof(ELegacyPreservation))] public float[] ComboDamageMultiplier = new float[4];
    
    // Do something upon base attack
    private void OnAttack_Base()
    {
    }

    // Do something upon combo attack
    private void OnAttack_Combo()
    {
        if (ComboHitSpawnObject != null)
        {
            Instantiate(ComboHitSpawnObject, _playerTransform.transform.position,
                Quaternion.identity);
        }
    } 
    
    // Do something upon base attack hit success
    private void OnHit_Base()
    {
        
    }

    // Do something upon combo attack hit success
    private void OnHit_Combo()
    {
        
    } 
}