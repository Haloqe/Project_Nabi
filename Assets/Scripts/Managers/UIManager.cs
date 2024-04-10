using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    // Input Actions
    private InputSystemUIInputModule _uiInputModule;
    private InputActionMap _playerIAMap;
    private InputActionMap _UIIAMap;
    private InputActionReference _uiPointIARef;
    private InputActionReference _playerPointIARef;
    
    // Prefabs
    private GameObject _defeatedUIPrefab;
    private GameObject _inGameCombatPrefab;
    private GameObject _focusedOverlayPrefab;
    private GameObject _zoomedMapPrefab;
    private GameObject _loadingScreenPrefab;
    private GameObject[] _warriorUIPrefabs;

    // UI Instantiated Objects
    public GameObject inGameCombatUI;
    private GameObject _defeatedUI;
    private GameObject _warriorUIObject;
    private GameObject _focusedOverlay;
    private GameObject _zoomedMap;
    private GameObject _loadingScreenUI;
    
    // UI Mechanism Script
    private WarriorUI _warriorUI;
    private MapController _mapController;
    
    // Minor Controllable Objects
    private Slider _playerHPSlider;
    private Image _playerHPGlobe;
    private Slider _darkGaugeSlider;
    private TextMeshProUGUI _darkGaugeText;
    
    // UI Navigation
    private GameObject _activeFocusedUI;
    int temp = 0;
    private Camera _uiCamera;

    protected override void Awake()
    {
        base.Awake();
        if (_toBeDestroyed) return;
        LoadAllUIPrefabs();
        
        PlayerEvents.defeated += OnPlayerDefeated;
        PlayerEvents.HPChanged += OnPlayerHPChanged;
        PlayerEvents.spawned += OnPlayerSpawned;
        GameEvents.gameLoadStarted += OnGameLoadStarted;
        GameEvents.gameLoadEnded += OnGameLoadEnded;

        _uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
        InputActionAsset IAAsset = _uiInputModule.actionsAsset;
        _UIIAMap = IAAsset.FindActionMap("UI");
        _UIIAMap.FindAction("Close").performed += OnClose;
        _UIIAMap.FindAction("CloseMap").performed += OnCloseMap;
        _UIIAMap.FindAction("Navigate").performed += OnNavigate;
        _UIIAMap.FindAction("Navigate").canceled += OnNavigate;
        _UIIAMap.FindAction("Reset").performed += OnReset;
        _UIIAMap.FindAction("Zoom").performed += OnZoom;
        _UIIAMap.FindAction("Zoom").canceled += OnZoom;
        _UIIAMap.FindAction("Submit").performed += OnSubmit;

        _uiPointIARef = InputActionReference.Create(_UIIAMap.FindAction("Point"));
        _playerPointIARef = InputActionReference.Create(_uiInputModule.point);
    }

    public void UseUIControl()
    {
        if (_playerIAMap != null) _playerIAMap.Disable();
        _UIIAMap.Enable();
    }

    private void OnGameLoadStarted()
    {
        _uiCamera = GameObject.Find("UI Camera").GetComponent<Camera>();
        _loadingScreenUI.SetActive(true);
        _UIIAMap.Enable();
        if (_playerIAMap != null) _playerIAMap.Disable();
        
        _focusedOverlay     = Instantiate(_focusedOverlayPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _defeatedUI         = Instantiate(_defeatedUIPrefab, Vector3.zero, Quaternion.identity).GameObject();
        inGameCombatUI      = Instantiate(_inGameCombatPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _zoomedMap          = Instantiate(_zoomedMapPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _mapController      = _zoomedMap.GetComponent<MapController>();
        _playerHPSlider     = inGameCombatUI.GetComponentInChildren<Slider>();
        _playerHPGlobe      = inGameCombatUI.transform.Find("Globe").Find("HealthGlobe").Find("HealthGlobeMask").Find("Fill").GetComponent<Image>();
        _darkGaugeSlider    = inGameCombatUI.transform.Find("DarkSlider").GetComponentInChildren<Slider>();
        _darkGaugeText      = inGameCombatUI.transform.Find("DarkSlider").GetComponentInChildren<TextMeshProUGUI>();
        _zoomedMap.SetActive(false);
        inGameCombatUI.SetActive(true);
        inGameCombatUI.GetComponent<Canvas>().worldCamera = _uiCamera;
    }
    
    private void OnGameLoadEnded()
    {
        _loadingScreenUI.SetActive(false);
        _playerIAMap.Enable();
        _UIIAMap.Disable();
        UpdateDarkGaugeUI(0);
    }

    public void UpdateDarkGaugeUI(float value)
    {
        _darkGaugeSlider.value = value / 100.0f;
        _darkGaugeText.text = $"{value}/100";
    }
    
    private void OnPlayerHPChanged(float changeAmount, float hpRatio)
    {
        //_playerHPSlider.value = hpRatio;
        _playerHPGlobe.rectTransform.localPosition = new Vector3(0, _playerHPGlobe.rectTransform.rect.height * hpRatio - _playerHPGlobe.rectTransform.rect.height, 0);
    }

    private void OnPlayerDefeated()
    {
        CloseFocusedUI();
        _playerIAMap.Disable();
        StartCoroutine(GameOverCoroutine());
    }
    
    private void LoadAllUIPrefabs()
    {
        string path = "Prefabs/UI/";
        _focusedOverlayPrefab   = Utility.LoadGameObjectFromPath(path + "InGame/FocusedCanvas");
        _defeatedUIPrefab       = Utility.LoadGameObjectFromPath(path + "InGame/GameOverCanvas");
        _inGameCombatPrefab     = Utility.LoadGameObjectFromPath(path + "InGame/CombatCanvas");
        _zoomedMapPrefab        = Utility.LoadGameObjectFromPath(path + "InGame/ZoomedMap");
        _loadingScreenPrefab    = Utility.LoadGameObjectFromPath(path + "LoadingCanvas");
        
        _warriorUIPrefabs = new GameObject[(int)EWarrior.MAX];
        for (int i = 0; i < (int)EWarrior.MAX; i++)
        {
            _warriorUIPrefabs[i] = Utility.LoadGameObjectFromPath(path + "InGame/Warrior/" + (EWarrior)i);
        }
        _loadingScreenUI = Instantiate(_loadingScreenPrefab, Vector3.zero, Quaternion.identity).GameObject();
        DontDestroyOnLoad(_loadingScreenUI);
    }

    private void LoadMainMenuUI()
    {
        // TODO
    }

    private void OnPlayerSpawned()
    {
        _playerHPSlider.value = FindObjectOfType<PlayerDamageReceiver>().GetHPRatio();
        _playerIAMap = FindObjectOfType<PlayerInput>().actions.FindActionMap("Player");
        _playerIAMap.Enable();
        _UIIAMap.Disable();
    }

    private IEnumerator GameOverCoroutine()
    {
        yield return new WaitForSeconds(Define.GameOverDelayTime);
        OpenFocusedUI(_defeatedUI);
    }

    private void OpenFocusedUI(GameObject uiObject, bool shouldShowOverlay = false)
    {
        if (shouldShowOverlay) _focusedOverlay.SetActive(true);
        _playerIAMap.Disable();
        _UIIAMap.Enable();
        _activeFocusedUI = uiObject;
        _uiInputModule.point = _uiPointIARef;
        uiObject.SetActive(true);
    }

    public void CloseFocusedUI()
    {
        if (!_activeFocusedUI) return;
        if (_playerIAMap != null) _playerIAMap.Enable();
        if (_activeFocusedUI == _warriorUIObject) Destroy(_warriorUIObject);
        
        _UIIAMap.Disable();
        _focusedOverlay.SetActive(false);
        _activeFocusedUI.SetActive(false);
        _activeFocusedUI = null;
        _uiInputModule.point = _playerPointIARef;
    }

    public bool OpenWarriorUI(WarriorClockworkInteractor interactor)
    {
        if (_activeFocusedUI) return false; 
        
        _warriorUIObject = Instantiate(_warriorUIPrefabs[(int)interactor.warrior], Vector3.zero, Quaternion.identity).GameObject();
        _warriorUIObject.GetComponent<Canvas>().worldCamera = _uiCamera;
        _warriorUI = _warriorUIObject.GetComponent<WarriorUI>();
        _warriorUI.Initialise(interactor.warrior);
        OpenFocusedUI(_warriorUIObject);
        Destroy(interactor.gameObject);
        return true;
    }

    public void ToggleMap()
    {
        _mapController.ResetMapCamera();
        OpenFocusedUI(_zoomedMap);
    }

    private void OnCloseMap(InputAction.CallbackContext obj)
    {
        if (_activeFocusedUI == _zoomedMap)
        {
            CloseFocusedUI();
        }
    }
    
    private void OnClose(InputAction.CallbackContext obj)
    {
        Debug.Log("UIManager::OnClose");
        if (_activeFocusedUI == _warriorUIObject)
        {
            _warriorUI.OnCancel();
        }
        else if (_activeFocusedUI)
        {
            CloseFocusedUI();
        }
    }

    // UI Navigation
    private void OnNavigate(InputAction.CallbackContext obj)
    {
        var value = obj.ReadValue<Vector2>();
        if (_activeFocusedUI == _zoomedMap) _mapController.OnNavigate(value);
        else if (_activeFocusedUI == _warriorUIObject) _warriorUI.OnNavigate(value); 
    }
    
    private void OnZoom(InputAction.CallbackContext obj)
    {
        var value = obj.ReadValue<float>();
        if (_activeFocusedUI == _zoomedMap) _mapController.OnZoom(value);
    }

    private void OnReset(InputAction.CallbackContext obj)
    {
        if (_activeFocusedUI == _zoomedMap)
        {
            _mapController.ResetMapCamera(true);
        }
    }

    private void OnSubmit(InputAction.CallbackContext obj)
    {
        if (_activeFocusedUI == _warriorUIObject)
        {
            _warriorUI.OnSubmit();
        }
    }
}
