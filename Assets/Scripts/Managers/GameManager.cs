using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private bool _shouldRandomGenerateMap;
    public GameObject PlayerPrefab;

    // Level Details
    // public int PrevStage = 0;
    // public int CurrStage = 0;

    // Used for changing active scene through portals
    private Vector3 _playerPrevPosition { get; set; }
    private Vector3 _playerNextPosition { get; set; }

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
            UIManager.Instance.UseUIControl();
        }
        else if (scene.name.Contains("InGame"))
        {
            PostLoadInGame();
        }
    }

    public void LoadInGame()
    {
        // Load new in-game scene
        if (_shouldRandomGenerateMap) 
            SceneManager.LoadScene("Scenes/MapGen_InGame");
        else // TEMP TODO for debug purpose
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // todo temp
    public bool IsFirstRun = true;
    private void PostLoadInGame()
    {
        // Display loading screen UI
        GameEvents.GameLoadStarted.Invoke();
        
        // Generate new map
        if (_shouldRandomGenerateMap) LevelManager.Instance.Generate();
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

    // TODO Save
    public void LoadMainMenu()
    {
        Destroy(PlayerController.Instance.gameObject);
        IsFirstRun = true;
        UIManager.Instance.StopAllCoroutines();
        SceneManager.LoadScene("Scenes/MainMenu");
    }
}
