using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LegacyData_Ranged", menuName = "LegacyData/LegacyData_Ranged")]
public class Legacy_Ranged : ActiveLegacySO
{
    [Header("Attack Type Specific Settings")]
    // 플레이어의 위치를 원점으로, 총알이 나갈 때 생성할 오브젝트
    public AttackSpawnObject AttackSpawnObject;
    
    // 총알의 위치를 원점으로, 총알이 사라질 때 생성할 오브젝트
    public AttackSpawnObject BulletDestroySpawnObject;
    
    private List<GameObject> _manualDestroyList;

    public override void Init(Transform playerTransform)
    {
        base.Init(playerTransform);
        _manualDestroyList = new List<GameObject>();
        _spawnScaleMultiplier = 1.0f;
    }

    private void OnAttack()
    {
        if (AttackSpawnObject == null) return;
        
        // Spawn object
        AttackSpawnObject spawnedObject = null;
        if (AttackSpawnObject.IsAttached) spawnedObject = Instantiate(AttackSpawnObject, _playerTransform);
        else spawnedObject = Instantiate(AttackSpawnObject, _playerTransform.position + AttackSpawnObject.transform.position, Quaternion.identity);
        
        // Change scale
        var transform = spawnedObject.transform;
        var localScale = transform.localScale;
        transform.localScale = new Vector3(localScale.x * _spawnScaleMultiplier, localScale.y * _spawnScaleMultiplier, localScale.z);
        
        // Handle destruction manually?
        if (AttackSpawnObject.ShouldManuallyDestroy) 
            _manualDestroyList.Add(spawnedObject.gameObject);
    }

    private void OnHit()
    {

    }

    public void OnBulletDestroy(Vector3 bulletPos)
    {
        if (BulletDestroySpawnObject == null) return;
        
        // Spawn bullet destroy effect
        var obj = Instantiate(BulletDestroySpawnObject, bulletPos, Quaternion.identity);
        var transform = obj.transform;
        var localScale = transform.localScale;
        transform.localScale = new Vector3(localScale.x * _spawnScaleMultiplier, localScale.y * _spawnScaleMultiplier, localScale.z);
        
        // Destroy all temporary objects that are manually managed
        foreach (var objToDestroy in _manualDestroyList)
        {
            Destroy(objToDestroy);
        }
        _manualDestroyList.Clear();
    }
}