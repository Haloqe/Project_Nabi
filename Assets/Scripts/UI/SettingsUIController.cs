using System.Collections;
using System.Linq;
using FullscreenEditor;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
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
    private Vector2Int[] _resolutionOptions;
    
    // 1 - Control
    private Color _dupRebindColour;
    private bool _isPointingLabel;
    private bool[] _isUniqueBindings;
    private RebindActionUI[] _rebindActionUis;
    private TextMeshProUGUI[] _rebindTexts;
    private GameObject _dupWarningText;
    
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
        
        // 0 - General
        _resolutionOptions = new Vector2Int[]
        {
            new Vector2Int(1280,720), new Vector2Int(1600,900), new Vector2Int(1920,1080),
        };
        
        // 1 - Control
        _dupWarningText = settingDetails[1].transform.Find("DupWarningText").gameObject;
        _dupRebindColour = new Color(0.65f, 0f, 0f, 1f);
        _rebindActionUis = settingDetails[1].GetComponentsInChildren<RebindActionUI>();
        _isUniqueBindings = new bool[_rebindActionUis.Length];
        _rebindTexts = new TextMeshProUGUI[_rebindActionUis.Length];
        for (int i = 0; i < _rebindActionUis.Length; i++)
        {
            _isUniqueBindings[i] = true;
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
        
        foreach (var rebindUI in _rebindActionUis)
        {
            rebindUI.UpdateBindingDisplay();
        }
        SelectLanguage(LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale));
        SelectFullscreen(GameManager.Instance.IsFullScreen);
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
                        if (!GameManager.Instance.IsFullScreen) 
                            SelectResolution(_selectedResolutionIdx + (int)value.x);
                        break;
            
                    case 2: // Language
                        SelectLanguage(_selectedLanguageIdx + (int)value.x);
                        break;
                }
            }
            else if (_curSettingsGroup == 2 && !_isNavigatingSound)
            {
                StartCoroutine(NavigateSoundCoroutine(_curDetailSettingIdx, value.x));
            }
        }
    }

    private void SelectFullscreen(bool isFullscreen)
    {
        fullscreenToggle.isOn = isFullscreen;
        GameManager.Instance.SetFullscreen(isFullscreen);
        if (isFullscreen)
        {
            generalResolutionTMPs[_selectedResolutionIdx].color = _unselectedDetailTextColor;
        }
        else
        {
            SelectResolution(2);
        }
    }

    private void SelectResolution(int resolutionIndex)
    {
        // Clamp index
        if (resolutionIndex == -1) resolutionIndex = generalResolutionTMPs.Length - 1;
        else if (resolutionIndex == generalResolutionTMPs.Length) resolutionIndex = 0; 
        
        // Select resolution
        generalResolutionTMPs[_selectedResolutionIdx].color = _unselectedDetailTextColor;
        _selectedResolutionIdx = resolutionIndex;
        generalResolutionTMPs[_selectedResolutionIdx].color = _selectedDetailTextColor;
        GameManager.Instance.SetResolution(_resolutionOptions[_selectedResolutionIdx]);
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
        bool allUnique = true;
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
                allUnique = false;
            }
        }
        _dupWarningText.SetActive(!allUnique);
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
    
    public override void OnSubmit()
    {
        switch (_curSettingsGroup)
        {
            case 0:
                if (_curDetailSettingIdx == 0)
                {
                    
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
        if (!CanSaveRebinds()) return; // Todo warning
        
        // Cleanup
        if (_isNavigatingSound) StopAllCoroutines();
        _isNavigatingSound = false;
        
        // Save settings
        AudioManager.Instance.SaveAudioSettings();
        UIManager.Instance.SaveActionRebinds();
        
        // Return to parent
        if (parentUI != null) parentUI.OnSettingsClosed();
        parentUI = null;
        gameObject.SetActive(false);
    }

    public override void OnTab()
    {
        if (_curSettingsGroup == 1 && !CanSaveRebinds()) return; // Todo warning
        UnselectSettingsGroup();
        SelectSettingsGroup(_curSettingsGroup == 2 ? 0 : _curSettingsGroup + 1); 
        AudioManager.Instance.PlayUINavigateSound();
    }

    public void OnReset()
    {
        switch (_curSettingsGroup)
        {
            case 0:
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

    private bool CanSaveRebinds()
    {
        return _isUniqueBindings.All(isUnique => isUnique);
    }

    private void ResetAllRebinds()
    {
        UIManager.Instance.UIIAMap.RemoveAllBindingOverrides();
        UIManager.Instance.PlayerIAMap.RemoveAllBindingOverrides();
    }
}
