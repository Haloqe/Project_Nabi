using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "LegacyData_Dash", menuName = "LegacyData/LegacyData_Dash")]
public class Legacy_Dash : ActiveLegacySO
{
    [Space(15)][Header("Attack Type Specific Settings")]
    public GameObject SpawnObject_Pre;
    public GameObject SpawnObject_Peri;
    public GameObject SpawnObject_Post;
    public float PeriSpawnInterval;
    public float DashSpeedMultiplier;

    public override void Init(Transform playerTransform)
    {
        _spawnScaleMultiplier = 1.0f;
        _playerTransform = playerTransform;
        var player = _playerTransform.gameObject.GetComponent<PlayerDamageDealer>();
        if (SpawnObject_Pre)
            SpawnObject_Pre.GetComponent<AttackSpawnObject>().PlayerDamageDealer = player;
        if (SpawnObject_Peri)
            SpawnObject_Peri.GetComponent<AttackSpawnObject>().PlayerDamageDealer = player;
        if (SpawnObject_Post)
            SpawnObject_Post.GetComponent<AttackSpawnObject>().PlayerDamageDealer = player;
    }
    
    public override void OnUpdateStatusEffect(EStatusEffect newEffect)
    {
        if (SpawnObject_Pre)
            SpawnObject_Pre.GetComponent<AttackSpawnObject>().StatusEffect = newEffect;
        if (SpawnObject_Peri)
            SpawnObject_Peri.GetComponent<AttackSpawnObject>().StatusEffect = newEffect;
        if (SpawnObject_Post)
            SpawnObject_Post.GetComponent<AttackSpawnObject>().StatusEffect = newEffect;
    }

    public void OnDashBegin()
    {
        if (SpawnObject_Pre == null) return;
        var obj = Instantiate(SpawnObject_Pre, _playerTransform.position + SpawnObject_Pre.transform.position, Quaternion.identity);
        var localScale = obj.transform.localScale;
        obj.transform.localScale = new Vector3(localScale.x * _spawnScaleMultiplier, localScale.y * _spawnScaleMultiplier, localScale.z);
    }
    
    public void OnDashEnd()
    {
        if (SpawnObject_Post == null) return;
        var obj = Instantiate(SpawnObject_Post, _playerTransform.position + SpawnObject_Post.transform.position, Quaternion.identity);
        var localScale = obj.transform.localScale;
        obj.transform.localScale = new Vector3(localScale.x * _spawnScaleMultiplier, localScale.y * _spawnScaleMultiplier, localScale.z);
    }

    // A coroutine to spawn objects during dash
    public IEnumerator DashCoroutine()
    {
        if (SpawnObject_Peri == null) yield return null;
        
        float timer = PeriSpawnInterval;
        while (true)
        {
            timer += Time.deltaTime;
            if (timer >= PeriSpawnInterval)
            {
                var obj = Instantiate(SpawnObject_Peri, _playerTransform.position + SpawnObject_Peri.transform.position, Quaternion.identity);
                var localScale = obj.transform.localScale;
                obj.transform.localScale = new Vector3(localScale.x * _spawnScaleMultiplier, localScale.y * _spawnScaleMultiplier, localScale.z);
                timer = 0.0f;
            }
            yield return null;
        }
    }
}