using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LegacyData_Dash", menuName = "LegacyData/LegacyData_Dash")]
public class Legacy_Dash : ActiveLegacySO
{
    [Space(15)][Header("Attack Type Specific Settings")]
    public AttackSpawnObject SpawnObject_Pre;
    public AttackSpawnObject SpawnObject_Peri;
    public AttackSpawnObject SpawnObject_Post;
    public float PeriSpawnInterval;
    private List<GameObject> _manualDestroyList; 

    public override void Init(Transform playerTransform)
    {
        base.Init(playerTransform);
        _manualDestroyList = new List<GameObject>();
        _spawnScaleMultiplier = 1.0f;
    }

    public void OnDashBegin()
    {
        if (SpawnObject_Pre == null) return;

        // Spawn object
        AttackSpawnObject spawnedObject = null;
        if (SpawnObject_Pre.IsAttached) spawnedObject = Instantiate(SpawnObject_Pre, _playerTransform);
        else spawnedObject = Instantiate(SpawnObject_Pre, _playerTransform.position + SpawnObject_Pre.transform.position, Quaternion.identity);
        
        // Change scale
        var localScale = spawnedObject.transform.localScale;
        spawnedObject.transform.localScale = new Vector3(localScale.x * _spawnScaleMultiplier, localScale.y * _spawnScaleMultiplier, localScale.z);
        
        // Handle destruction manually?
        if (SpawnObject_Pre.ShouldManuallyDestroy)
        {
            _manualDestroyList.Add(spawnedObject.gameObject);
        }
    }
    
    public void OnDashEnd()
    {
        if (SpawnObject_Post != null)
        {
            // Spawn object
            AttackSpawnObject spawnedObject = null;
            if (SpawnObject_Post.IsAttached) spawnedObject = Instantiate(SpawnObject_Post, _playerTransform);
            else spawnedObject = Instantiate(SpawnObject_Post, _playerTransform.position + SpawnObject_Post.transform.position, Quaternion.identity);
            
            // Change scale
            var transform = spawnedObject.transform;
            var localScale = transform.localScale;
            transform.localScale = new Vector3(localScale.x * _spawnScaleMultiplier, localScale.y * _spawnScaleMultiplier, localScale.z);

            // Handle destruction manually?
            if (SpawnObject_Post.ShouldManuallyDestroy)
                _manualDestroyList.Add(spawnedObject.gameObject);
        }
        
        // Destroy all temporary objects that are manually managed
        foreach (var objToDestroy in _manualDestroyList)
        {
            Destroy(objToDestroy);
        }
        _manualDestroyList.Clear();
    }

    // A coroutine to spawn objects during dash
    public IEnumerator DashSpawnCoroutine()
    {
        if (SpawnObject_Peri == null) yield break;
        
        float timer = PeriSpawnInterval;
        while (true)
        {
            timer += Time.deltaTime;
            if (timer >= PeriSpawnInterval)
            {
                // Spawn object
                AttackSpawnObject spawnedObject = null;
                if (SpawnObject_Peri.IsAttached) spawnedObject = Instantiate(SpawnObject_Peri, _playerTransform);
                else spawnedObject = Instantiate(SpawnObject_Peri, _playerTransform.position + SpawnObject_Peri.transform.position, Quaternion.identity);
                
                // Change scale
                var transform = spawnedObject.transform;
                var localScale = transform.localScale;
                transform.localScale = new Vector3(localScale.x * _spawnScaleMultiplier, localScale.y * _spawnScaleMultiplier, localScale.z);
        
                // Handle destruction manually?
                if (SpawnObject_Peri.ShouldManuallyDestroy) 
                    _manualDestroyList.Add(spawnedObject.gameObject);
                
                timer = 0.0f;
            }
            yield return null;
        }
    }
}