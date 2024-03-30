using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    private Dictionary<int, SEnemyData> _enemies;
    [NamedArray(typeof(EEnemyName))] public GameObject[] EnemyPrefabs;

    protected override void Awake()
    {
        base.Awake();
        if (_toBeDestroyed) return;
        _enemies = new Dictionary<int, SEnemyData>();
    }

    public void Init(string dataPath)
    {
        Debug.Log("Initialising Enemy Data");
        Debug.Assert(dataPath != null && File.Exists(dataPath));

        using (var reader = new StreamReader(dataPath))
        using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                SEnemyData data = new SEnemyData
                {
                    ID = int.Parse(csv.GetField("ID")),
                    Name = csv.GetField("Name"),
                    PrefabPath = csv.GetField("Prefab"),
                    DefaultMoveSpeed = float.Parse(csv.GetField("DefaultMoveSpeed")),
                    MaxHealth = float.Parse(csv.GetField("MaxHealth")),
                    DefaultDamage = float.Parse(csv.GetField("DefaultDamage")),
                    Type = csv.GetField("Type"),
                    IdleProbability = float.Parse(csv.GetField("IdleProbability")),
                    IdleAverageDuration = float.Parse(csv.GetField("IdleAverageDuration")),
                    WalkAverageDuration = float.Parse(csv.GetField("WalkAverageDuration")),
                    ChasePlayerDuration = float.Parse(csv.GetField("ChasePlayerDuration")),
                    DetectRangeX = float.Parse(csv.GetField("DetectRangeX")),
                    DetectRangeY = float.Parse(csv.GetField("DetectRangeY")),
                    AttackRangeX = float.Parse(csv.GetField("AttackRangeX")),
                    AttackRangeY = float.Parse(csv.GetField("AttackRangeY"))
                };

                _enemies.Add(data.ID, data);
            }
        }
    }

    public SEnemyData GetEnemyData(int enemyID)
    {
        return _enemies[enemyID];
    }

    public void SpawnEnemy(int enemyID, Vector3 spawnLocation)
    {
        Instantiate(EnemyPrefabs[enemyID-1], spawnLocation, Quaternion.identity);
    }
}
