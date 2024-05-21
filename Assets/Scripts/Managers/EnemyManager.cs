using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    public int NumEnemyTypes { get; private set; }
    private Dictionary<int, SEnemyData> _enemies;
    private GameObject[] _enemyPrefabs;
    public GameObject GoldPrefab { private set; get; }
    public EnemyVisibilityChecker VisibilityChecker { private set; get; }
    private List<EnemyBase> _spawnedEnemies;
    private Transform _enemiesContainer;

    protected override void Awake()
    {
        base.Awake();
        if (IsToBeDestroyed) return;
        _enemies = new Dictionary<int, SEnemyData>();
        _spawnedEnemies = new List<EnemyBase>();
        GoldPrefab = Resources.Load("Prefabs/Items/Coin").GameObject();
        GameEvents.MapLoaded += OnMapLoaded;
        GameEvents.GameLoadEnded += OnGameLoadEnded;
        InGameEvents.EnemySlayed += (enemy => _spawnedEnemies.Remove(enemy));
        PlayerEvents.Defeated += () =>
        {
            StopAllCoroutines();
            _spawnedEnemies.Clear();
        };
        
        Init(Application.dataPath + "/Tables/EnemyDataTable.csv");
    }
    
    // Spawn enemies from spawners
    private void OnMapLoaded()
    {
        _enemiesContainer = new GameObject("Enemies").transform;

        var spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        foreach (var spawner in spawners)
        {
            spawner.Spawn();
        }
    }

    private void OnGameLoadEnded()
    {
        VisibilityChecker = Camera.main.GetComponent<EnemyVisibilityChecker>();
        StartCoroutine(ActiveCheckCoroutine());
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
                    DefaultArmour = float.Parse(csv.GetField("DefaultArmour")),
                    DefaultArmourPenetration = float.Parse(csv.GetField("DefaultArmourPenetration")),
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
        NumEnemyTypes = _enemies.Count;
        _enemyPrefabs = new GameObject[NumEnemyTypes];
        foreach (var data in _enemies)
        {
            _enemyPrefabs[data.Key] = Resources.Load("Prefabs/Enemies/" + data.Value.PrefabPath).GameObject();
        }
    }

    public SEnemyData GetEnemyData(int enemyID)
    {
        return _enemies[enemyID];
    }

    public GameObject SpawnEnemy(int enemyID, Vector3 spawnLocation)
    {
        var enemy = Instantiate(_enemyPrefabs[enemyID], spawnLocation, Quaternion.identity).GameObject();
        enemy.SetActive(false);
        enemy.transform.SetParent(_enemiesContainer);
        _spawnedEnemies.Add(enemy.GetComponent<EnemyBase>());
        return enemy;
    }
    
    private IEnumerator ActiveCheckCoroutine()
    {
        var activeDistance = 50f;
        var wait = new WaitForSeconds(2f);
        var player = PlayerController.Instance.transform;

        while (true)
        {
            foreach (var enemy in _spawnedEnemies)
            {
                if (enemy == null) continue;
                float distance = Vector3.Distance(enemy.transform.position, player.position);
        
                // Close to the player and is not activated -> activate enemy
                if (distance <= activeDistance && !enemy.gameObject.activeSelf) 
                {
                    enemy.gameObject.SetActive(true);
                }
                // Far from the player and is activated -> deactivate enemy
                else if (distance > activeDistance && enemy.gameObject.activeSelf)
                {
                    enemy.gameObject.SetActive(false);
                }
            }
        
            yield return wait;
        }
    }
}
