using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public ESceneType ActiveScene { get; private set; }
    public PlayerMetaData PlayerMetaData { get; private set; }
    public GameObject PlayerPrefab;
    
    // References
    private PlayerController _player;
    private UIManager _uiManager;
    private AudioManager _audioManager;
    
    // DEBUG Only
    private int _ingameMapSceneIdx;          // 플레이어가 실행한 씬
    private int _releaseMapSceneIdx=1;       // 랜덤 제너레이션 씬
    
    // todo temp
    public bool IsFirstRun = true;

    protected override void Awake()
    {
        base.Awake();
        if (IsToBeDestroyed) return;
        PlayerMetaData = SaveSystem.LoadMetaData();
        _ingameMapSceneIdx = _releaseMapSceneIdx;
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerEvents.Defeated += OnPlayerDefeated;
    }

    private void Start()
    {
        _uiManager = UIManager.Instance;
        _audioManager = AudioManager.Instance;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name.Contains("MainMenu"))
        {
            ActiveScene = ESceneType.MainMenu;
            GameEvents.MainMenuLoaded.Invoke();
        }
        else if (scene.name.StartsWith("Boss"))
        {
            ActiveScene = ESceneType.Boss;
            LevelManager.Instance.SpawnPlayer();
            PostLoadInGame();
        }
        else if (scene.name.StartsWith("MapGen"))
        {
            ActiveScene = ESceneType.CombatMap;
            _ingameMapSceneIdx = scene.buildIndex;
            PostLoadInGame();
        }
        else if (scene.name.EndsWith("InGame"))
        {
            ActiveScene = ESceneType.DebugCombatMap;
            _ingameMapSceneIdx = scene.buildIndex;
            PostLoadInGame();
        }
    }

    private void LoadInGame()
    {
        // Loading screen
        _uiManager.HideAllInGameUI();
        _uiManager.DisplayLoadingScreen();
        
        // Load new in-game scene
        // TEMP TODO
        StartCoroutine(LoadSceneAsync());
    }
    
    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_ingameMapSceneIdx);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public bool hasGameLoadEnded;
    private void PostLoadInGame()
    {
        if (ActiveScene == ESceneType.CombatMap || ActiveScene == ESceneType.DebugCombatMap)
        {
            if (IsFirstRun) GameEvents.InGameFirstLoadStarted.Invoke();
            else GameEvents.CombatSceneChanged.Invoke();
        }
        
        // Generate new map
        if (ActiveScene == ESceneType.CombatMap)
        {
            hasGameLoadEnded = false;
            LevelManager.Instance.Generate();
        }
        else
        {
            hasGameLoadEnded = true;
        }
        StartCoroutine(PostLoadInGameCoroutine());
    }

    private IEnumerator PostLoadInGameCoroutine()
    {
        yield return new WaitUntil(() => hasGameLoadEnded);
        GameEvents.MapLoaded.Invoke();
        
        // If not first run, reset variables
        if (ActiveScene != ESceneType.Boss && !IsFirstRun) GameEvents.Restarted.Invoke();

        // When all set, spawn player 
        LevelManager.Instance.SpawnPlayer();
        
        GameObject followObject = new GameObject("CameraFollowingObject");
        followObject.AddComponent<CameraFollowObject>();
        GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>().Follow = followObject.transform;
        //StartCoroutine(PlayerSpawnCoroutine());
        
        _player = PlayerController.Instance;
        GameEvents.GameLoadEnded.Invoke();
        if (ActiveScene == ESceneType.Boss) GameEvents.CombatSceneChanged.Invoke();
    }

    // private IEnumerator PlayerSpawnCoroutine()
    // {
    //     yield return new WaitUntil(() => _playerInitialised);
    //     
    //     // End loading
    //     _player = PlayerController.Instance;
    //     GameEvents.GameLoadEnded.Invoke();
    //     if (ActiveScene == ESceneType.Boss) GameEvents.CombatSceneChanged.Invoke();
    // }
    
    public void LoadMainMenu()
    {
        _uiManager.HideAllInGameUI();
        if (_player != null) _player.gameObject.SetActive(false);
        SceneManager.LoadScene("Scenes/MainMenu");
    }

    public void LoadBossMap()
    {
        _uiManager.DisplayLoadingScreen();
        _audioManager.StopBgm();
        SceneManager.LoadScene("Scenes/Boss_InGame");
    }

    private void OnPlayerDefeated()
    {
        IsFirstRun = false;
        PlayerMetaData.isDirty = true;
        PlayerMetaData.numDeaths++;
        PlayerMetaData.numSouls = _player.playerInventory.SoulShard;
        SaveSystem.SaveMetaData();
    }
    
   public void QuitGame()
    {
        // TODO Confirm panel
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    public void StartNewGame()
    {
        if (_player) {Destroy(_player.gameObject);}
        _uiManager.DestroyAllInGameUI();
        IsFirstRun = true;
        PlayerMetaData = new PlayerMetaData();
        SaveSystem.RemoveMetaData();
        LoadInGame();
    }

    public void ContinueGame()
    {
        LoadInGame();
    }
}
