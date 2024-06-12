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
    private int _ingameMapSceneIdx;       // 플레이어가 실행한 씬
    private int _releaseMapSceneIdx = 1;  // 랜덤 제너레이션 씬
    private int _releaseMapSceneIdx2 = 7; // 여왕벌 이후

    // todo temp
    public bool isFirstRun = true;

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
        else if (scene.name.StartsWith("MapGen_Pre"))
        {
            ActiveScene = ESceneType.CombatMap0;
            //GameObject.Find("MainBackground").GetComponent<Canvas>().worldCamera = Camera.main;
            _ingameMapSceneIdx = scene.buildIndex;
            PostLoadInGame();
        }
        else if (scene.name.StartsWith("MapGen_Post"))
        {
            ActiveScene = ESceneType.CombatMap1;
            PostLoadInGame();
        }
        else if (scene.name.StartsWith("RealMidBoss"))
        {
            ActiveScene = ESceneType.MidBoss;
            GameObject.Find("MainBackground").GetComponent<Canvas>().worldCamera = Camera.main;
            LevelManager.Instance.SpawnPlayer();
            PostLoadInGame();
        }
        else if (scene.name.StartsWith("Boss"))
        {
            ActiveScene = ESceneType.Boss;
            GameObject.Find("MainBackground").GetComponent<Canvas>().worldCamera = Camera.main;
            LevelManager.Instance.SpawnPlayer();
            PostLoadInGame();
        }
        else if (scene.name.EndsWith("InGame"))
        {
            ActiveScene = ESceneType.DebugCombatMap;
            _ingameMapSceneIdx = scene.buildIndex;
            PostLoadInGame();
        }
    }

    private void LoadInGame(int idx)
    {
        // Loading screen
        _uiManager.HideAllInGameUI();
        _uiManager.DisplayLoadingScreen();

        // Load new in-game scene
        StartCoroutine(LoadSceneAsync(idx));
    }

    private IEnumerator LoadSceneAsync(int idx)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(
            idx == 0 ? _ingameMapSceneIdx : _releaseMapSceneIdx2);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public bool hasGameLoadEnded;
    private void PostLoadInGame()
    {
        if (ActiveScene is ESceneType.CombatMap0 or ESceneType.DebugCombatMap)
        {
            if (isFirstRun) GameEvents.InGameFirstLoadStarted.Invoke();
            else GameEvents.CombatSceneChanged.Invoke();
        }

        // Generate new map
        if (ActiveScene is ESceneType.CombatMap0 or ESceneType.CombatMap1)
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
        if (ActiveScene is ESceneType.CombatMap0 or ESceneType.DebugCombatMap && !isFirstRun) 
            GameEvents.Restarted.Invoke();

        // When all set, spawn player 
        LevelManager.Instance.SpawnPlayer();
        
        _player = PlayerController.Instance;
        GameObject followObject = new GameObject("CameraFollowingObject");
        _player.playerMovement.CameraFollowObject = followObject.AddComponent<CameraFollowObject>();
        GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>().Follow = followObject.transform;
        GameEvents.GameLoadEnded.Invoke();
        
        if (ActiveScene is ESceneType.Boss or ESceneType.MidBoss or ESceneType.CombatMap1)
            GameEvents.CombatSceneChanged.Invoke();
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

    public void LoadMidBossMap()
    {
        _uiManager.DisplayLoadingScreen();
        SceneManager.LoadScene("Scenes/RealMidBoss_InGame");
    }
    
    public void LoadBossMap()
    {
        Debug.Log("LoadBossMap");
        _uiManager.DisplayLoadingScreen();
        SceneManager.LoadScene("Scenes/Boss_InGame");
    }

    private void OnPlayerDefeated()
    {
        isFirstRun = false;
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
        isFirstRun = true;
        PlayerMetaData = new PlayerMetaData();
        SaveSystem.RemoveMetaData();
        LoadInGame(0);
    }

    public void ContinueGame()
    {
        LoadInGame(0);
    }

    public void LoadPostMidBossCombatMap()
    {
        LoadInGame(1);
    }
}
