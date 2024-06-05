using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsUIController : UIControllerBase
{
    // Ref
    private AudioManager _audioManager;
    private UIManager _uiManager;
    
    // Parent
    public MainMenuUIController parentMainMenu;
    
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
    
    // 1 - Control
    private Color _dupRebindColour;
    private bool _isPointingLabel;
    private bool[] _isUniqueBindings;
    private RebindActionUI[] _rebindActionUis;
    private TextMeshProUGUI[] _rebindTexts;
    private GameObject _dupWarningText;
    //private AutoScrollRect _autoScrollRect;
    
    // 2 - Sound
    private Toggle _muteToggle;
    private Slider[] _soundSliders;
    
    // Colours
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
        
        // Get references from details
        _settingDetailImages = new Image[][]
        {
            settingImagesGeneral, settingImagesControl, settingImagesSound,
        };
        
        // 1 - Control
        //_autoScrollRect = settingDetails[1].GetComponentInChildren<AutoScrollRect>();
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
        _audioManager = AudioManager.Instance;
        _uiManager = UIManager.Instance;
        
        // TODO
        // get data from savefile
        LoadSoundSaveData();
        
        foreach (var rebindUI in _rebindActionUis)
        {
            rebindUI.UpdateBindingDisplay();
        }
    }

    private void LoadSoundSaveData()
    {
        _muteToggle.isOn = _audioManager.SoundSettingsData.isMuted;
        _soundSliders[1].value = _audioManager.ConvertAudioMixerVolumeToFloat(1);
        _soundSliders[2].value = _audioManager.ConvertAudioMixerVolumeToFloat(2);
        _soundSliders[3].value = _audioManager.ConvertAudioMixerVolumeToFloat(3);
        _soundSliders[4].value = _audioManager.ConvertAudioMixerVolumeToFloat(4);
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
            if (_curSettingsGroup == 2 && !_isNavigatingSound)
            {
                StartCoroutine(NavigateSoundCoroutine(_curDetailSettingIdx, value.x));
            }
        }
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
            targetRelatedAction = _uiManager.PlayerIAMap.FindAction("Interact_Hold");
        }
        else if (_curDetailSettingIdx == 9) // Map
        {
            targetRelatedAction = _uiManager.UIIAMap.FindAction("CloseMap");
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
        _audioManager.SetAudioMixerVolumeByFloat(idx, _soundSliders[idx].value);
    }

    public void OnMuteToggleChanged()
    {
        if (_muteToggle.isOn) _audioManager.Mute();
        else _audioManager.Unmute();
    }
    
    public override void OnSubmit()
    {
        switch (_curSettingsGroup)
        {
            case 0:
                break;
            
            case 1:
                if (_curSettingsGroup == 1 && _isPointingLabel)
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
        _uiManager.SaveActionRebinds();
        _audioManager.SaveAudioSettings();
        
        // Return to parent
        if (parentMainMenu != null)
        {
            parentMainMenu.OnSettingsClosed();
            parentMainMenu = null;
        }
        gameObject.SetActive(false);
    }

    public override void OnTab()
    {
        if (_curSettingsGroup == 1 && !CanSaveRebinds()) return; // Todo warning
        UnselectSettingsGroup();
        SelectSettingsGroup(_curSettingsGroup == 2 ? 0 : _curSettingsGroup + 1); 
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
                _audioManager.ResetAudioSettingsToDefault();
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
        if (_curSettingsGroup == 0) return;
        _settingDetailImages[_curSettingsGroup][_curDetailSettingIdx].enabled = false;
    }

    private bool _curFocusedBottom = false;
    private void SelectSettingDetail(int idx)
    {
        if (_curSettingsGroup == 0) return;

        _curDetailSettingIdx = idx;
        _settingDetailImages[_curSettingsGroup][_curDetailSettingIdx].enabled = true;

        if (_curSettingsGroup == 1)
        {
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
            // _autoScrollRect.SetSelectedGameObject(settingDetails[1].transform
            //     .Find("ScrollRect").Find("Container").Find("Interact").gameObject);
        }
    }

    private bool CanSaveRebinds()
    {
        return _isUniqueBindings.All(isUnique => isUnique);
    }

    private void ResetAllRebinds()
    {
        _uiManager.UIIAMap.RemoveAllBindingOverrides();
        _uiManager.PlayerIAMap.RemoveAllBindingOverrides();
    }
}
