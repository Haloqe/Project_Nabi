using System;
using System.Collections;
using TMPro;
using Unity.Mathematics;
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
    private GameObject _evadePopupPrefab;
    private GameObject _textPopupPrefab;
    private GameObject _critPopupPrefab;

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
    private TextMeshProUGUI _hpText;
    private Slider _darkGaugeSlider;
    private TextMeshProUGUI _darkGaugeText;
    private Image _bloodOverlay;
    private TensionController _tensionController;
    
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
        _hpText             = inGameCombatUI.transform.Find("Globe").GetComponentInChildren<TextMeshProUGUI>();
        _darkGaugeSlider    = inGameCombatUI.transform.Find("DarkSlider").GetComponentInChildren<Slider>();
        _darkGaugeText      = inGameCombatUI.transform.Find("DarkSlider").GetComponentInChildren<TextMeshProUGUI>();
        _tensionController  = inGameCombatUI.transform.Find("TensionSlider").GetComponent<TensionController>();
        _bloodOverlay       = inGameCombatUI.transform.Find("BloodOverlay").GetComponent<Image>();

        _bloodOverlay.gameObject.SetActive(false);
        _zoomedMap.SetActive(false);
        inGameCombatUI.SetActive(true);
        inGameCombatUI.GetComponent<Canvas>().worldCamera = _uiCamera;
        inGameCombatUI.GetComponent<Canvas>().planeDistance = 20;
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

    public void IncrementTensionGaugeUI() => _tensionController.IncrementTension();
    
    private void OnPlayerHPChanged(float changeAmount, float oldHpRatio, float newHpRatio)
    {
        // Update hp globe
        _playerHPGlobe.rectTransform.localPosition = new Vector3(
            0, _playerHPGlobe.rectTransform.rect.height * newHpRatio - _playerHPGlobe.rectTransform.rect.height, 0);
        
        // Update hp text
        float hp = newHpRatio * 100f;
        if (Math.Abs(hp % 1) < 0.1) 
        {
            _hpText.text = (int)hp + "/100";
        }
        else
        {
            _hpText.text = hp.ToString("F1") + "/100";
        }
        
        // If HP previously below threshold and just moved over threshold, turn off blood overlay
        float critHpRatio = PlayerController.Instance.HpCriticalThreshold;
        if (oldHpRatio <= critHpRatio) 
        {
            if (newHpRatio > critHpRatio)
            {
                _bloodOverlay.gameObject.SetActive(false);
                StopCoroutine(nameof(BloodOverlayCoroutine));
            }
            // When player dies, stop blinking the overlay
            else if (newHpRatio == 0.0f)
            {
                StopCoroutine(nameof(BloodOverlayCoroutine));
            }
        }
        // If HP previously over threshold and just moved below threshold, turn on blood overlay
        else if (newHpRatio <= critHpRatio)
        {
            _bloodOverlay.gameObject.SetActive(true);
            StartCoroutine(nameof(BloodOverlayCoroutine));
        }
    }

    private IEnumerator BloodOverlayCoroutine()
    {
        float duration = 1;
        var minColour = new Color(0, 0, 0, 0);
        var midColour = new Color(1, 1, 1, 0.5f);
        var maxColour = Color.white;
        
        // Increase alpha to 0.5
        for (float time = 0; time < duration; time += Time.deltaTime)
        {
            float progress = Mathf.Lerp(0, time, duration);
            _bloodOverlay.color = Color.Lerp(minColour, midColour, progress);
            yield return null;
        }
        
        // Alternate between 0.5 and 1.0 alpha
        while (true)
        {
            for (float time = 0; time < duration * 2; time += Time.deltaTime)
            {
                float progress = Mathf.PingPong(time, duration) / duration;
                _bloodOverlay.color = Color.Lerp(midColour, maxColour, progress);
                yield return null;
            }
        }
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
        _evadePopupPrefab       = Utility.LoadGameObjectFromPath(path + "InGame/TextPopUp/EvadeUI");
        _textPopupPrefab        = Utility.LoadGameObjectFromPath(path + "InGame/TextPopUp/GeneralTextUI");
        _critPopupPrefab        = Utility.LoadGameObjectFromPath(path + "InGame/TextPopUp/CritPopUp");
        
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

    public bool OpenWarriorUI(Clockwork interactor)
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
    
    // InGame popup
    public void DisplayTextPopUp(string text, Vector3 position, Transform parent = null)
    {
        var ui = Instantiate(_textPopupPrefab, position, quaternion.identity);
        ui.GetComponent<TextUI>().Init(parent, text);
    }
    
    // InGame popup - crit
    public void DisplayCritPopUp(Vector3 position)
    {
        var ui = Instantiate(_critPopupPrefab, position, quaternion.identity);
        ui.GetComponent<TextUI>().Init(null, string.Empty);
    }
    
    public void DisplayPlayerEvadePopUp()
    {
        Instantiate(_evadePopupPrefab, PlayerController.Instance.transform.position + new Vector3(0, 2.3f, 0), quaternion.identity);
    }
}
