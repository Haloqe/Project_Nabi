using UnityEngine;

[CreateAssetMenu(fileName = "LegacyMelee", menuName = "LegacyData/LegacyData_Melee")]
public class Legacy_Melee : LegacySO
{
    // Prefab object to spawn upon combo hit attack
    public GameObject ComboHitSpawnObject;
    [NamedArray(typeof(ELegacyPreservation))]
    public float[] ComboDamageMultiplier = new float[4]{1,1,1,1};
    
    // Object Spawn positions
    private Vector3[] _comboSpawnOffsets;
    
    public override void Init()
    {
        // Pre-calculate object spawn positions
        var position = ComboHitSpawnObject.transform.position;
        _comboSpawnOffsets = new Vector3[2]; // [0] left, [1] right
        _comboSpawnOffsets[0] = position;
        _comboSpawnOffsets[1] = new Vector3(-position.x, position.y, 0);
        if (ComboHitSpawnObject)
            ComboHitSpawnObject.GetComponent<AttackSpawnObject>().PlayerDamageDealer 
                = PlayerTransform.gameObject.GetComponent<PlayerDamageDealer>();
    }
    public override void OnUpdateStatusEffect(EStatusEffect newEffect)
    {
        if (ComboHitSpawnObject)
            ComboHitSpawnObject.GetComponent<AttackSpawnObject>().StatusEffect = newEffect;
    }

    // Do something upon base attack
    public void OnAttack_Base()
    {
    }

    // Do something upon combo attack
    public void OnAttack_Combo()
    {
        if (ComboHitSpawnObject != null)
        {
            Instantiate(ComboHitSpawnObject, 
                PlayerTransform.position + (PlayerTransform.localScale.x > 0 ? _comboSpawnOffsets[0] : _comboSpawnOffsets[1]),
                Quaternion.identity);
        }
    } 
    
    // Do something upon base attack hit success
    public void OnHit_Base()
    {
        
    }

    // Do something upon combo attack hit success
    public void OnHit_Combo()
    {
        
    } 
}