using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class SettingsUIController : UIControllerBase
{
    // Parent
    public UIControllerBase parentUI;
    
    // Settings group
    private int _curSettingsGroup; // 0: 일반 1: 조작 2: 사운드
    [SerializeField] private Image[] settingGroupBackgrounds;
    [SerializeField] private TextMeshProUGUI[] settingGroupTMPs;
    
    // Setting Details
    private int _curDetailSettingIdx;
    [SerializeField] private GameObject[] settingDetails;
    [SerializeField] private Image[] settingImagesGeneral;
    [SerializeField] private Image[] settingImagesControl;
    [SerializeField] private Image[] settingImagesSound;
    private Image[][] _settingDetailImages;
    
    // 0 - General
    private Color _unselectedDetailTextColor;
    private Color _selectedDetailTextColor;
    private int _selectedResolutionIdx;
    private int _selectedLanguageIdx;
    public Toggle fullscreenToggle;
    public TextMeshProUGUI[] generalResolutionTMPs;
    public TextMeshProUGUI[] generalLanguageTMPs;
    [SerializeField] private GameObject coverCanvas;
    
    // 1 - Control
    private Color _dupRebindColour;
    private bool _isPointingLabel;
    private bool[] _isUniqueBindings;
    private RebindActionUI[] _rebindActionUis;
    private TextMeshProUGUI[] _rebindTexts;
    private GameObject _dupWarningText;
    private bool _canSaveRebinds = true;
    
    // 2 - Sound
    private Toggle _muteToggle;
    private Slider[] _soundSliders;
    
    // Colours
    public Image backgroundImage;
    private Color _unselectedBgColour;
    private Color _selectedBgColour;
    private Color _unselectedTextColour;
    private Color _selectedTextColour;

    private void Awake()
    {
        // Colours
        _unselectedBgColour = new Color(0.0540f, 0.0997f, 0.1226f, 0.9020f);
        _selectedBgColour = new Color(0.6478f, 0.8775f, 1f, 0.0431f);
        _unselectedTextColour = Color.white;
        _selectedTextColour = new Color(0.4311f, 0.5934f, 0.7924f, 1);
        _unselectedDetailTextColor = new Color(0.45f, 0.45f, 0.45f, 1);
        _selectedDetailTextColor = new Color(0.476f, 0.774f, 0.8f, 1);
        
        // Get references from details
        _settingDetailImages = new Image[][]
        {
            settingImagesGeneral, settingImagesControl, settingImagesSound,
        };
        
        // 1 - Control
        _dupWarningText = settingDetails[1].transform.Find("DupWarningText").gameObject;
        _dupRebindColour = new Color(0.65f, 0f, 0f, 1f);
        _rebindActionUis = settingDetails[1].GetComponentsInChildren<RebindActionUI>();
        _rebindTexts = new TextMeshProUGUI[_rebindActionUis.Length];
        for (int i = 0; i < _rebindActionUis.Length; i++)
        {
            _rebindTexts[i] = _rebindActionUis[i].bindingText;
        }
        
        // 2 - Sound
        _muteToggle = settingImagesSound[0].transform.GetComponentInChildren<Toggle>();
        _soundSliders = new Slider[settingImagesSound.Length];
        for (int i = 0; i < settingImagesSound.Length; i++)
        {
            _soundSliders[i] = settingImagesSound[i].transform.GetComponentInChildren<Slider>();
        }
    }

    private void Start()
    {
        // TODO
        // get data from savefile
        LoadSoundSaveData();
        coverCanvas.SetActive(false);
        
        foreach (var rebindUI in _rebindActionUis)
        {
            rebindUI.UpdateBindingDisplay();
        }
        
        // General
        // 0 - General
        for (int i = 0; i < GameManager.Instance.ResolutionOptions.Length; i++)
        {
            if (!GameManager.Instance.IsResolutionSupported[i])
            {
                generalResolutionTMPs[i].color = new Color(0.1132075f, 0.1132075f, 0.1132075f, 1);
            }
        }
        SelectLanguage(LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale));
        fullscreenToggle.isOn = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
        SelectResolution(GameManager.Instance.ResolutionIndex);
    }

    private void LoadGeneralSaveData()
    {
        GameManager.Instance.ResetGeneralSettingsToDefault();
        SelectLanguage(LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale));
        fullscreenToggle.isOn = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
        SelectResolution(GameManager.Instance.ResolutionIndex);
    }
    
    private void LoadSoundSaveData()
    {
        _muteToggle.isOn = AudioManager.Instance.SoundSettingsData.isMuted;
        _soundSliders[1].value = AudioManager.Instance.ConvertAudioMixerVolumeToFloat(0);
        _soundSliders[2].value = AudioManager.Instance.ConvertAudioMixerVolumeToFloat(1);
        _soundSliders[3].value = AudioManager.Instance.ConvertAudioMixerVolumeToFloat(2);
        _soundSliders[4].value = AudioManager.Instance.ConvertAudioMixerVolumeToFloat(3);
        _soundSliders[5].value = AudioManager.Instance.ConvertAudioMixerVolumeToFloat(4);
    }

    private void OnEnable()
    {
        UnselectSettingsGroup();
        SelectSettingsGroup(0);
    }

    public override void OnNavigate(Vector2 value)
    {
        if (value.x == 0)
        {
            if (_curSettingsGroup == 2 && _isNavigatingSound)
            {
                StopAllCoroutines();
                _isNavigatingSound = false;
            }
        }
        
        // Change option details
        if (value.y != 0)
        {
            // Cleanup
            UnselectSettingDetail();
            if (_isNavigatingSound) StopAllCoroutines();
            _isNavigatingSound = false;
            AudioManager.Instance.PlayUINavigateSound();
            
            int lastIdx = _settingDetailImages[_curSettingsGroup].Length - 1;
            if (value.y > 0) // up
            {
                if (_curDetailSettingIdx == 0) SelectSettingDetail(lastIdx);   
                else SelectSettingDetail(_curDetailSettingIdx - 1);
            }
            else // down
            {
                if (_curDetailSettingIdx == lastIdx) SelectSettingDetail(0);   
                else SelectSettingDetail(_curDetailSettingIdx + 1);
            }
        }
        else if (value.x != 0)
        {
            if (_curSettingsGroup == 0)
            {
                switch (_curDetailSettingIdx)
                {
                    case 0: // Fullscreen toggle
                        break; 
            
                    case 1: // Resolution
                        // Clamp index
                        int resIndex = _selectedResolutionIdx;
                        if (value.x < 0) // left
                        {
                            resIndex = _selectedResolutionIdx - 1;
                            if (resIndex == -1) resIndex = generalResolutionTMPs.Length - 1;
                            while (resIndex != -1 && !GameManager.Instance.IsResolutionSupported[resIndex])
                            {
                                resIndex--;
                            }
                        }
                        else if (value.x > 0) // right
                        {
                            resIndex = _selectedResolutionIdx + 1;
                            if (resIndex == generalResolutionTMPs.Length) resIndex = 0;
                            while (resIndex != generalResolutionTMPs.Length && !GameManager.Instance.IsResolutionSupported[resIndex])
                            {
                                resIndex++;
                            }
                        }
                        if (resIndex != -1 && resIndex != generalResolutionTMPs.Length)
                        {
                            SelectResolution(resIndex);
                        }
                        break;
            
                    case 2: // Language
                        StartCoroutine(SelectLanguageCoroutine(_selectedLanguageIdx + (int)value.x));
                        break;
                }
            }
            else if (_curSettingsGroup == 2 && !_isNavigatingSound)
            {
                StartCoroutine(NavigateSoundCoroutine(_curDetailSettingIdx, value.x));
            }
        }
    }

    private IEnumerator SelectLanguageCoroutine(int localeIndex)
    {
        UIManager.Instance.UIIAMap.Disable();
        coverCanvas.SetActive(true);
        var coverImage = coverCanvas.GetComponent<Image>();
        
        // Fade In
        coverImage.color = new Color(coverImage.color.r, coverImage.color.g, coverImage.color.b, 0);
        while (coverImage.color.a < 1.0f)
        {
            coverImage.color = new Color(coverImage.color.r, coverImage.color.g, coverImage.color.b, coverImage.color.a + Time.unscaledDeltaTime / 0.3f);
            yield return null;
        }
        
        // Change language
        coverImage.color = new Color(coverImage.color.r, coverImage.color.g, coverImage.color.b, 1);
        SelectLanguage(localeIndex);
        yield return new WaitForSeconds(0.7f);

        // Fade Out
        while (coverImage.color.a > 0.0f)
        {
            coverImage.color = new Color(coverImage.color.r, coverImage.color.g, coverImage.color.b, coverImage.color.a - Time.unscaledDeltaTime / 0.3f);
            yield return null;
        }
        coverImage.color = new Color(coverImage.color.r, coverImage.color.g, coverImage.color.b, 0);
        
        UIManager.Instance.UIIAMap.Enable();
    }

    private void SelectResolution(int resolutionIndex)
    {
        // Select resolution
        generalResolutionTMPs[_selectedResolutionIdx].color = _unselectedDetailTextColor;
        _selectedResolutionIdx = resolutionIndex;
        generalResolutionTMPs[_selectedResolutionIdx].color = _selectedDetailTextColor;
        GameManager.Instance.SetResolution(_selectedResolutionIdx);
    }

    private void SelectLanguage(int localeIndex)
    {
        // Clamp index
        if (localeIndex == -1) localeIndex = generalLanguageTMPs.Length - 1;
        else if (localeIndex == generalLanguageTMPs.Length) localeIndex = 0; 
        
        // Select language
        generalLanguageTMPs[_selectedLanguageIdx].color = _unselectedDetailTextColor;
        _selectedLanguageIdx = localeIndex;
        generalLanguageTMPs[_selectedLanguageIdx].color = _selectedDetailTextColor;
        GameManager.Instance.SetLocale((ELocalisation)_selectedLanguageIdx);
    }
    
    public void OnRebindStop(RebindActionUI ui, InputActionRebindingExtensions.RebindingOperation op)
    {
        _isPointingLabel = true;
        
        // Check if the binding is unique
        IsBindingUniqueAll();
        
        // Extra actions for OpenMap and Interact
        if (_curDetailSettingIdx != 8 && _curDetailSettingIdx != 9) return;
        InputAction targetRelatedAction = null;
        if (_curDetailSettingIdx == 8) // Interact
        {
            targetRelatedAction = UIManager.Instance.PlayerIAMap.FindAction("Interact_Hold");
        }
        else if (_curDetailSettingIdx == 9) // Map
        {
            targetRelatedAction = UIManager.Instance.UIIAMap.FindAction("CloseMap");
        }
        var newBinding = targetRelatedAction.bindings[0];
        newBinding.overridePath = op.action.bindings[0].overridePath;
        targetRelatedAction.ApplyBindingOverride(0, newBinding);
    }

    private void IsBindingUniqueAll()
    {
        _canSaveRebinds = true;
        for (int i = 0; i < _rebindActionUis.Length; i++)
        {
            if (IsBindingUnique(_rebindActionUis[i].actionReference))
            {
                _rebindTexts[i].color = Color.black;
            }
            else
            {
                // Duplicate binding exists
                _rebindTexts[i].color = _dupRebindColour;
                _canSaveRebinds = false;
            }
        }
        _dupWarningText.SetActive(!_canSaveRebinds);
    }
    
    private bool IsBindingUnique(InputAction actionToRebind)
    {
        var newBinding = actionToRebind.bindings[0];
 
        var bindings = actionToRebind.actionMap.bindings;
        foreach (var binding in bindings)
        {
            if (binding.action == newBinding.action)
                continue;
            if (binding.effectivePath == newBinding.effectivePath)
            {
                if (newBinding.action == "Interact" && binding.action == "Interact_Hold") continue;
                return false;
            }
        }
        return true;
    }

    private bool _isNavigatingSound = false;
    private IEnumerator NavigateSoundCoroutine(int idx, float x)
    {
        _isNavigatingSound = true;
        while (true)
        {
            _soundSliders[idx].value = Mathf.Clamp(_soundSliders[idx].value + x * 0.005f, 0.0001f, 1);
            yield return null;
        }
    }

    public void OnSoundSliderValueChanged(int idx)
    {
        AudioManager.Instance.SetAudioMixerVolumeByFloat(idx, _soundSliders[idx + 1].value);
    }

    public void OnMuteToggleChanged()
    {
        if (_muteToggle.isOn) AudioManager.Instance.Mute();
        else AudioManager.Instance.Unmute();
    }
    
    public void OnFullscreenToggleChanged()
    {
        GameManager.Instance.SetFullscreen(fullscreenToggle.isOn);
    }
    
    public override void OnSubmit()
    {
        switch (_curSettingsGroup)
        {
            case 0:
                if (_curDetailSettingIdx == 0)
                {
                    fullscreenToggle.isOn = !fullscreenToggle.isOn;
                }
                break;
            
            case 1:
                if (_isPointingLabel)
                {
                    _isPointingLabel = false;
                    _rebindActionUis[_curDetailSettingIdx].StartInteractiveRebind();
                }
                break;
            
            case 2:
                if (_curDetailSettingIdx == 0)
                {
                    _muteToggle.isOn = !_muteToggle.isOn;
                }
                break;
        }
    }
    
    public override void OnClose()
    {
        if (!_canSaveRebinds) return; // Todo warning
        
        // Cleanup
        if (_isNavigatingSound) StopAllCoroutines();
        _isNavigatingSound = false;
        
        // Save settings
        GameManager.Instance.SaveGeneralSettings();
        AudioManager.Instance.SaveAudioSettings();
        UIManager.Instance.SaveActionRebinds();
        
        // Return to parent
        if (parentUI != null) parentUI.OnSettingsClosed();
        parentUI = null;
        gameObject.SetActive(false);
    }

    public override void OnTab()
    {
        if (_curSettingsGroup == 1 && !_canSaveRebinds) return;
        UnselectSettingsGroup();
        SelectSettingsGroup(_curSettingsGroup == 2 ? 0 : _curSettingsGroup + 1); 
        AudioManager.Instance.PlayUINavigateSound();
    }

    public void OnReset()
    {
        switch (_curSettingsGroup)
        {
            case 0:
                LoadGeneralSaveData();
                StartCoroutine(SelectLanguageCoroutine((int)ELocalisation.KOR));
                break;
            
            case 1:
                ResetAllRebinds();
                break;
            
            case 2:
                AudioManager.Instance.ResetAudioSettingsToDefault();
                LoadSoundSaveData();
                break;
        }
    }
    
    private void UnselectSettingsGroup()
    {
        settingGroupBackgrounds[_curSettingsGroup].color = _unselectedBgColour;
        settingGroupTMPs[_curSettingsGroup].color = _unselectedTextColour;
        settingDetails[_curSettingsGroup].SetActive(false);
        UnselectSettingDetail();
    }

    private void SelectSettingsGroup(int idx)
    {
        settingGroupBackgrounds[idx].color = _selectedBgColour;
        settingGroupTMPs[idx].color = _selectedTextColour;
        settingDetails[idx].SetActive(true);
        _curSettingsGroup = idx;
        if (_curSettingsGroup == 1)
        {
            _isPointingLabel = true;
            _curFocusedBottom = false;
            settingDetails[1].transform.Find("ScrollRect").GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        }
        SelectSettingDetail(0);
    }

    private void UnselectSettingDetail()
    {
        _settingDetailImages[_curSettingsGroup][_curDetailSettingIdx].enabled = false;
    }

    private bool _curFocusedBottom;
    private void SelectSettingDetail(int idx)
    {
        _curDetailSettingIdx = idx;
        _settingDetailImages[_curSettingsGroup][_curDetailSettingIdx].enabled = true;
        if (_curSettingsGroup != 1) return;
        
        // Scroll for control settings
        if (_curDetailSettingIdx > 7 && !_curFocusedBottom)
        {
            _curFocusedBottom = true;
            settingDetails[1].transform.Find("ScrollRect").GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
        }
        else if (_curDetailSettingIdx < 3 && _curFocusedBottom)
        {
            _curFocusedBottom = false;
            settingDetails[1].transform.Find("ScrollRect").GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        }
    }

    private void ResetAllRebinds()
    {
        UIManager.Instance.UIIAMap.RemoveAllBindingOverrides();
        UIManager.Instance.PlayerIAMap.RemoveAllBindingOverrides();
    }
}
