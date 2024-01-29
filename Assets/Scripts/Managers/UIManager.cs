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
    private PlayerMovement _playerMovement;

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
    private Slider _playerHPSlider;

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

    private void OnClose(InputAction.CallbackContext obj)
    {
        Debug.Log("UIManager::OnClose");
        if (_activeFocusedUI)
        {
            CloseFocusedUI();
        }
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
        Debug.Log("UIManager::LoadInGameUI " + temp++);
        InputActionAsset IAAsset = FindObjectOfType<InputSystemUIInputModule>().actionsAsset;
        _UIIAMap = IAAsset.FindActionMap("UI");
        
        _focusedOverlay     = Instantiate(_focusedOverlayPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _defeatedUI         = Instantiate(_defeatedUIPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _inGameCombatUI     = Instantiate(_inGameCombatPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _metalContractUI    = Instantiate(_metalContractUIPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _zoomedMap          = Instantiate(_zoomedMapPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _playerHPSlider     = _inGameCombatUI.GetComponentInChildren<Slider>();

        _inGameCombatUI.SetActive(true);

        // TEMP 지금은 mainscene->ingame 순서가 아니라 디버깅용이라 여기서 하면 오류
        //_metalContractUI.GetComponent<MetalContractUI>().Initialize();
    }

    private void OnPlayerAdded()
    {
        InputActionAsset IAAsset = FindObjectOfType<PlayerInput>().actions;
        _playerIAMap = IAAsset.FindActionMap("Player");
        _playerMovement = FindObjectOfType<PlayerMovement>();
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
        _playerMovement.DisableMovement(true);
        _playerIAMap.Disable();
        _UIIAMap.Enable();
        _activeFocusedUI = uiObject;
        uiObject.SetActive(true);
        Debug.Log("Enabled");
    }

    public void CloseFocusedUI()
    {
        if (_playerMovement) _playerMovement.EnableMovement(true);
        _playerIAMap.Enable();
        _UIIAMap.Disable();
        _focusedOverlay.SetActive(false);
        if (_activeFocusedUI)
        {
            _activeFocusedUI.SetActive(false);
            _activeFocusedUI = null;
        }
    }

    public void LoadMetalContractUI(MetalContractItem contractItem)
    {
        OpenFocusedUI(_metalContractUI);
    }

    public void ToggleMap()
    {
        OpenFocusedUI(_zoomedMap);
    }

    private void OnCloseMap(InputAction.CallbackContext obj)
    {
        Debug.Log("OnCloseMap");
        if (_activeFocusedUI == _zoomedMap)
        {
            CloseFocusedUI();
        }
    }
}
