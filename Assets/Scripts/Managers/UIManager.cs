using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    string prevSceneName = String.Empty;
    private InputActionMap _playerIAMap;
    private InputActionMap _UIIAMap;
    private PlayerMovement _playerMovement;

    private UnityEngine.Object _defeatedUIPrefab;
    private UnityEngine.Object _activeAbilityUIPrefab;
    private UnityEngine.Object _metalContractUIPrefab;
    private UnityEngine.Object _focusedOverlayPrefab;

    private GameObject _defeatedUI;
    private GameObject _activeAbilityUI;
    private GameObject _metalContractUI;
    private GameObject _focusedOverlay;

    private GameObject _activeFocusedUI;
    int temp = 0;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
        InputAction closeAction = FindObjectOfType<InputSystemUIInputModule>().actionsAsset.FindAction("UI/Close");
        closeAction.performed += OnClose;
        PlayerEvents.Defeated += OnPlayerDefeated;
        LoadAllUIPrefabs();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        Debug.Log("UIManager::OnSceneLoaded");
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

        prevSceneName = name; // temp
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
        _activeAbilityUIPrefab  = Utility.LoadObjectFromPath(path + "InGame/Ability/AbilityCanvas");
        _metalContractUIPrefab  = Utility.LoadObjectFromPath(path + "InGame/Ability/MetalContractCanvas");
    }

    private void LoadMainMenuUI()
    {
        // TODO
    }

    private void LoadInGameUI()
    {
        Debug.Log("UIManager::LoadInGameUI " + temp++);
        InputActionAsset IAAsset = FindObjectOfType<PlayerInput>().actions;
        _playerIAMap = IAAsset.FindActionMap("Player");
        _UIIAMap = IAAsset.FindActionMap("UI");
        _playerMovement = FindObjectOfType<PlayerMovement>();

        _focusedOverlay     = Instantiate(_focusedOverlayPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _defeatedUI         = Instantiate(_defeatedUIPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _activeAbilityUI    = Instantiate(_activeAbilityUIPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _metalContractUI    = Instantiate(_metalContractUIPrefab, Vector3.zero, Quaternion.identity).GameObject();

        _activeAbilityUI.SetActive(true);
        PlayerAbilityManager.Instance.InitInGameVariables();
    }

    private void LoadDefeatedUI()
    {
        CloseFocusedUI();
        _defeatedUI.SetActive(true);
    }

    private void OpenFocusedUI()
    {
        _focusedOverlay.SetActive(true);
        _playerMovement.EnableDisableMovement(false);
        _playerIAMap.Disable();
        _UIIAMap.Enable();
    }

    private void CloseFocusedUI()
    {
        if (_playerMovement) _playerMovement.EnableDisableMovement(true);
        _playerIAMap.Enable();
        _UIIAMap.Disable();
        _focusedOverlay.SetActive(false);
        _activeFocusedUI.SetActive(false);
        _activeFocusedUI = null;
    }

    public void LoadMetalContractUI()
    {
        OpenFocusedUI();
        _activeFocusedUI = _metalContractUI;
        _metalContractUI.SetActive(true);
    }
}
