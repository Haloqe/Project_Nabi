using System.Collections;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    // Input Actions
    private InputSystemUIInputModule _uiInputModule;
    public InputActionAsset InputActionAsset;
    public InputActionMap PlayerIAMap { get; private set; }
    public InputActionMap UIIAMap { get; private set; }
    private InputActionReference _uiPointIARef;
    private InputActionReference _playerPointIARef;
    private InputActionReference _uiClickIARef;
    private InputActionReference _playerClickIARef;
    
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
    private GameObject _soulPopupPrefab;
    private GameObject _goldPlusPopupPrefab;
    private GameObject _goldMinusPopupPrefab;
    private GameObject _bookPrefab;
    private GameObject _metaUpgradeUIPrefab;
    private GameObject _roomGuidePrefab;
    private GameObject _settingsPrefab;
    private GameObject _timerPrefab;
    private GameObject _creditsUIPrefab;

    // UI Instantiated Objects
    private GameObject _mainMenuUI;
    private GameObject _inGameCombatUI;
    private GameObject _defeatedUI;
    private GameObject _warriorUIObject;
    private GameObject _focusedOverlay;
    private GameObject _zoomedMap;
    private GameObject _minimap;
    private GameObject _loadingScreenUI;
    private GameObject _bookUI;
    private GameObject _metaUpgradeUI;
    private GameObject _settingsUI;
    
    // UI Mechanism Script
    private MainMenuUIController _mainMenuUIController;
    private WarriorUIController _warriorUIController;
    private MapController _mapController;
    private DefeatedUIController _defeatedUIController;
    private BookUIController _bookUIController;
    private MetaUIController _metaUIController;
    private SettingsUIController _settingsUIController;
    
    // Minor Controllable Objects
    private Slider _playerHPSlider;
    private Image _playerHPGlobe;
    private TextMeshProUGUI _hpText;
    private Slider _darkGaugeSlider;
    private TextMeshProUGUI _darkGaugeText;
    private Image _tensionOverlay;
    public PlayerTensionController TensionController { get; private set; }
    
    // Blood overlay
    private Image _bloodOverlayLowHP;
    private Image _bloodOverlayHit;
    private float _bloodOverlayHitDisplayTime;
    
    // UI Navigation
    private GameObject _activeFocusedUI;
    private UIControllerBase _activeFocusedUIController;
    private Camera _uiCamera;
    
    // References
    private PlayerController _playerController;
    private PlayerAttackManager _playerAttackManager;
    private AudioManager _audioManager;
    private GameManager _gameManager;
    private TextMeshProUGUI[] _combatKeyBindTMPs;
    
    // Flower bomb
    [NamedArray(typeof(EFlowerType))] [SerializeField] private Sprite[] flowerIconSprites = new Sprite[(int)EFlowerType.MAX];
    private Image[] _flowerIconImages;
    private TextMeshProUGUI _flowerCountText;
    private Animator _flowerPanelAnimator;
    private float _flowerUIDisplayTime;
    
    protected override void Awake()
    {
        base.Awake();
        if (IsToBeDestroyed) return;
        LoadAllUIPrefabs();
        
        PlayerEvents.Defeated += OnPlayerDefeated;
        PlayerEvents.StartResurrect += OnPlayerStartResurrect;
        PlayerEvents.EndResurrect += OnPlayerEndResurrect;
        PlayerEvents.HpChanged += OnPlayerHPChanged;
        PlayerEvents.Spawned += OnPlayerSpawned;
        GameEvents.MainMenuLoaded += OnMainMenuLoaded;
        GameEvents.InGameFirstLoadStarted += OnInGameFirstLoad;
        GameEvents.GameLoadEnded += OnGameLoadEnded;
        GameEvents.CombatSceneChanged += OnCombatSceneChanged;
        InGameEvents.BossSlayed += OnBossSlayed;
        InGameEvents.TimeSlowDown += () => _tensionOverlay.gameObject.SetActive(true);
        InGameEvents.TimeRevertNormal += () => _tensionOverlay.gameObject.SetActive(false);

        _uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
        PlayerIAMap = InputActionAsset.FindActionMap("Player");
        UIIAMap = InputActionAsset.FindActionMap("UI");
        UIIAMap.FindAction("Close").performed += OnClose;
        UIIAMap.FindAction("CloseMap").performed += OnCloseMap;
        UIIAMap.FindAction("Tab").performed += OnTab;
        UIIAMap.FindAction("Navigate").performed += OnNavigate;
        UIIAMap.FindAction("Navigate").canceled += OnNavigate;
        UIIAMap.FindAction("Reset").performed += OnReset;
        UIIAMap.FindAction("Zoom").performed += OnZoom;
        UIIAMap.FindAction("Zoom").canceled += OnZoom;
        UIIAMap.FindAction("Submit").performed += OnSubmit;

        _uiPointIARef = InputActionReference.Create(UIIAMap.FindAction("Point"));
        _playerPointIARef = InputActionReference.Create(_uiInputModule.point);
        _uiClickIARef = InputActionReference.Create(UIIAMap.FindAction("Click"));
        _playerClickIARef = InputActionReference.Create(_uiInputModule.leftClick);
        LoadActionRebinds();
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
        _audioManager = AudioManager.Instance;
    }

    private void OnInGameFirstLoad()
    {
        //_uiCamera = GameObject.Find("UI Camera").GetComponent<Camera>();
        _uiCamera = CameraManager.Instance.inGameMainCamera;
        _activeFocusedUI = null;
        
        _focusedOverlay     = Instantiate(_focusedOverlayPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _defeatedUI         = Instantiate(_defeatedUIPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _inGameCombatUI     = Instantiate(_inGameCombatPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _zoomedMap          = Instantiate(_zoomedMapPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _bookUI             = Instantiate(_bookPrefab, Vector3.zero, Quaternion.identity).GameObject();
        _metaUpgradeUI      = Instantiate(_metaUpgradeUIPrefab, Vector3.zero, Quaternion.identity).GameObject();
        
        _metaUIController   = _metaUpgradeUI.GetComponent<MetaUIController>();
        _bookUIController   = _bookUI.GetComponentInChildren<BookUIController>();
        _defeatedUIController = _defeatedUI.GetComponent<DefeatedUIController>();
        _mapController      = _zoomedMap.GetComponent<MapController>();
        
        _playerHPSlider     = _inGameCombatUI.GetComponentInChildren<Slider>();
        _playerHPGlobe      = _inGameCombatUI.transform.Find("Globe").Find("HealthGlobe").Find("HealthGlobeMask").Find("Fill").GetComponent<Image>();
        _hpText             = _inGameCombatUI.transform.Find("Globe").GetComponentInChildren<TextMeshProUGUI>();
        _darkGaugeSlider    = _inGameCombatUI.transform.Find("DarkSlider").GetComponentInChildren<Slider>();
        _darkGaugeText      = _inGameCombatUI.transform.Find("DarkSlider").GetComponentInChildren<TextMeshProUGUI>();
        _bloodOverlayLowHP  = _inGameCombatUI.transform.Find("BloodOverlay_LowHP").GetComponent<Image>();
        _bloodOverlayHit    = _inGameCombatUI.transform.Find("BloodOverlay_Hit").GetComponent<Image>();
        _tensionOverlay     = _inGameCombatUI.transform.Find("TensionOverlay").GetComponent<Image>();
        _minimap            = _inGameCombatUI.transform.Find("MinimapContainer").Find("Minimap").gameObject;
        TensionController   = _inGameCombatUI.transform.Find("TensionSlider").GetComponent<PlayerTensionController>();

        _combatKeyBindTMPs = new TextMeshProUGUI[5];
        var activeLayoutGroup = _inGameCombatUI.transform.Find("ActiveLayoutGroup");
        for (int i = 0; i < 4; i++)
        {
            _combatKeyBindTMPs[i] = activeLayoutGroup.Find("Slot_" + i).Find("KeyText").GetComponent<TextMeshProUGUI>();
        }
        
        _inGameCombatUI.SetActive(true);
        _inGameCombatUI.GetComponent<Canvas>().worldCamera = _uiCamera;
        _inGameCombatUI.GetComponent<Canvas>().planeDistance = 20;
        
        DontDestroyOnLoad(_focusedOverlay);
        DontDestroyOnLoad(_defeatedUI);
        DontDestroyOnLoad(_inGameCombatUI);
        DontDestroyOnLoad(_zoomedMap);
        DontDestroyOnLoad(_bookUI);
        DontDestroyOnLoad(_metaUpgradeUI);
        
        // UI - flower bombs
        var flowerSlotRoot = activeLayoutGroup.Find("Slot_3");
        var otherFlowerSlots = flowerSlotRoot.Find("MaskPanel").Find("OtherFlowers").Find("Canvas");
        _flowerIconImages = new Image[]
        {
            flowerSlotRoot.Find("AbilityIcon").GetComponent<Image>(),
            otherFlowerSlots.Find("Icon1").GetComponent<Image>(),
            otherFlowerSlots.Find("Icon2").GetComponent<Image>(),
            otherFlowerSlots.Find("Icon3").GetComponent<Image>(),
        };
        _flowerPanelAnimator = flowerSlotRoot.Find("MaskPanel").GetComponentInChildren<Animator>();
        _flowerCountText = flowerSlotRoot.Find("Count").GetComponent<TextMeshProUGUI>();
        _combatKeyBindTMPs[4] = flowerSlotRoot.Find("MaskPanel").GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void HideAllInGameUI()
    {
        if (_inGameCombatUI == null) return;
        _focusedOverlay.SetActive(false);
        _defeatedUI.SetActive(false);
        _inGameCombatUI.SetActive(false);
        _zoomedMap.SetActive(false);
        _bookUI.SetActive(false);
        _metaUpgradeUI.SetActive(false);
    }

    public void DestroyAllInGameUI()
    {
        if (_inGameCombatUI == null) return;
        Destroy(_focusedOverlay);
        Destroy(_defeatedUI);
        Destroy(_inGameCombatUI);
        Destroy(_zoomedMap);
        Destroy(_bookUI);
        Destroy(_metaUpgradeUI);
    }
    
    private void OnGameLoadEnded()
    {
        _bloodOverlayLowHP.gameObject.SetActive(false);
        _tensionOverlay.gameObject.SetActive(false);
        _zoomedMap.SetActive(false);
        _loadingScreenUI.SetActive(false);
        _inGameCombatUI.SetActive(true);
        _flowerUIDisplayTime = 0;
        _bloodOverlayHitDisplayTime = 0;

        ESceneType currSceneType = _gameManager.ActiveScene;
        switch (currSceneType)
        {
            case ESceneType.CombatMap0:
                DisplayRoomGuideUI("임시 메타맵", "");
                DisableMap();
                break;
            
            case ESceneType.Boss:
                var hpRatio = _playerController.playerDamageReceiver.GetHPRatio();
                OnPlayerHPChanged(0,hpRatio,hpRatio);
                return;
        }
        
        OnPlayerHPChanged(0,1,1);
        UpdateDarkGaugeUI(0);
        UsePlayerControl();
    }

    private void OnCombatSceneChanged()
    {
        // Minimap and zoomed map disabled in the boss map
        DisableMap();
        
        // Update combat UI with bound legacy information
        //_playerAttackManager.UpdateLegacyUI();
        
        _uiCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _activeFocusedUI = null;
        _inGameCombatUI.GetComponent<Canvas>().worldCamera = _uiCamera;
    }
    
    private void UseUIControl()
    {
        PlayerIAMap.Disable();
        UIIAMap.Enable();
    }

    private void UsePlayerControl()
    {
        PlayerIAMap.Enable();
        UIIAMap.Disable();
        _uiInputModule.point = _playerPointIARef;
        _uiInputModule.leftClick = _playerClickIARef;
    }

    public void UpdateDarkGaugeUI(float value)
    {
        _darkGaugeSlider.value = value / _playerController.playerDamageDealer.DarkGaugeMax;
        _darkGaugeText.text = $"{value}/{_playerController.playerDamageDealer.DarkGaugeMax}";
    }

    public void IncrementTensionGaugeUI() => TensionController.IncrementTension();
    
    private void OnPlayerHPChanged(float changeAmount, float oldHpRatio, float newHpRatio)
    {
        // Update hp globe
        _playerHPGlobe.rectTransform.localPosition = new Vector3(
            0, _playerHPGlobe.rectTransform.rect.height * newHpRatio - _playerHPGlobe.rectTransform.rect.height, 0);
        
        // Update hp text
        float maxHp = _playerController.playerDamageReceiver.MaxHealth;
        float hp = newHpRatio * maxHp;
        _hpText.text = Utility.FormatFloat(hp) + "/" + Utility.FormatFloat(maxHp);
        
        // Show hit blood overlay if damaged
        if (changeAmount < 0) StartCoroutine(nameof(BloodOverlayHitCoroutine));
        
        // If HP previously below threshold and just moved over threshold, turn off low hp blood overlay
        float critHpRatio = _playerController.HpCriticalThreshold;
        if (oldHpRatio <= critHpRatio) 
        {
            if (newHpRatio > critHpRatio)
            {
                _bloodOverlayLowHP.gameObject.SetActive(false);
                StopCoroutine(nameof(BloodOverlayLowHPCoroutine));
            }
            // When player dies, stop blinking the overlay
            else if (newHpRatio == 0.0f)
            {
                StopCoroutine(nameof(BloodOverlayLowHPCoroutine));
            }
        }
        // If HP previously over threshold and just moved below threshold, turn on blood overlay
        else if (newHpRatio <= critHpRatio)
        {
            _bloodOverlayLowHP.gameObject.SetActive(true);
            StartCoroutine(nameof(BloodOverlayLowHPCoroutine));
        }
    }

    private IEnumerator BloodOverlayLowHPCoroutine()
    {
        float duration = 1;
        var minColour = new Color(0, 0, 0, 0);
        var midColour = new Color(1, 1, 1, 0.5f);
        var maxColour = Color.white;
        
        // Increase alpha to 0.5
        for (float time = 0; time < duration; time += Time.unscaledDeltaTime)
        {
            float progress = Mathf.Lerp(0, time, duration);
            _bloodOverlayLowHP.color = Color.Lerp(minColour, midColour, progress);
            yield return null;
        }
        
        // Alternate between 0.5 and 1.0 alpha
        while (true)
        {
            for (float time = 0; time < duration * 2; time += Time.unscaledDeltaTime)
            {
                float progress = Mathf.PingPong(time, duration) / duration;
                _bloodOverlayLowHP.color = Color.Lerp(midColour, maxColour, progress);
                yield return null;
            }
        }
    }

    private IEnumerator BloodOverlayHitCoroutine()
    {
        float duration = 0.5f;
        _bloodOverlayHitDisplayTime += duration; 
        _bloodOverlayHit.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(duration);
        
        _bloodOverlayHitDisplayTime -= duration;
        if (_bloodOverlayHitDisplayTime <= 0)
        {
            _bloodOverlayHit.gameObject.SetActive(false);
        }
    }
    
    private IEnumerator GameOverCoroutine()
    {
        yield return new WaitForSeconds(Define.GameOverDelayTime);
        OpenFocusedUI(_defeatedUI);
    }

    private void OnPlayerDefeated(bool isRealDeath)
    {
        StopCoroutine(nameof(BloodOverlayLowHPCoroutine));
        CloseFocusedUI();
        PlayerIAMap.Disable();
        if (isRealDeath) StartCoroutine(GameOverCoroutine());
    }
    
    private void OnPlayerStartResurrect()
    {
        StopCoroutine(nameof(BloodOverlayLowHPCoroutine));
        StopCoroutine(nameof(BloodOverlayHitCoroutine));
        CloseFocusedUI();
        PlayerIAMap.Disable();
    }
    
    private void OnPlayerEndResurrect()
    {
        UsePlayerControl();
    }
    
    private void LoadAllUIPrefabs()
    {
        string path = "Prefabs/UI/";
        _loadingScreenPrefab    = Utility.LoadGameObjectFromPath(path + "LoadingCanvas");
        _settingsPrefab         = Utility.LoadGameObjectFromPath(path + "SettingsCanvas");
        _creditsUIPrefab        = Utility.LoadGameObjectFromPath(path + "CreditsCanvas");
        _focusedOverlayPrefab   = Utility.LoadGameObjectFromPath(path + "InGame/FocusedCanvas");
        _defeatedUIPrefab       = Utility.LoadGameObjectFromPath(path + "InGame/DefeatedCanvas");
        _inGameCombatPrefab     = Utility.LoadGameObjectFromPath(path + "InGame/CombatCanvas");
        _zoomedMapPrefab        = Utility.LoadGameObjectFromPath(path + "InGame/ZoomedMap");
        _evadePopupPrefab       = Utility.LoadGameObjectFromPath(path + "InGame/TextPopUp/EvadeUI");
        _textPopupPrefab        = Utility.LoadGameObjectFromPath(path + "InGame/TextPopUp/GeneralTextUI");
        _critPopupPrefab        = Utility.LoadGameObjectFromPath(path + "InGame/TextPopUp/CritPopUp");
        _soulPopupPrefab        = Utility.LoadGameObjectFromPath(path + "InGame/TextPopUp/SoulPopUp");
        _goldMinusPopupPrefab   = Utility.LoadGameObjectFromPath(path + "InGame/TextPopUp/GoldMinusPopUp");
        _goldPlusPopupPrefab    = Utility.LoadGameObjectFromPath(path + "InGame/TextPopUp/GoldPlusPopUp");
        _bookPrefab             = Utility.LoadGameObjectFromPath(path + "InGame/Book/BookCanvas");
        _metaUpgradeUIPrefab    = Utility.LoadGameObjectFromPath(path + "InGame/MetaUpgradeCanvas");
        _roomGuidePrefab        = Utility.LoadGameObjectFromPath(path + "InGame/RoomGuideUI");
        _timerPrefab            = Utility.LoadGameObjectFromPath(path + "InGame/TimerUI");
        
        _warriorUIPrefabs = new GameObject[(int)EWarrior.MAX];
        for (int i = 0; i < (int)EWarrior.MAX; i++)
        {
            _warriorUIPrefabs[i] = Utility.LoadGameObjectFromPath(path + "InGame/Warrior/" + (EWarrior)i);
        }
        _loadingScreenUI = Instantiate(_loadingScreenPrefab, Vector3.zero, Quaternion.identity).gameObject;
        _loadingScreenUI.SetActive(false);
        _settingsUI = Instantiate(_settingsPrefab, Vector3.zero, Quaternion.identity).gameObject;
        _settingsUI.SetActive(false);
        _settingsUIController = _settingsUI.GetComponent<SettingsUIController>();
        DontDestroyOnLoad(_loadingScreenUI);
        DontDestroyOnLoad(_settingsUI);
    }

    public void UpdateCombatKeyBindText()
    {
        // Inputs
        var inputBindings = new InputBinding[]
        {
            _playerController.playerInput.actions["Attack_Melee"].bindings[0],
            _playerController.playerInput.actions["Attack_Range"].bindings[0],
            _playerController.playerInput.actions["Attack_Dash"].bindings[0],
            _playerController.playerInput.actions["Attack_Area"].bindings[0],
            _playerController.playerInput.actions["SelectNextFlowerBomb"].bindings[0],
        };
        
        for (int i = 0; i < 5; i++)
        {
            _combatKeyBindTMPs[i].text = inputBindings[i].ToDisplayString();
        }
    }

    public void DisplayLoadingScreen()
    {
        _loadingScreenUI.SetActive(true);
    }
    
    public void HideLoadingScreen()
    {
        _loadingScreenUI.SetActive(false);
    }

    private void OnMainMenuLoaded()
    {
        _mainMenuUIController = FindObjectOfType<MainMenuUIController>();
        _mainMenuUI = _mainMenuUIController.GameObject();
        OpenFocusedUI(_mainMenuUI);
    }

    private void OnPlayerSpawned()
    {
        if (!_gameManager.isFirstRun) return;
        _playerAttackManager = PlayerAttackManager.Instance;
        _playerController = PlayerController.Instance;
        _playerHPSlider.value = 1;
        UsePlayerControl();
    }

    private void OpenFocusedUI(GameObject uiObject, bool shouldShowOverlay = false)
    {
        if (shouldShowOverlay && _focusedOverlay) _focusedOverlay.SetActive(true);
        UseUIControl();
        _activeFocusedUI = uiObject;
        _activeFocusedUIController = uiObject.GetComponentInChildren<UIControllerBase>();
        _uiInputModule.point = _uiPointIARef;
        _uiInputModule.leftClick = _uiClickIARef;
        uiObject.SetActive(true);
    }

    public void CloseFocusedUI()
    {
        if (!_activeFocusedUI) return;
        PlayerIAMap.Enable();
        if (_activeFocusedUI == _warriorUIObject) Destroy(_warriorUIObject);
        
        UIIAMap.Disable();
        _focusedOverlay.SetActive(false);
        _activeFocusedUI.SetActive(false);
        _activeFocusedUI = null;
        _activeFocusedUIController = null;
        _uiInputModule.point = _playerPointIARef;
        _uiInputModule.leftClick = _playerClickIARef;
    }

    public bool OpenWarriorUI(Clockwork interactor, bool isPristineClockwork = false)
    {
        if (_activeFocusedUI) return false; 
        
        _warriorUIObject = Instantiate(_warriorUIPrefabs[(int)interactor.warrior], Vector3.zero, Quaternion.identity).GameObject();
        _warriorUIObject.GetComponent<Canvas>().worldCamera = _uiCamera;
        _warriorUIController = _warriorUIObject.GetComponent<WarriorUIController>();
        _warriorUIController.Initialise(interactor.warrior, isPristineClockwork);
        OpenFocusedUI(_warriorUIObject);
        Destroy(interactor.gameObject);
        return true;
    }

    public SettingsUIController OpenSettings()
    {
        _settingsUI.SetActive(true);
        return _settingsUIController;
    }
    
    public void OpenMetaUpgradeUI()
    {
        if (_activeFocusedUI == null) OpenFocusedUI(_metaUpgradeUI, true);
    }

    public void OpenBook()
    {
        _audioManager.LowerBGMVolumeUponUI();
        OpenFocusedUI(_bookUI, true);
    }
    
    private void OnTab(InputAction.CallbackContext obj)
    {
        if (_activeFocusedUIController) _activeFocusedUIController.OnTab();
    }

    public void OpenMap()
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
        if (_activeFocusedUI == _warriorUIObject || _activeFocusedUI == _bookUI || _activeFocusedUI == _mainMenuUI)
        {
            _activeFocusedUIController.OnClose();
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
        if (_activeFocusedUIController) _activeFocusedUIController.OnNavigate(value);
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
        else if (_activeFocusedUI == _mainMenuUI)
        {
            _mainMenuUIController.OnReset();
        }
        else if (_activeFocusedUI == _bookUI)
        {
            _bookUIController.OnReset();
        }
    }

    private void OnSubmit(InputAction.CallbackContext obj)
    {
        if (_activeFocusedUIController) _activeFocusedUIController.OnSubmit();
    }
    
    // InGame popup
    public void DisplayTextPopUp(string text, Vector3 position, Transform parent = null)
    {
        var ui = Instantiate(_textPopupPrefab, position + playerUpOffset, quaternion.identity);
        ui.GetComponent<TextPopUpUI>().Init(parent, text);
    }
    private Vector3 playerUpOffset = new Vector3(0, 2.3f, 0);
    private readonly static int ShowFlowerPanel = Animator.StringToHash("Show");
    private readonly static int HideFlowerPanel = Animator.StringToHash("Hide");

    // InGame popup - crit
    public void DisplayCritPopUp(Vector3 position)
    {
        var ui = Instantiate(_critPopupPrefab, position + playerUpOffset, quaternion.identity);
        ui.GetComponent<TextPopUpUI>().Init(null, string.Empty, 0.4f);
    }
    
    public void DisplayPlayerEvadePopUp()
    {
        Instantiate(_evadePopupPrefab, _playerController.transform.position + playerUpOffset, quaternion.identity);
    }
    
    // InGame popup - soul
    public void DisplaySoulPopUp(int value)
    {
        var valueText = value < 0 ? value.ToString() : "+" + value;
        var popup = Instantiate(_soulPopupPrefab);
        popup.GetComponent<RectTransform>().position = _playerController.transform.position + playerUpOffset;
        popup.GetComponentInChildren<TextMeshProUGUI>().text = valueText;
        Destroy(popup, 2.5f);
    }
    
    // InGame popup - gold
    public void DisplayGoldPopUp(int value)
    {
        var valueText = value < 0 ? value.ToString() : "+" + value;
        var popup = Instantiate(value < 0 ? _goldMinusPopupPrefab : _goldPlusPopupPrefab);
        popup.GetComponent<RectTransform>().position = _playerController.transform.position + playerUpOffset;
        popup.GetComponentInChildren<TextMeshProUGUI>().text = valueText;
        Destroy(popup, 1f);
    }

    // Flower
    private Color _noFlowerColour = new Color(1, 1, 1, 0.15f);
    private int[] _flowerIndices = new int[]{1,2,3,4};
    public void UpdateFlowerBombUI(int selectedFlowerIdx, int flowerCount)
    {
        // Compute indices
        _flowerIndices[0] = selectedFlowerIdx;
        _flowerIndices[1] = (selectedFlowerIdx % 4) + 1;
        _flowerIndices[2] = (selectedFlowerIdx + 1) % 4 + 1;
        _flowerIndices[3] = (selectedFlowerIdx + 2) % 4 + 1;
        
        // Update icons respectively
        for (int i = 0; i < 4; i++)
        {
            _flowerIconImages[i].sprite = flowerIconSprites[_flowerIndices[i]];
            _flowerIconImages[i].color = _playerController.playerInventory
                .GetNumberOfFlowers(_flowerIndices[i]) == 0 ? _noFlowerColour : Color.white;
        }
        
        // Update count text
        _flowerCountText.text = flowerCount.ToString();

        // Display flower UI for a while
        StartCoroutine(FlowerBombUICoroutine());
    }

    public void DisplayFlowerBombUI()
    {
        StartCoroutine(FlowerBombUICoroutine());
    }

    public void UpdateFlowerUICount(int flowerIndex, int flowerCount)
    {
        _flowerCountText.text = flowerCount.ToString();
        for (int i = 0; i < 4; i++)
        {
            if (_flowerIndices[i] == flowerIndex)
            {
                _flowerIconImages[i].color = flowerCount == 0 ? _noFlowerColour : Color.white;
                return;
            }
        }
    }

    private IEnumerator FlowerBombUICoroutine()
    {
        // Show left/right UI
        if (_flowerUIDisplayTime == 0)
        {
            _flowerPanelAnimator.SetTrigger(ShowFlowerPanel);
        }
        _flowerUIDisplayTime += 2.0f;
        
        // Wait for some duration
        yield return new WaitForSeconds(2.0f);
        _flowerUIDisplayTime -= 2.0f;
        
        // Hide left/right UI
        if (_flowerUIDisplayTime == 0.0f)
        {
            _flowerPanelAnimator.SetTrigger(HideFlowerPanel);
        }
    }
    
    // Portal UIs
    public void DisplayRoomGuideUI(string roomName, string description)
    {
        var ui = Instantiate(_roomGuidePrefab, Vector3.zero, Quaternion.identity).GetComponent<RoomGuideUI>();
        ui.InitRoomGuide(roomName, description);
    }
    
    public void DisableMap()
    {
        _minimap.SetActive(false);
        _minimap.transform.parent.gameObject.SetActive(false);
        _playerController.IsMapEnabled = false;
    }
    
    public void EnableMap()
    {
        _minimap.SetActive(true);
        _minimap.transform.parent.gameObject.SetActive(true);
        _playerController.IsMapEnabled = true;
    }

    public GameObject GetInGameCombatUI() => _inGameCombatUI;
    
    public void SaveActionRebinds()
    {
        var uiRebinds = UIIAMap.SaveBindingOverridesAsJson();
        var playerRebinds = PlayerIAMap.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("uiRebinds", uiRebinds);
        PlayerPrefs.SetString("playerRebinds", playerRebinds);
    }

    public void LoadActionRebinds()
    {
        var uiRebinds = PlayerPrefs.GetString("uiRebinds");
        var playerRebinds = PlayerPrefs.GetString("playerRebinds");
        if (!string.IsNullOrEmpty(uiRebinds))
            UIIAMap.LoadBindingOverridesFromJson(uiRebinds);
        if (!string.IsNullOrEmpty(playerRebinds))
            PlayerIAMap.LoadBindingOverridesFromJson(playerRebinds);
    }

    public GameObject DisplayCountdownUI()
    {
        return Instantiate(_timerPrefab, Vector3.zero, Quaternion.identity);
    }

    private void OnBossSlayed() => StartCoroutine(GameEndCoroutine());
    private IEnumerator GameEndCoroutine()
    {
        _audioManager.StopBgm(1f);
        SaveSystem.SaveMetaData();
        yield return new WaitForSecondsRealtime(5f);
        
        PlayerEvents.Defeated.Invoke(false);
        _gameManager.PlayerMetaData.isDirty = true;
        _gameManager.PlayerMetaData.numSouls = _playerController.playerInventory.SoulShard;
        
        var creditsUI = DisplayCreditsUI();
        DontDestroyOnLoad(creditsUI.gameObject);
        creditsUI.gameObject.SetActive(true);
        PlayerIAMap.Disable();
        UIIAMap.Disable();
        yield return new WaitForSecondsRealtime(1.5f);
        
        _gameManager.LoadMainMenu();
    }

    public CreditsUI DisplayCreditsUI()
    {
        return Instantiate(_creditsUIPrefab, Vector3.zero, Quaternion.identity).GetComponent<CreditsUI>();
    }
}
