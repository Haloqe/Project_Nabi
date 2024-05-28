using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public PlayerMetaInfo PlayerMetaInfo { get; private set; }
    public GameObject PlayerPrefab;
    
    // References
    private PlayerController _player;
    
    // DEBUG Only
    private int _debugIngameBuildIdx;
    
    public bool HasSaveData { get; private set; }
    // todo temp
    public bool IsFirstRun = true;

    protected override void Awake()
    {
        base.Awake();
        if (IsToBeDestroyed) return;
        PlayerMetaInfo = new PlayerMetaInfo();
        PlayerMetaInfo.Reset();
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerEvents.Defeated += OnPlayerDefeated;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name.Contains("MainMenu"))
        {
            GameEvents.MainMenuLoaded.Invoke();
        }
        else if (scene.name.Contains("InGame"))
        {
            _debugIngameBuildIdx = scene.buildIndex;
            PostLoadInGame();
        }
    }

    public void LoadInGame()
    {
        // Loading screen
        UIManager.Instance.DisplayLoadingScreen();
        
        // Load new in-game scene
        int randGenMapIdx = SceneManager.GetSceneByName("Scenes/MapGen_InGame").buildIndex;
        // TEMP TODO
        SceneManager.LoadScene(_debugIngameBuildIdx == randGenMapIdx ? randGenMapIdx : _debugIngameBuildIdx);
    }
    
    private void PostLoadInGame()
    {
        // Display loading screen UI
        GameEvents.GameLoadStarted.Invoke();
        
        // Generate new map
        if (_debugIngameBuildIdx == SceneManager.GetSceneByName("Scenes/MapGen_InGame").buildIndex)
        {
            LevelManager.Instance.Generate();
        }
        GameEvents.MapLoaded.Invoke();
        
        // If not first run, reset variables
        // TODo temp
        //if (PlayerController.Instance != null) GameEvents.restarted.Invoke();
        if (!IsFirstRun) GameEvents.Restarted.Invoke();

        // When all set, spawn player 
        LevelManager.Instance.SpawnPlayer();
        PlayerAttackManager.Instance.InitInGameVariables();
        _player = PlayerController.Instance;
        
        // End loading
        GameEvents.GameLoadEnded.Invoke();
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
