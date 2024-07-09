using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public bool shouldInitiallyBeActive;
    public int spawnLimit;
    [NamedArray(typeof(EEnemyType))] public float[] spawnProbabilities;
    private float[] _spawnProbabilitiesAcc;
    private List<GameObject> _spawnedEnemies;
    
    public List<GameObject> Spawn()
    {
        _spawnedEnemies = new List<GameObject>(spawnLimit);
        if (spawnLimit == 0) return _spawnedEnemies;
        
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
        for (int i = 0; i < spawnLimit; i++)
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

            if ((EEnemyType)spawnID is EEnemyType.VoidMantis or EEnemyType.Spider)
            {
                // Randomise x-position to disperse them
                float randX = UnityEngine.Random.Range(-3f, 3f);
                // Randomise z-position to handle z-fighting
                float randZ = UnityEngine.Random.Range(-2f, 2f);
                transform.position = new Vector3(transform.position.x + randX, transform.position.y, randZ);
            }
            else
            {
                // In the case of insectivore, bee, and queen bee, do not use random x offset
                // Randomise z-position to handle z-fighting
                float randZ = UnityEngine.Random.Range(-2f, 2f);
                transform.position = new Vector3(transform.position.x, transform.position.y, randZ);
            }
            
            // Spawn enemy
            _spawnedEnemies.Add(enemyManager.SpawnEnemy(spawnID, transform.position, shouldInitiallyBeActive));
        }
        return _spawnedEnemies;
    }
}
