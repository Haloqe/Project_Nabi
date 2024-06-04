using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUIController : UIControllerBase
{
    private AudioManager _audioManager;
    
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
        
        // TODO
        // get data from savefile
        LoadSoundSaveData();
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
        if (_curSettingsGroup != 2) return;
        
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
        if (_curSettingsGroup != 2) return;
        
        switch (_curSettingsGroup)
        {
            case 0:
                break;
            
            case 1:
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
        if (_isNavigatingSound) StopAllCoroutines();
        _isNavigatingSound = false;
        
        // Todo save settings
        _audioManager.SaveAudioSettings();
        
        if (parentMainMenu != null)
        {
            parentMainMenu.OnSettingsClosed();
            parentMainMenu = null;
        }
        gameObject.SetActive(false);
    }
    
    public override void OnTab()
    {
        UnselectSettingsGroup();
        SelectSettingsGroup(_curSettingsGroup == 2 ? 0 : _curSettingsGroup + 1); 
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
        SelectSettingDetail(0);
    }

    private void UnselectSettingDetail()
    {
        if (_curSettingsGroup != 2) return;
        _settingDetailImages[_curSettingsGroup][_curDetailSettingIdx].enabled = false;
    }

    private void SelectSettingDetail(int idx)
    {
        if (_curSettingsGroup != 2) return;

        _curDetailSettingIdx = idx;
        _settingDetailImages[_curSettingsGroup][_curDetailSettingIdx].enabled = true;
    }
}