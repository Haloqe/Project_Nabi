using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
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

    private GameObject _defeatedUI;
    private GameObject _inGameCombatUI;
    private GameObject _metalContractUI;
    private GameObject _focusedOverlay;
    private GameObject _zoomedMap;
    private MapController _mapController;
    private Slider _playerHPSlider;
    
    // UI Navigation
    private GameObject _activeFocusedUI;
    int temp = 0;

    protected override void Awake()
    {
        base.Awake();
        LoadAllUIPrefabs();
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerEvents.defeated += OnPlayerDefeated;
        PlayerEvents.HPChanged += OnPlayerHPChanged;
        PlayerEvents.playerAddedToScene += OnPlayerAdded;
        var uiAssets = FindObjectOfType<InputSystemUIInputModule>().actionsAsset;
        uiAssets.FindAction("UI/Close").performed += OnClose;
        uiAssets.FindAction("UI/CloseMap").performed += OnCloseMap;
        uiAssets.FindAction("UI/Navigate").performed += OnNavigate;
        uiAssets.FindAction("UI/Navigate").canceled += OnNavigate;
        uiAssets.FindAction("UI/Reset").performed += OnReset;
        uiAssets.FindAction("UI/Zoom").performed += OnZoom;
        uiAssets.FindAction("UI/Zoom").canceled += OnZoom;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        string name = scene.name;
        if (name.Contains("InGame"))
        {
            LoadInGameUI();
        }
        else if (name.Contains("MainMenu"))
        {
            LoadMainMenuUI();
        }
        else
        {

        }
    }

    private void OnPlayerHPChanged(float changeAmount, float hpRatio)
    {
        _playerHPSlider.value = hpRatio;
    }

    private void OnPlayerDefeated()
    {
        LoadDefeatedUI();
    }
    
    private void LoadAllUIPrefabs()
    {
        string path = "Prefabs/UI/";
        _focusedOverlayPrefab   = Utility.LoadObjectFromPath(path + "InGame/FocusedCanvas");
        _defeatedUIPrefab       = Utility.LoadObjectFromPath(path + "InGame/DeadCanvas");
        _inGameCombatPrefab     = Utility.LoadObjectFromPath(path + "InGame/CombatCanvas");
        _metalContractUIPrefab  = Utility.LoadObjectFromPath(path + "InGame/Ability/MetalContractCanvas");
        _zoomedMapPrefab        = Utility.LoadObjectFromPath(path + "InGame/ZoomedMap");
    }

    private void LoadMainMenuUI()
    {
        // TODO
    }

    private void LoadInGameUI()
    {
        InputActionAsset IAAsset = FindObjectOfType<InputSystemUIInputModule>().actionsAsset;
        _UIIAMap = IAAsset.FindActionMap("UI");
        
        _focusedOverlay     = Instantiate(_focusedOverlayPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _defeatedUI         = Instantiate(_defeatedUIPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _inGameCombatUI     = Instantiate(_inGameCombatPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _metalContractUI    = Instantiate(_metalContractUIPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _zoomedMap          = Instantiate(_zoomedMapPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _mapController      = _zoomedMap.GetComponent<MapController>();
        _playerHPSlider     = _inGameCombatUI.GetComponentInChildren<Slider>();
        _zoomedMap.SetActive(false);

        _inGameCombatUI.SetActive(true);

        // TEMP 지금은 mainscene->ingame 순서가 아니라 디버깅용이라 여기서 하면 오류
        //_metalContractUI.GetComponent<MetalContractUI>().Initialize();
    }

    private void OnPlayerAdded()
    {
        InputActionAsset IAAsset = FindObjectOfType<PlayerInput>().actions;
        _playerIAMap = IAAsset.FindActionMap("Player");
        _playerHPSlider.value = FindObjectOfType<PlayerDamageReceiver>().GetHPRatio();
        
        // TODO in scene manager perhaps?
        PlayerAttackManager.Instance.InitInGameVariables();
    }

    private void LoadDefeatedUI()
    {
        CloseFocusedUI();
        _defeatedUI.SetActive(true);
    }

    private void OpenFocusedUI(GameObject uiObject, bool shouldShowOverlay = false)
    {
        if (shouldShowOverlay) _focusedOverlay.SetActive(true);
        _playerIAMap.Disable();
        _UIIAMap.Enable();
        _activeFocusedUI = uiObject;
        uiObject.SetActive(true);
        Debug.Log("Enabled");
    }

    public void CloseFocusedUI()
    {
        _playerIAMap.Enable();
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
