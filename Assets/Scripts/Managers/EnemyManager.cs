using Structs.EnemyStructs;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;


public class EnemyManager : Singleton<EnemyManager>
{
    private Dictionary<int, SEnemyData> _enemies;

    protected override void Awake()
    {
        base.Awake();
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
                    Id = int.Parse(csv.GetField("Id")),
                    Name_EN = csv.GetField("Name_EN"),
                    Name_KO = csv.GetField("Name_KO"),
                    PrefabPath = csv.GetField("Prefab"),
                    MaxHealth = float.Parse(csv.GetField("MaxHealth")),
                    MoveSpeed = float.Parse(csv.GetField("MoveSpeed"))
                };

                _enemies.Add(data.Id, data);
                Debug.Log("Enemy [" + data.Name_EN + "][" + data.Id + "] added to the dictionary.");
            }
        }
    }
}
