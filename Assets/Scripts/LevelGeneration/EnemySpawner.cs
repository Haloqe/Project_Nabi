using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public int spawnCount;
    [NamedArray(typeof(EEnemyType))] public float[] spawnProbabilities;
    private float[] _spawnProbabilitiesAcc;

    public void Spawn()
    {
        if (spawnCount == 0) return;
        
        // Save accumulated probabilities
        _spawnProbabilitiesAcc = new float[spawnProbabilities.Length];
        _spawnProbabilitiesAcc[0] = spawnProbabilities[0];
        for (int i = 1; i < spawnProbabilities.Length; i++)
        {
            _spawnProbabilitiesAcc[i] = _spawnProbabilitiesAcc[i - 1] + spawnProbabilities[i];
        }
        
        // Spawn enemies
        var enemyManager = EnemyManager.Instance;
        System.Random sysRandom = new System.Random();
        var probabilitySum = _spawnProbabilitiesAcc[^1];
        for (int i = 0; i < spawnCount; i++)
        {
            int spawnID;
            // If probabilities are not set, apply equal chances
            if (probabilitySum == 0)
            {
                spawnID = UnityEngine.Random.Range(0, spawnProbabilities.Length);
            }
            // or follow the assigned probabilities
            else
            {
                double randValue = sysRandom.NextDouble() * probabilitySum;
                if (randValue == 0)
                {
                    spawnID = Array.FindIndex(_spawnProbabilitiesAcc, probability => probability != 0);   
                }
                else
                {
                    spawnID = Array.FindIndex(_spawnProbabilitiesAcc, probability => probability >= randValue);
                }
            }
            enemyManager.SpawnEnemy(spawnID, transform.position);
        }
    }
}
