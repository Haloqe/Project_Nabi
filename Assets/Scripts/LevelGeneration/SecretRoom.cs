using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SecretRoom : MonoBehaviour
{
    public string roomName;
    public int roomLevel; // 0 - Lowest, 1 - Medium, 2 - Highest
    public float enemyKillTimeLimit;
    public GameObject chestPrefab;
    public Transform chestPosition;
    
    private Vector3 _exitDestination;
    private Portal _returnPortal;
    private Chest _chest;
    private EnemySpawner _enemySpawner;
    private List<GameObject> _spawnedEnemies;

    private void Awake()
    {
        _returnPortal = GetComponentInChildren<Portal>(includeInactive: true);
        _enemySpawner = GetComponentInChildren<EnemySpawner>(includeInactive: true);
        _returnPortal.portalType = EPortalType.SecretToCombat;
        _returnPortal.connectedSecretRoom = this;
    }

    public void OnEnter(Vector3 previousPos)
    {
        _chest = Instantiate(chestPrefab, chestPosition.position, quaternion.identity).GetComponent<Chest>();
        _exitDestination = previousPos;
        _returnPortal.SetDestination(previousPos);
        _chest.ResetReward(roomLevel);
        string desc = enemyKillTimeLimit > 0 ? $"시간 제한: {enemyKillTimeLimit}초" : string.Empty;
        UIManager.Instance.OnEnterSecretRoom(roomName, desc);
        
        if (roomLevel is 0 or 1)
        {
            _chest.gameObject.SetActive(true);
            _returnPortal.gameObject.SetActive(true);
        }
        else
        {
            _chest.gameObject.SetActive(false);
            _returnPortal.gameObject.SetActive(false);
            SpawnEnemies();
        }
    }

    public void OnExit()
    {
        InGameEvents.EnemySlayed -= CheckEnemiesAllKilled;
        if (_spawnedEnemies != null && _spawnedEnemies.Count != 0)
        {
            if (_spawnedEnemies.Count != 0)
            {
                for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
                {
                    _spawnedEnemies[i].GetComponent<EnemyBase>().Die(shouldDropReward:false);
                }
            }
            _enemySpawner.gameObject.SetActive(false);
            _spawnedEnemies.Clear();
        }
        Destroy(_chest.gameObject);
    }

    private IEnumerator EnemyKillTimeLimitCoroutine()
    {
        yield return new WaitForSecondsRealtime(enemyKillTimeLimit + 0.1f);
        var player = PlayerController.Instance;
        player.transform.position = _exitDestination;
        player.EnablePlayerInput();
        UIManager.Instance.EnableMap();
        OnExit();
    }

    public void PrewarmRoom()
    {
        if (_enemySpawner != null)
        {
            _enemySpawner.gameObject.SetActive(true);
            _spawnedEnemies = _enemySpawner.Spawn();
        }
    }

    private void SpawnEnemies()
    {
        if (enemyKillTimeLimit > 0)
        {
            StartCoroutine(nameof(EnemyKillTimeLimitCoroutine));
        }
        InGameEvents.EnemySlayed += CheckEnemiesAllKilled;
    }
    
    private void CheckEnemiesAllKilled(EnemyBase killedEnemy)
    {
        if (!_spawnedEnemies.Contains(killedEnemy.gameObject)) return;
        _spawnedEnemies.Remove(killedEnemy.gameObject);
        if (_spawnedEnemies.Count == 0)
        {
            StopCoroutine(nameof(EnemyKillTimeLimitCoroutine));
            _chest.gameObject.SetActive(true);
            _returnPortal.gameObject.SetActive(true);
        }
    }
}
