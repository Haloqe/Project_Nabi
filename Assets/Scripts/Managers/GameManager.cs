using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public ESceneType ActiveScene { get; private set; }
    public PlayerMetaInfo PlayerMetaInfo { get; private set; }
    public GameObject PlayerPrefab;
    
    // References
    private PlayerController _player;
    
    // DEBUG Only
    private int _ingameMapSceneIdx;   // 플레이어가 실행한 씬
    private int _releaseMapSceneIdx=1;       // 테스트용 랜덤 제너레이션 씬
    
    public bool HasSaveData { get; private set; }
    // todo temp
    public bool IsFirstRun = true;

    protected override void Awake()
    {
        base.Awake();
        if (IsToBeDestroyed) return;
        PlayerMetaInfo = new PlayerMetaInfo();
        PlayerMetaInfo.Reset();
        _ingameMapSceneIdx = _releaseMapSceneIdx;
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerEvents.Defeated += OnPlayerDefeated;
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

    public void LoadInGame()
    {
        // Loading screen
        UIManager.Instance.DisplayLoadingScreen();
        
        // Load new in-game scene
        // TEMP TODO
        SceneManager.LoadScene(_ingameMapSceneIdx);
    }
    
    private void PostLoadInGame()
    {
        // Display loading screen UI
        GameEvents.GameLoadStarted.Invoke();
        
        // Generate new map
        if (ActiveScene == ESceneType.CombatMap)
        {
            LevelManager.Instance.Generate();
        }
        GameEvents.MapLoaded.Invoke();
        
        // If not first run, reset variables
        if (!IsFirstRun) GameEvents.Restarted.Invoke();

        // When all set, spawn player 
        LevelManager.Instance.SpawnPlayer();
        PlayerAttackManager.Instance.InitInGameVariables();
        _player = PlayerController.Instance;
        
        // End loading
        GameEvents.GameLoadEnded.Invoke();
        if (ActiveScene == ESceneType.Boss) GameEvents.CombatSceneChanged.Invoke();
    }
    
    public void LoadMainMenu()
    {
        // If player exists (ingame->mainmenu), destroy the player
        if (_player)
        {
            Destroy(_player.gameObject);
        }
        
        // TEMP TODO
        IsFirstRun = true;
        PlayerMetaInfo.Reset();
        SceneManager.LoadScene("Scenes/MainMenu");
    }

    public void LoadBossMap()
    {
        UIManager.Instance.DisplayLoadingScreen();
        SceneManager.LoadScene("Scenes/Boss_InGame");
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

    private void OnPlayerDefeated()
    {
        IsFirstRun = false;
        PlayerMetaInfo.NumDeaths++;
        PlayerMetaInfo.NumSouls = _player.playerInventory.SoulShard;
    }
}
