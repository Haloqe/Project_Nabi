using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    private InputActionMap _playerIAMap;
    private InputActionMap _UIIAMap;

    private UnityEngine.Object _defeatedUIPrefab;
    private UnityEngine.Object _inGameCombatPrefab;
    private UnityEngine.Object _metalContractUIPrefab;
    private UnityEngine.Object _focusedOverlayPrefab;
    private UnityEngine.Object _zoomedMapPrefab;
    private UnityEngine.Object _loadingScreenPrefab;

    private GameObject _defeatedUI;
    private GameObject _inGameCombatUI;
    private GameObject _metalContractUI;
    private GameObject _focusedOverlay;
    private GameObject _zoomedMap;
    private GameObject _loadingScreenUI;
    
    private MapController _mapController;
    private Slider _playerHPSlider;
    
    // UI Navigation
    private GameObject _activeFocusedUI;
    int temp = 0;

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
        
        InputActionAsset IAAsset = FindObjectOfType<InputSystemUIInputModule>().actionsAsset;
        _UIIAMap = IAAsset.FindActionMap("UI");
        _UIIAMap.FindAction("Close").performed += OnClose;
        _UIIAMap.FindAction("CloseMap").performed += OnCloseMap;
        _UIIAMap.FindAction("Navigate").performed += OnNavigate;
        _UIIAMap.FindAction("Navigate").canceled += OnNavigate;
        _UIIAMap.FindAction("Reset").performed += OnReset;
        _UIIAMap.FindAction("Zoom").performed += OnZoom;
        _UIIAMap.FindAction("Zoom").canceled += OnZoom;
    }

    public void UseUIControl()
    {
        if (_playerIAMap != null) _playerIAMap.Disable();
        _UIIAMap.Enable();
    }

    private void OnGameLoadStarted()
    {
        _loadingScreenUI.SetActive(true);
        _UIIAMap.Enable();
        if (_playerIAMap != null) _playerIAMap.Disable();
        
        _focusedOverlay     = Instantiate(_focusedOverlayPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _defeatedUI         = Instantiate(_defeatedUIPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _inGameCombatUI     = Instantiate(_inGameCombatPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _metalContractUI    = Instantiate(_metalContractUIPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _zoomedMap          = Instantiate(_zoomedMapPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _mapController      = _zoomedMap.GetComponent<MapController>();
        _playerHPSlider     = _inGameCombatUI.GetComponentInChildren<Slider>();
        _zoomedMap.SetActive(false);
        _inGameCombatUI.SetActive(true);
    }
    
    private void OnGameLoadEnded()
    {
        _loadingScreenUI.SetActive(false);
        _playerIAMap.Enable();
        _UIIAMap.Disable();
    }
    
    private void OnPlayerHPChanged(float changeAmount, float hpRatio)
    {
        _playerHPSlider.value = hpRatio;
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
        _focusedOverlayPrefab   = Utility.LoadObjectFromPath(path + "InGame/FocusedCanvas");
        _defeatedUIPrefab       = Utility.LoadObjectFromPath(path + "InGame/GameOverCanvas");
        _inGameCombatPrefab     = Utility.LoadObjectFromPath(path + "InGame/CombatCanvas");
        _metalContractUIPrefab  = Utility.LoadObjectFromPath(path + "InGame/Ability/MetalContractCanvas");
        _zoomedMapPrefab        = Utility.LoadObjectFromPath(path + "InGame/ZoomedMap");
        _loadingScreenPrefab    = Utility.LoadObjectFromPath(path + "LoadingCanvas");
        
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
        uiObject.SetActive(true);
    }

    private void CloseFocusedUI()
    {
        //TODO remove null check
        if (_playerIAMap != null) _playerIAMap.Enable();
        
        _UIIAMap.Disable();
        _focusedOverlay.SetActive(false);
        if (_activeFocusedUI)
        {
            _activeFocusedUI.SetActive(false);
            _activeFocusedUI = null;
        }
    }

    // public void LoadMetalContractUI(MetalContractItem contractItem)
    // {
    //     OpenFocusedUI(_metalContractUI);
    // }

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
        if (_activeFocusedUI)
        {
            CloseFocusedUI();
        }
    }

    // UI Navigation
    private void OnNavigate(InputAction.CallbackContext obj)
    {
        var value = obj.ReadValue<Vector2>();
        if (_activeFocusedUI == _zoomedMap) _mapController.OnNavigate(value);
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
}
