using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public GameObject PlayerPrefab;

    // Used for changing active scene through portals
    private Vector3 _playerPrevPosition { get; set; }
    private Vector3 _playerNextPosition { get; set; }
    
    // DEBUG Only
    private int _debugIngameBuildIdx;
    
    public bool HasSaveData { get; private set; }
    // todo temp
    public bool IsFirstRun = true;

    protected override void Awake()
    {
        base.Awake();
        if (IsToBeDestroyed) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
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
        
        // End loading
        GameEvents.GameLoadEnded.Invoke();
    }
    
    public void SaveStageInfo()
    {
        // PrevStage = CurrStage;
        _playerPrevPosition = PlayerController.Instance.transform.position;
    }
    
    public void LoadMainMenu()
    {
        // If player exists (ingame->mainmenu), destroy the player
        if (PlayerController.Instance)
        {
            Destroy(PlayerController.Instance.gameObject);
        }
        
        IsFirstRun = true;
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
}
