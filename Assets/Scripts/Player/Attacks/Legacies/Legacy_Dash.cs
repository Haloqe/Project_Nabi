using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "LegacyData_Dash", menuName = "LegacyData/LegacyData_Dash")]
public class Legacy_Dash : ActiveLegacySO
{
    [Header("Attack Type Specific Settings")]
    public GameObject SpawnObject_Pre;
    public GameObject SpawnObject_Peri;
    public GameObject SpawnObject_Post;
    public float PeriSpawnInterval;
    public float DashSpeedMultiplier;

    public override void Init(Transform playerTransform)
    {
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
        Instantiate(SpawnObject_Pre, 
            _playerTransform.position + SpawnObject_Pre.transform.position,
            Quaternion.identity);
    }

    public void OnDashEnd()
    {
        if (SpawnObject_Post == null) return;
        Instantiate(SpawnObject_Post, 
            _playerTransform.position + SpawnObject_Post.transform.position,
            Quaternion.identity);
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
                Instantiate(SpawnObject_Peri,
                    _playerTransform.position + SpawnObject_Peri.transform.position,
                    Quaternion.identity);
                timer = 0.0f;
            }
            yield return null;
        }
    }
}