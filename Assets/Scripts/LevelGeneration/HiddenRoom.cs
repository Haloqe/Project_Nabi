using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class HiddenRoom : MonoBehaviour
{
    [SerializeField] public int virtualCameraIndex;
    private CinemachineVirtualCamera _roomCamera;
    private UIManager _uiManager;
    public string roomName;
    public int roomLevel; // 0 - Lowest, 1 - Medium, 2 - Highest
    public int enemyKillTimeLimit;
    public GameObject chestPrefab;
    public Transform chestPosition;

    private AudioSource _audioSource;
    private Vector3 _exitDestination;
    private Portal _returnPortal;
    private Chest _chest;
    private EnemySpawner _enemySpawner;
    private List<GameObject> _spawnedEnemies;
    
    private GameObject _countdownUI;
    private TextMeshProUGUI _countdownText;

    private void Awake()
    {
        _roomCamera = CameraManager.Instance.AllVirtualCameras[virtualCameraIndex];
        _audioSource = GetComponent<AudioSource>();
        _returnPortal = GetComponentInChildren<Portal>(includeInactive: true);
        _enemySpawner = GetComponentInChildren<EnemySpawner>(includeInactive: true);
        _returnPortal.portalType = EPortalType.SecretToCombat;
        _returnPortal.connectedHiddenRoom = this;
    }

    private void Start()
    {
        _uiManager = UIManager.Instance;
    }

    public void OnEnter(Vector3 previousPos)
    {
        CameraManager.Instance.SwapCamera(
            CameraManager.Instance.AllVirtualCameras[1], _roomCamera);
        StartCoroutine(BGMFadeInCoroutine());
        _chest = Instantiate(chestPrefab, chestPosition.position, quaternion.identity).GetComponent<Chest>();
        _exitDestination = previousPos;
        _returnPortal.SetDestination(previousPos);
        _chest.ResetReward(roomLevel);
        
        string desc = enemyKillTimeLimit > 0 ? $"시간 제한: {enemyKillTimeLimit}초" : string.Empty;
        _uiManager.DisplayRoomGuideUI(roomName, desc);
        
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
        CameraManager.Instance.SwapCamera(_roomCamera, CameraManager.Instance.AllVirtualCameras[1]);
        Destroy(_countdownUI);
        StartCoroutine(BGMFadeOutCoroutine());
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
        // Initialise UI
        _countdownUI = _uiManager.DisplayCountdownUI();
        _countdownText = _countdownUI.GetComponentInChildren<TextMeshProUGUI>();

        bool urgentChanged = false;
        int counter = enemyKillTimeLimit;
        while (counter > 0)
        {
            _countdownText.text = counter.ToString("D2");
            if (!urgentChanged && counter <= 10)
            {
                urgentChanged = true;
                _countdownText.color = Color.red;
                _countdownText.fontSize += 5f;
            }
            yield return new WaitForSecondsRealtime(1f);
            counter--;
        }
        yield return new WaitForSecondsRealtime(0.1f);
        
        var player = PlayerController.Instance;
        player.transform.position = _exitDestination;
        player.EnablePlayerInput();
        _uiManager.EnableMap();
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
        // Wait until the last enemy is killed
        if (!_spawnedEnemies.Contains(killedEnemy.gameObject)) return;
        _spawnedEnemies.Remove(killedEnemy.gameObject);
        if (_spawnedEnemies.Count != 0) return;

        // All enemies slayed
        if (enemyKillTimeLimit > 0)
        {
            StopCoroutine(nameof(EnemyKillTimeLimitCoroutine));
            _countdownText.text = ":)";
            _countdownText.color = Color.white;   
        }
        _chest.gameObject.SetActive(true);
        _returnPortal.gameObject.SetActive(true);
    }

    private IEnumerator BGMFadeOutCoroutine()
    {
        float start = _audioSource.volume;
        float currentTime = 0;
        while (currentTime < 0.5f)
        {
            currentTime += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(start, 0, currentTime / 0.5f);
            yield return null;
        }
        _audioSource.enabled = false;
        _audioSource.volume = start;
    }
    
    private IEnumerator BGMFadeInCoroutine()
    {
        float target = _audioSource.volume;
        _audioSource.enabled = true;
        _audioSource.volume = 0;
        float currentTime = 0;
        while (currentTime < 0.5f)
        {
            currentTime += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(0, target, currentTime / 0.5f);
            yield return null;
        }
    }
}
