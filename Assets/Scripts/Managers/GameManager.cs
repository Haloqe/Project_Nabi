using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public ESceneType ActiveScene { get; private set; }
    public PlayerMetaData PlayerMetaData { get; private set; }
    public GameObject PlayerPrefab;
    private bool _isInitialLoad = true;
    public bool isRunningTutorial;

    // References
    private PlayerController _player;
    private UIManager _uiManager;
    private AudioManager _audioManager;

    // DEBUG Only
    private int _ingameMapSceneIdx;       // 플레이어가 실행한 씬
    private int _releaseMapSceneIdx = 1;  // 랜덤 제너레이션 씬
    private int _releaseMapSceneIdx2 = 4; // 여왕벌 이후

    // todo temp
    public bool isFirstRun = true;

    protected override void Awake()
    {
        base.Awake();
        if (IsToBeDestroyed) return;
        
        PlayerMetaData = SaveSystem.LoadMetaData();
        _ingameMapSceneIdx = _releaseMapSceneIdx;
        var cameraPrefab = Resources.Load<GameObject>("Prefabs/InGameCameras");
        var inGameCameras = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);
        inGameCameras.GetComponentInChildren<AudioListener>().enabled = false;
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerEvents.Defeated += OnPlayerDefeated;
        GameEvents.Restarted += OnRestarted;
    }

    private void Start()
    {
        _uiManager = UIManager.Instance;
        _audioManager = AudioManager.Instance;
    }

    public void OnMainMenuInitialLoad()
    {
        _uiManager.UIIAMap.Enable();
        GameObject.Find("LogoUI").SetActive(false);
        GameEvents.MainMenuLoaded.Invoke();
        _isInitialLoad = false;
    }
        
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name.Contains("MainMenu"))
        {
            CameraManager.Instance.inGameAudioListener.enabled = false;
            ActiveScene = ESceneType.MainMenu;
            
            // Initial load
            if (_isInitialLoad)
            {
                GetComponent<InputSystemUIInputModule>().enabled = false;
            } 
            else
            {
                GameObject.Find("LogoUI").SetActive(false);
                
                // In the case of delayed load, hide loading screen
                if (_uiManager) _uiManager.HideLoadingScreen();
                GameEvents.MainMenuLoaded.Invoke();
            }
        }
        else if (scene.name.StartsWith("Tutorial"))
        {
            ActiveScene = ESceneType.Tutorial;
            PostLoadInGame();
        }
        else if (scene.name.StartsWith("MapGen_Pre"))
        {
            CameraManager.Instance.inGameAudioListener.enabled = true;
            ActiveScene = ESceneType.CombatMap0;
            _ingameMapSceneIdx = scene.buildIndex;
            CameraManager.Instance.gameObject.SetActive(true);
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
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(idx == 0 ? _ingameMapSceneIdx : _releaseMapSceneIdx2);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public bool hasGameLoadEnded;
    private void PostLoadInGame()
    {
        if (ActiveScene is ESceneType.Tutorial)
        {
            GameEvents.InGameFirstLoadStarted.Invoke();
        }
        else if (ActiveScene is ESceneType.CombatMap0)
        {
            GameEvents.CombatSceneChanged.Invoke();
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
        if (ActiveScene is ESceneType.CombatMap0 /*or ESceneType.DebugCombatMap*/ && !isFirstRun) 
            GameEvents.Restarted.Invoke();

        // When all set, spawn player 
        LevelManager.Instance.SpawnPlayer();
        
        _player = PlayerController.Instance;
        if (ActiveScene is not ESceneType.Tutorial)
        {
            GameObject followObject = new GameObject("CameraFollowingObject");
            _player.playerMovement.CameraFollowObject = followObject.AddComponent<CameraFollowObject>();
            CameraManager.Instance.CurrentCamera.Follow = followObject.transform;
        }
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
        _uiManager.DisplayLoadingScreen();
        SceneManager.LoadScene("Scenes/Boss_InGame");
    }

    private void OnPlayerDefeated(bool isRealDeath)
    {
        isFirstRun = false;
        if (isRealDeath)
        {
            PlayerMetaData.isDirty = true;
            PlayerMetaData.numDeaths++;
            PlayerMetaData.numSouls = _player.playerInventory.SoulShard;
            SaveSystem.SaveMetaData();
        }
    }

    private void OnRestarted()
    {
        CameraManager.Instance.SwapCamera(
            CameraManager.Instance.CurrentCamera,
            CameraManager.Instance.AllVirtualCameras[0]);
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
        if (_player) Destroy(_player.gameObject);
        _uiManager.DestroyAllInGameUI();
        isFirstRun = true;
        PlayerMetaData = new PlayerMetaData();
        SaveSystem.RemoveMetaData();
        SceneManager.LoadScene("IntroPreTutorial");
    }

    public void ContinueGame()
    {
        _player.gameObject.SetActive(true);
        LoadInGame(0);
    }

    public void LoadPostMidBossCombatMap()
    {
        LoadInGame(1);
    }
    
    public void LoadMainMenuDelayed() => StartCoroutine(LoadMainMenuDelayedCoroutine());
    private IEnumerator LoadMainMenuDelayedCoroutine()
    {
        _uiManager.DisplayLoadingScreen();
        yield return new WaitForSecondsRealtime(0.5f);
        LoadMainMenu();
    }

    public void LoadTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }
}
