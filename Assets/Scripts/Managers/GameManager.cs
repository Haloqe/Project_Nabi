using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public ESceneType ActiveScene { get; private set; }
    public PlayerMetaData PlayerMetaData { get; private set; }
    public GameObject PlayerPrefab;
    private bool _isInitialLoad = true;
    
    // Display
    public Vector2Int[] ResolutionOptions { get; private set; }
    public bool[] IsResolutionSupported { get; private set; }
    public int ResolutionIndex { get; private set; }
    
    // Tutorial
    public bool isRunningTutorial;
    public ECutSceneType activeCutScene;
    public bool isRunningCutScene;

    // References
    private PlayerController _player;

    // DEBUG Only
    private int _ingameMapSceneIdx;       // 플레이어가 실행한 씬
    private int _releaseMapSceneIdx = 1;  // 랜덤 제너레이션 씬
    private int _releaseMapSceneIdx2 = 2; // 여왕벌 이후

    // todo temp
    public bool isFirstRun = true;
    
    // Settings
    public SettingsData_General GeneralSettingsData { get; private set; }
    private SettingsData_General _defaultGeneralSettingsData;

    protected override void Awake()
    {
        base.Awake();
        if (IsToBeDestroyed) return;
        
        ResolutionOptions = new Vector2Int[]
        {
            new Vector2Int(1280,720), new Vector2Int(1600,900), new Vector2Int(1920,1080),
        };
        
        PlayerMetaData = SaveSystem.LoadMetaData();
        _ingameMapSceneIdx = _releaseMapSceneIdx;
        
        var cameraPrefab = Resources.Load<GameObject>("Prefabs/InGameCameras");
        var inGameCameras = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);
        inGameCameras.SetActive(false);
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerEvents.Defeated += OnPlayerDefeated;
        GameEvents.Restarted += OnRestarted;
    }

    private void Start()
    {
        GetSavedGeneralData();
    }

    public void SaveGeneralSettings()
    {
        GeneralSettingsData.IsFullscreen = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
        GeneralSettingsData.ResolutionIndex = ResolutionIndex;
        GeneralSettingsData.Localisation = (int)Define.Localisation;
        SaveSystem.SaveGeneralSettingsData();
    }
    
    private void GetSavedGeneralData()
    {
        // Get default settings first
        _defaultGeneralSettingsData = new SettingsData_General();
        _defaultGeneralSettingsData.IsFullscreen = true;
        _defaultGeneralSettingsData.Localisation = (int)ELocalisation.KOR;
        // Get the highest supported resolution
        IsResolutionSupported = new bool[ResolutionOptions.Length];
        for (int i = 0; i < ResolutionOptions.Length; i++)
        {
            IsResolutionSupported[i] = false;
            if (Screen.resolutions.Any(resolution => resolution.width == ResolutionOptions[i][0]
                    && resolution.height == ResolutionOptions[i][1]))
            {
                IsResolutionSupported[i] = true;
            }
        }
        ResolutionIndex = ResolutionOptions.Length - 1;
        while (ResolutionIndex > 0 && !IsResolutionSupported[ResolutionIndex])
        {
            ResolutionIndex--;
        }
        _defaultGeneralSettingsData.ResolutionIndex = ResolutionIndex;
        
        // Get saved settings
        GeneralSettingsData = SaveSystem.LoadGeneralSettingsData();
        if (GeneralSettingsData == null)
        {
            // If no save data exists, use default settings
            ResetGeneralSettingsToDefault();
        }
        else
        {
            // If save data exists, apply the saved settings
            SetFullscreen(GeneralSettingsData.IsFullscreen);
            SetResolution(GeneralSettingsData.ResolutionIndex);
            SetLocale((ELocalisation)GeneralSettingsData.Localisation);
        }
    }
    
    public void ResetGeneralSettingsToDefault()
    {
        GeneralSettingsData = _defaultGeneralSettingsData.Clone();
        SetFullscreen(GeneralSettingsData.IsFullscreen);
        SetResolution(GeneralSettingsData.ResolutionIndex);
        SetLocale((ELocalisation)GeneralSettingsData.Localisation);
    }
    
    public void SetFullscreen(bool isFullscreen)
    {
        GeneralSettingsData.IsFullscreen = isFullscreen;
        Screen.SetResolution(ResolutionOptions[GeneralSettingsData.ResolutionIndex][0], ResolutionOptions[GeneralSettingsData.ResolutionIndex][1],
            isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
    }
    
    public void SetResolution(int resolutionIndex)
    {
        GeneralSettingsData.ResolutionIndex = resolutionIndex;
        Screen.SetResolution(ResolutionOptions[resolutionIndex][0], ResolutionOptions[resolutionIndex][1], Screen.fullScreenMode);
    }
    
    public void SetLocale(ELocalisation locale)
    {
        GeneralSettingsData.Localisation = (int)locale;
        Define.Localisation = locale;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[(int)locale];
    }
    
    public void OnMainMenuInitialLoad()
    {
        GetComponent<InputSystemUIInputModule>().enabled = true;
        UIManager.Instance.UIIAMap.Enable();
        GameObject.Find("LogoUI").SetActive(false);
        GameEvents.MainMenuLoaded.Invoke();
        _isInitialLoad = false;
    }
        
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name.Contains("MainMenu"))
        {
            CameraManager.Instance.SwapCamera(CameraManager.Instance.AllVirtualCameras[0]);
            CameraManager.Instance.inGameAudioListener.enabled = false;
            CameraManager.Instance.gameObject.SetActive(false);
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
                if (UIManager.Instance) UIManager.Instance.HideLoadingScreen();
                GameEvents.MainMenuLoaded.Invoke();
            }
        }
        else if (scene.name.StartsWith("Tutorial"))
        {
            PostLoadInGame();
        }
        else if (scene.name.StartsWith("MapGen_Pre"))
        {
            CameraManager.Instance.gameObject.SetActive(true);
            CameraManager.Instance.inGameAudioListener.enabled = true;
            ActiveScene = ESceneType.CombatMap0;
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
        UIManager.Instance.HideAllInGameUI();
        UIManager.Instance.DisplayLoadingScreen();

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
    private bool _isInGameFirstLoad = true;
    private void PostLoadInGame()
    {
        if (ActiveScene is ESceneType.Tutorial)
        {
            GameEvents.InGameFirstLoadStarted.Invoke();
            _isInGameFirstLoad = false;
        }
        else if (ActiveScene is ESceneType.CombatMap0)
        {
            if (_isInGameFirstLoad)
            {
                _isInGameFirstLoad = false;
                GameEvents.InGameFirstLoadStarted.Invoke();
            }
            else
            {
                GameEvents.CombatSceneChanged.Invoke();
            }
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
            Debug.Log("Follow object added");
            CameraManager.Instance.CurrentCamera.Follow = followObject.transform;
        }
        GameEvents.MapLoaded.Invoke();
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
        UIManager.Instance.HideAllInGameUI();
        if (_player != null) _player.gameObject.SetActive(false);
        SceneManager.LoadScene("Scenes/MainMenu");
    }

    public void LoadMidBossMap()
    {
        UIManager.Instance.DisplayLoadingScreen();
        SceneManager.LoadScene("Scenes/RealMidBoss_InGame");
    }
    
    public void LoadBossMap()
    {
        UIManager.Instance.DisplayLoadingScreen();
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
        UIManager.Instance.DestroyAllInGameUI();
        isFirstRun = true;
        PlayerMetaData = new PlayerMetaData();
        SaveSystem.RemoveMetaData();
        SceneManager.LoadScene("IntroPreTutorial_" + (Define.Localisation == ELocalisation.ENG ? "ENG" : "KOR"));
        ActiveScene = ESceneType.Tutorial;
    }

    public void ContinueGame()
    {
        LoadInGame(0);
    }

    public void LoadPostMidBossCombatMap()
    {
        LoadInGame(1);
    }
    
    public void LoadMainMenuDelayed() => StartCoroutine(LoadMainMenuDelayedCoroutine());
    private IEnumerator LoadMainMenuDelayedCoroutine()
    {
        UIManager.Instance.DisplayLoadingScreen();
        yield return new WaitForSecondsRealtime(0.5f);
        LoadMainMenu();
    }

    public void LoadTutorial()
    {
        SceneManager.LoadScene("Tutorial_" + (Define.Localisation == ELocalisation.ENG ? "ENG" : "KOR"));
    }

    public void LoadBossCutScene()
    {
        SceneManager.LoadScene("CutScene_Boss_" + (Define.Localisation == ELocalisation.ENG ? "ENG" : "KOR"));
        UIManager.Instance.PlayerIAMap.Disable();
        UIManager.Instance.HideAllInGameUI();
        _player.gameObject.SetActive(false);
    }

    public void OnBossSlayed()
    {
        PlayerEvents.Defeated.Invoke(false);
        PlayerMetaData.isDirty = true;
        PlayerMetaData.numSouls = _player.playerInventory.SoulShard;
        SaveSystem.SaveMetaData();
    }
}
