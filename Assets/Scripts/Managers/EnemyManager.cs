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
                    ID = int.Parse(csv.GetField("ID")),
                    Name = csv.GetField("Name"),
                    PrefabPath = csv.GetField("Prefab"),
                    MaxHealth = float.Parse(csv.GetField("MaxHealth")),
                    DefaultMoveSpeed = float.Parse(csv.GetField("DefaultMoveSpeed"))
                };

                _enemies.Add(data.ID, data);
            }
        }
    }
}
