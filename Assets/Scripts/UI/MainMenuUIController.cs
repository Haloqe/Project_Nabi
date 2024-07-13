using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class MainMenuUIController : UIControllerBase
{
    // References
    private Animator _animator;
    private GameManager _gameManager;
    private UIManager _uiManager;
    private AudioManager _audioManager;
    private GameObject _newGameConfirmPanel;
    
    // Confirm
    private TextMeshProUGUI[] _newGameConfirmTMPs; // 0: No, 1: Yes
    private int _selectedConfirmOption;
    private Color _confirmSelectedColour;
    
    // Settings
    private SettingsUIController _settingsUIController;
    private bool _isSettingsActive;
    
    // Credits
    private CreditsUI _creditsUI;
    
    // 0: New game, 1: Continue, 2: Settings, 3: Quit
    private TextMeshProUGUI[] _options;
    private int _selectedOptionIdx;
    private bool _hasSaveData;
    private bool _underTransition;
    
    // Colours
    private Color _selectedColour;
    private Color _unselectedColour;
    private Color _unavailableColour;
    private Coroutine _colourChangeCoroutine;
    
    // SFX
    private AudioSource _audioSource;
    private readonly static int Revert = Animator.StringToHash("Revert");
    private readonly static int Transition = Animator.StringToHash("Transition");

    private void Awake()
    {
        // Colours
        _selectedColour = new Color(0.391f, 0.696f, 0.707f, 1f);
        _unselectedColour = Color.white;
        _unavailableColour = new Color(0.330f, 0.330f, 0.330f, 1f);
        _confirmSelectedColour = new Color(0.9f, 0.7f, 0, 1);
        
        // Initialise
        _options = transform.Find("Options").GetComponentsInChildren<TextMeshProUGUI>();
        for (int i = 0; i < _options.Length; i++)
        {
            _options[i].GetComponent<MainMenuButton>().Init(this, i);
            _options[i].color = _unselectedColour;
        }
        
        _selectedOptionIdx = 0;
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        
        // Confirm panels
        _newGameConfirmPanel = gameObject.transform.Find("NewGameConfirmPanel").gameObject;
        _newGameConfirmTMPs = _newGameConfirmPanel.transform.Find("Options").GetComponentsInChildren<TextMeshProUGUI>();
        GameEvents.MainMenuLoaded += SelectInitialOption;
    }

    private void OnDestroy()
    {
        GameEvents.MainMenuLoaded -= SelectInitialOption;
    }

    private void SelectInitialOption()
    {
        SelectOption(_hasSaveData ? 1 : 0, false);
    }

    private void Start()
    {
        // References
        _gameManager = GameManager.Instance;
        _uiManager = UIManager.Instance;
        _audioManager = AudioManager.Instance;
        
        // Initialise ui
        _hasSaveData = _gameManager.PlayerMetaData.isDirty;
        _options[1].color = _hasSaveData ? _unselectedColour : _unavailableColour;
        
        // Credits
        _creditsUI = _uiManager.DisplayCreditsUI();
        _creditsUI.BaseUIController = this;
    }

    private void SelectOption(int newOptionIdx, bool shouldPlaySound = true)
    {
        if (_underTransition) return;
        if (newOptionIdx == 1 && !_hasSaveData) return;
        
        if (_selectedOptionIdx != newOptionIdx && shouldPlaySound) _audioSource.Play();
        UnselectCurrentOption();
        _options[newOptionIdx].color = _selectedColour;
        _options[newOptionIdx].text = "> " + LocalizationSettings.StringDatabase.GetLocalizedString("UIStringTable", 
                "MainMenu_Option_" + newOptionIdx, LocalizationSettings.SelectedLocale);
        _selectedOptionIdx = newOptionIdx;
        _colourChangeCoroutine = StartCoroutine(ColourChangeCoroutine());
    }

    private void UnselectCurrentOption()
    {
        if (_colourChangeCoroutine != null) StopCoroutine(_colourChangeCoroutine);
        _options[_selectedOptionIdx].text = LocalizationSettings.StringDatabase.GetLocalizedString("UIStringTable", 
            "MainMenu_Option_" + _selectedOptionIdx, LocalizationSettings.SelectedLocale);
        _options[_selectedOptionIdx].color = _unselectedColour;
    }
    
    private IEnumerator ColourChangeCoroutine()
    {
        var added = _selectedColour + _unselectedColour;
        var mid = new Color(added.r/2, added.g/2, added.b/2, 1);
        float duration = 1;
        var targetTMP = _options[_selectedOptionIdx];
        
        // Alternate between two colours
        while (true)
        {
            for (float time = 0; time < duration * 2; time += Time.unscaledDeltaTime)
            {
                float progress = Mathf.PingPong(time, duration) / duration;
                targetTMP.color = Color.Lerp(_selectedColour, mid, progress);
                yield return null;
            }
        }
    }

    public override void OnNavigate(Vector2 value)
    {
        if (UIManager.Instance.isRunningEndingCredits) return;
        if (_underTransition) return;
        
        // Confirm panel
        if (_newGameConfirmPanel.activeSelf)
        {
            OnNavigateConfirmPanel(value.x);
            return;
        }
        
        // Settings
        if (_isSettingsActive)
        {
            _settingsUIController.OnNavigate(value);
            return;
        }
        
        // Base
        switch (value.y)
        {
            // Left or Right
            case 0:
                return;
            // Up
            case > 0:
                {
                    if (_selectedOptionIdx == 0) SelectOption(4);
                    else if (_selectedOptionIdx == 2 && !_hasSaveData) SelectOption(0);
                    else SelectOption(_selectedOptionIdx - 1);
                    break;
                }
            // Down
            case < 0:
                {
                    if (_selectedOptionIdx == 4) SelectOption(0);
                    else if (_selectedOptionIdx == 0 && !_hasSaveData) SelectOption(2);
                    else SelectOption(_selectedOptionIdx + 1);
                    break;
                }
        }
    }
    
    public override void OnSubmit()
    {
        if (_uiManager.isRunningEndingCredits) return;
        if (_underTransition) return;

        // Confirm Panel
        if (_newGameConfirmPanel.activeSelf)
        {
            _newGameConfirmPanel.SetActive(false);
            if (_selectedConfirmOption == 1) // Restart
            {
                StartHideTransition();
            }
            return;
        }
        
        // Settings
        if (_isSettingsActive)
        {
            _settingsUIController.OnSubmit();
            return;
        }
        
        // Basic main menu
        switch (_selectedOptionIdx)
        {
            case 0: // New Game
                if (_hasSaveData)
                {
                    _newGameConfirmPanel.SetActive(true);
                    SelectConfirmOption(0);
                    return;
                }
                StartHideTransition(0.5f);
                break;
            
            case 1: // Continue
                StartHideTransition(stopBgm:true);
                break;
            
            case 2: // Settings
                StartHideTransition(stopBgm:false);
                break;
            
            case 3: // Credits
                StartHideTransition(stopBgm:false);
                break;
            
            case 4: // Quit
                _gameManager.QuitGame();
                break;
        }
    }
    
    public override void OnClose()
    {
        if (_isSettingsActive)
        {
            _settingsUIController.OnClose();
            return;
        }
        if (_creditsUI.gameObject.activeSelf)
        {
            _creditsUI.OnCancel();
            return;
        }
        if (_newGameConfirmPanel.activeSelf) _newGameConfirmPanel.SetActive(false);
    }
    
    public override void OnTab()
    {
        if (_isSettingsActive) _settingsUIController.OnTab();
    }

    public void OnPointerEnter(int optionIdx) => SelectOption(optionIdx);
    public void OnPointerClick() => OnSubmit();

    private void StartHideTransition(float fadeOutDuration = 1f, bool stopBgm = true)
    {
        if (stopBgm) _audioManager.StopBgm(fadeOutDuration);
        _audioManager.PlayUIConfirmSound();
        _animator.enabled = true;
        _animator.SetTrigger(Transition);
        _underTransition = true;
    }

    public void OnEndHideTransition()
    {
        _animator.ResetTrigger(Transition);
        switch (_selectedOptionIdx)
        {
            case 0: // New Game
                // TODO Opening cutscene
                _gameManager.StartNewGame();
                break;
            
            case 1: // Continue
                _gameManager.ContinueGame();
                break;
            
            case 2: // Settings
                _settingsUIController = _uiManager.OpenSettings();
                _settingsUIController.backgroundImage.color = new Color(0, 0, 0, 0.9f);
                _settingsUIController.parentUI = this;
                _isSettingsActive = true;
                _underTransition = false;
                break;
            
            case 3: // Credits
                _creditsUI.gameObject.SetActive(true);
                _creditsUI.IsClosing = false;
                break;
        }
    }

    public void OnEndShowTransition()
    {
        _underTransition = false;
        _animator.ResetTrigger(Revert);
        _animator.enabled = false;
    }

    public override void OnSettingsClosed()
    {
        _isSettingsActive = false;
        _underTransition = true;
        _animator.SetTrigger(Revert);
    }

    public void OnCreditsClosed()
    {
        _creditsUI.gameObject.SetActive(false);
        _underTransition = true;
        _animator.SetTrigger(Revert);
    }
    
    // Confirm panel
    private void OnNavigateConfirmPanel(float x)
    {
        if (x == 0) return;
        
        _audioSource.Play();
        UnselectPreviousConfirmOption();
        if (x < 0) // left
        {
            SelectConfirmOption(_selectedConfirmOption == 1 ? 0 : 1);
        }
        else // right
        {
            SelectConfirmOption(_selectedConfirmOption == 0 ? 1 : 0);
        }
    }

    private void SelectConfirmOption(int idx)
    {
        _newGameConfirmTMPs[idx].color = _confirmSelectedColour;
        _newGameConfirmTMPs[idx].text = "> " + LocalizationSettings.StringDatabase.GetLocalizedString("UIStringTable",
            idx == 0 ? "Option_No" : "Option_Yes") + " <";
        _selectedConfirmOption = idx;
    }
    
    private void UnselectPreviousConfirmOption()
    {
        _newGameConfirmTMPs[_selectedConfirmOption].color = Color.white;
        _newGameConfirmTMPs[_selectedConfirmOption].text = LocalizationSettings.StringDatabase.GetLocalizedString("UIStringTable",
            _selectedConfirmOption == 0 ? "Option_No" : "Option_Yes");
    }
    
    public void OnReset()
    {
        if (_isSettingsActive) _settingsUIController.OnReset();
    }
}
