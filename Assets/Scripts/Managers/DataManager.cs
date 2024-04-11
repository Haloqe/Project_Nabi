using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    string basePath = Application.dataPath + "/Tables/";
    
    private PlayerAttackManager _playerAttackManager;
    private EnemyManager _enemyManager;

    protected override void Awake()
    {
        base.Awake();
        if (_toBeDestroyed) return;
        
        _playerAttackManager = GetComponent<PlayerAttackManager>();
        _enemyManager = GetComponent<EnemyManager>();
    }
}
