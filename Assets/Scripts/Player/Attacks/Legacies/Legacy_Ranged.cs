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
    private int _attackCount = 0;

    public override void Init(Transform playerTransform)
    {
        base.Init(playerTransform);
        _manualDestroyList = new List<GameObject>();
        _spawnScaleMultiplier = 1.0f;
        if (AttackSpawnObject) AttackSpawnObject.attackParentType = ELegacyType.Ranged;
        if (BulletDestroySpawnObject) BulletDestroySpawnObject.attackParentType = ELegacyType.Ranged;
        _attackCount = 0;
    }

    private void OnAttack()
    {
        if (AttackSpawnObject == null) return;
        
        // Spawn object
        AttackSpawnObject spawnedObject = null;
        if (AttackSpawnObject.IsAttachedToPlayer) spawnedObject = Instantiate(AttackSpawnObject, _playerTransform);
        else spawnedObject = Instantiate(AttackSpawnObject, _playerTransform.position + AttackSpawnObject.transform.position, Quaternion.identity);
        
        // Change scale
        var transform = spawnedObject.transform;
        var localScale = transform.localScale;
        transform.localScale = new Vector3(localScale.x * _spawnScaleMultiplier, localScale.y * _spawnScaleMultiplier, localScale.z);
        
        // Handle destruction manually?
        if (!AttackSpawnObject.AutoDestroy) 
            _manualDestroyList.Add(spawnedObject.gameObject);
    }

    public void OnHit(IDamageable target, Vector3 bulletPos)
    {
        // Turbela
        if (target != null && warrior == EWarrior.Turbela && ++_attackCount == 3)
        {
            TurbelaBuffButterfly();
            _attackCount = 0;
        }
        
        // Spawn bullet destroy effect
        if (BulletDestroySpawnObject)
        {
            var obj = Instantiate(BulletDestroySpawnObject, bulletPos, Quaternion.identity);
            var transform = obj.transform;
            var localScale = transform.localScale;
            transform.localScale = new Vector3(localScale.x * _spawnScaleMultiplier, localScale.y * _spawnScaleMultiplier, localScale.z);
        }
        
        // Destroy all temporary objects that are manually managed
        foreach (var objToDestroy in _manualDestroyList)
        {
            Destroy(objToDestroy);
        }
        _manualDestroyList.Clear();
    }
    
    // For legacy - 선전포고 발포
    private void TurbelaBuffButterfly()
    {
        // Kill one random butterfly
        _playerDamageDealer.TurbelaKillButterfly(null);
        
        // Apply buff to spawned butterflies
        var duration = ExtraStatusEffects[(int)preservation].Duration;
        var speedMultiplier = ExtraStatusEffects[(int)preservation].Strength;
        _playerDamageDealer.TurbelaBuffButterflies(duration, speedMultiplier);
    }
}