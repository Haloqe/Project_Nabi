using UnityEngine;

[CreateAssetMenu(fileName = "LegacyMelee", menuName = "LegacyData/LegacyData_Melee")]
public class Legacy_Melee : ActiveLegacySO
{
    [Space(15)][Header("Attack Type Specific Settings")]
    // Prefab object to spawn upon combo hit attack
    public AttackSpawnObject ComboHitSpawnObject;
    
    // Damage
    [NamedArray(typeof(ELegacyPreservation))] public StatusEffectInfo[] comboStatusEffects = new StatusEffectInfo[4];
    
    // Object Spawn positions
    private Vector3[] _comboSpawnOffsets;
    
    public override void Init(Transform playerTransform)
    {
        base.Init(playerTransform);
        _spawnScaleMultiplier = 1.0f;
        if (!ComboHitSpawnObject) return;
        ComboHitSpawnObject.attackParentType = ELegacyType.Melee;
        
        // Pre-calculate object spawn positions
        var position = ComboHitSpawnObject.transform.position;
        _comboSpawnOffsets = new Vector3[2]; // [0] left, [1] right
        _comboSpawnOffsets[0] = position;
        _comboSpawnOffsets[1] = new Vector3(-position.x, position.y, 0);
    }

    // Do something upon base attack
    public void OnAttack_Base()
    {
    }

    // Do something upon combo attack
    public void OnAttack_Combo()
    {
        if (!ComboHitSpawnObject) return;
        
        var obj = Instantiate(ComboHitSpawnObject, 
            _playerTransform.position + (_playerTransform.localScale.x > 0 ? _comboSpawnOffsets[0] : _comboSpawnOffsets[1]),
            Quaternion.identity);
        var transform = obj.transform;
        var localScale = transform.localScale;
        transform.localScale = new Vector3(localScale.x * _spawnScaleMultiplier, localScale.y * _spawnScaleMultiplier, localScale.z);
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