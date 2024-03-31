using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    private Dictionary<int, SEnemyData> _enemies;
    public GameObject[] EnemyPrefabs { private set; get; }
    public GameObject GoldPrefab { private set; get; }
    public EnemyVisibilityChecker VisibilityChecker { private set; get; }

    protected override void Awake()
    {
        base.Awake();
        if (_toBeDestroyed) return;
        _enemies = new Dictionary<int, SEnemyData>();
        GoldPrefab = Resources.Load("Prefabs/Items/Coin").GameObject();
        GameEvents.gameLoadEnded += OnGameLoadEnded;
    }
    
    private void OnGameLoadEnded()
    {
        VisibilityChecker = Camera.main.GetComponent<EnemyVisibilityChecker>();
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
                    DamageType = csv.GetField("DamageType"),
                    IdleProbability = float.Parse(csv.GetField("IdleProbability")),
                    IdleAverageDuration = float.Parse(csv.GetField("IdleAverageDuration")),
                    WalkAverageDuration = float.Parse(csv.GetField("WalkAverageDuration")),
                    ChasePlayerDuration = float.Parse(csv.GetField("ChasePlayerDuration")),
                    DetectRangeX = float.Parse(csv.GetField("DetectRangeX")),
                    DetectRangeY = float.Parse(csv.GetField("DetectRangeY")),
                    AttackRangeX = float.Parse(csv.GetField("AttackRangeX")),
                    AttackRangeY = float.Parse(csv.GetField("AttackRangeY")),
                    MinGoldRange = int.Parse(csv.GetField("MinGoldRange")),
                    MaxGoldRange = int.Parse(csv.GetField("MaxGoldRange")),
                };

                _enemies.Add(data.ID, data);
            }
        }

        // Save prefabs
        EnemyPrefabs = new GameObject[_enemies.Count];
        foreach (var data in _enemies)
        {
            EnemyPrefabs[data.Key] = Resources.Load("Prefabs/Enemies/" + data.Value.PrefabPath).GameObject();
        }
    }

    public SEnemyData GetEnemyData(int enemyID)
    {
        return _enemies[enemyID];
    }

    public GameObject SpawnEnemy(int enemyID, Vector3 spawnLocation)
    {
        return Instantiate(EnemyPrefabs[enemyID], spawnLocation, Quaternion.identity).GameObject();
    }
}
