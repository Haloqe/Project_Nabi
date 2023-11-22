using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    string basePath = Application.dataPath + "/Tables/";

    private PlayerAbilityManager _playerAbilityManager;
    private RelicManager _relicManager;
    private EnemyManager _enemyManager;

    protected override void Awake()
    {
        base.Awake();

        _playerAbilityManager = GetComponent<PlayerAbilityManager>();
        _relicManager = GetComponent<RelicManager>();
        _enemyManager = GetComponent<EnemyManager>();
    }
    
    private void Start()
    {
        _playerAbilityManager.Init(basePath + "PlayerAbilitiesTable.csv");
        _relicManager.Init(basePath + "RelicsTable.csv");
        _enemyManager.Init(basePath + "EnemyDataTable.csv");
    }
}
