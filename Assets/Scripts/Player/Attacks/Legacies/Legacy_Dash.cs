using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "LegacyData_Dash", menuName = "LegacyData/LegacyData_Dash")]
public class Legacy_Dash : LegacySO
{
    public GameObject SpawnObject_Pre;
    public GameObject SpawnObject_Peri;
    public GameObject SpawnObject_Post;
    public float PeriSpawnInterval;
    public float DashSpeedMultiplier;

    public void OnDashBegin()
    {
        if (SpawnObject_Pre == null) return;
        Instantiate(SpawnObject_Pre, 
            PlayerTransform.position + SpawnObject_Pre.transform.position,
            Quaternion.identity);
    }

    public void OnDashEnd()
    {
        if (SpawnObject_Post == null) return;
        Instantiate(SpawnObject_Post, 
            PlayerTransform.position + SpawnObject_Post.transform.position,
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
                    PlayerTransform.position + SpawnObject_Peri.transform.position,
                    Quaternion.identity);
                timer = 0.0f;
            }
            yield return null;
        }
    }
}