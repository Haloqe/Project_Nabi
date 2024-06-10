using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    public SettingsData_Sound SoundSettingsData { get; private set; }
    private SettingsData_Sound _defaultSoundSettingsData;
    
    // Audio mixer groups
    [SerializeField] private AudioMixer audioMixer;
    private AudioMixerGroup _masterMixerGroup;
    private AudioMixerGroup _musicMixerGroup;
    private AudioMixerGroup _sfxMixerGroup;
    private AudioMixerGroup _uiMixerGroup;
    
    // Audio sources
    private GameObject _bgmPlayer;
    private AudioSource _introAudioSource;
    private AudioSource _loopAudioSource;
    
    // Audio clips
    // Main menu
    [SerializeField] private AudioClip mainMenuIntro;
    [SerializeField] private AudioClip mainMenuLoop;
    
    // Meta progress map
    
    // Combat map
    [SerializeField] private AudioClip combatIntro; 
    [SerializeField] private AudioClip combatLoop; 
    
    // Boss map
    [SerializeField] private AudioClip bossBgmIntro;
    [SerializeField] private AudioClip bossBgmLoop;

    // Audio settings
    private float _initialVolume;
    private bool _isUnderFadeOut;
    
    protected override void Awake()
    {
        base.Awake();
        if (IsToBeDestroyed) return;
        
        GameEvents.MainMenuLoaded += OnMainMenuLoaded;
        GameEvents.GameLoadEnded += OnGameLoadEnded;
        PlayerEvents.Defeated += StopBgm;
        
        _masterMixerGroup = audioMixer.FindMatchingGroups("Master")[0];
        _musicMixerGroup = audioMixer.FindMatchingGroups("Master/Background Music")[0];
        _sfxMixerGroup = audioMixer.FindMatchingGroups("Master/Sound Effects")[0];
        _uiMixerGroup = audioMixer.FindMatchingGroups("Master/UI")[0];
        
        _bgmPlayer = new GameObject("BGMPlayer");
        _introAudioSource = _bgmPlayer.AddComponent<AudioSource>();
        _loopAudioSource = _bgmPlayer.AddComponent<AudioSource>();
        _introAudioSource.outputAudioMixerGroup = _musicMixerGroup;
        _loopAudioSource.outputAudioMixerGroup = _musicMixerGroup;
        _introAudioSource.playOnAwake = false;
        _loopAudioSource.playOnAwake = false;
        _introAudioSource.loop = false;
        _loopAudioSource.loop = true;
        DontDestroyOnLoad(_bgmPlayer);
    }

    private void Start()
    {
        GetSavedData();
    }

    private void GetSavedData()
    {
        // Get default settings first
        _defaultSoundSettingsData = new SettingsData_Sound();
        _defaultSoundSettingsData.isMuted = false;
        audioMixer.GetFloat("MasterVolume", out _defaultSoundSettingsData.masterVolume);
        audioMixer.GetFloat("MusicVolume", out _defaultSoundSettingsData.musicVolume);
        audioMixer.GetFloat("SFXVolume", out _defaultSoundSettingsData.sfxVolume);
        audioMixer.GetFloat("UIVolume", out _defaultSoundSettingsData.uiVolume);
        
        // Get saved settings
        SoundSettingsData = SaveSystem.LoadSoundSettingsData();
        if (SoundSettingsData == null)
        {
            // If no save data exists, use default settings
            ResetAudioSettingsToDefault();
        }
        else
        {
            // If save data exists, apply the saved settings
            if (SoundSettingsData.isMuted) Mute();
            audioMixer.SetFloat("MasterVolume", SoundSettingsData.masterVolume);
            audioMixer.SetFloat("MusicVolume", SoundSettingsData.musicVolume);
            audioMixer.SetFloat("SFXVolume", SoundSettingsData.sfxVolume);
            audioMixer.SetFloat("UIVolume", SoundSettingsData.uiVolume);
        }
    }

    public void ResetAudioSettingsToDefault()
    {
        SoundSettingsData = _defaultSoundSettingsData.Clone();
    }
    
    private void OnMainMenuLoaded()
    {
        StartBgmLoop(mainMenuIntro, mainMenuLoop, 1);
    }

    private void OnGameLoadEnded()
    {
        if (GameManager.Instance.ActiveScene == ESceneType.CombatMap || 
            GameManager.Instance.ActiveScene == ESceneType.DebugCombatMap)
        {
            StartBgmLoop(combatIntro, combatLoop, 0.6f);
        }
        else if (GameManager.Instance.ActiveScene == ESceneType.Boss)
        {
            StartBgmLoop(bossBgmIntro, bossBgmLoop, 0.65f);
        }
    }

    private void StartBgmLoop(AudioClip intro, AudioClip loop, float volume)
    {
        // Set clips and volume
        _introAudioSource.clip = intro;
        _loopAudioSource.clip = loop;
        _introAudioSource.volume = volume;
        _loopAudioSource.volume = volume;
        _initialVolume = volume;
        
        // Calculate a clip’s exact duration
        double introDuration = (double)_introAudioSource.clip.samples / _introAudioSource.clip.frequency;
        
        // Queue the next clip to play when the current one ends
        _introAudioSource.Play();
        _loopAudioSource.PlayScheduled(AudioSettings.dspTime + introDuration);
    }

    public void StopBgm()
    {
        if (_introAudioSource == null) return;
        if (!_introAudioSource.isPlaying && !_loopAudioSource.isPlaying) return;
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        _isUnderFadeOut = true;
        float start = _introAudioSource.volume;
        float currentTime = 0;
        float duration = 1f;
        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            _introAudioSource.volume = Mathf.Lerp(start, 0, currentTime / duration);
            _loopAudioSource.volume = Mathf.Lerp(start, 0, currentTime / duration);
            yield return null;
        }
        _introAudioSource.Stop();
        _loopAudioSource.Stop();
        _isUnderFadeOut = false;
    }

    public void Mute()
    {
        AudioListener.volume = 0;
    }

    public void Unmute()
    {
        AudioListener.volume = 1;
    }

    public float ConvertAudioMixerVolumeToFloat(int idx)
    {
        float originalVolume = idx switch
        {
            1 => SoundSettingsData.masterVolume,
            2 => SoundSettingsData.musicVolume,
            3 => SoundSettingsData.sfxVolume,
            4 => SoundSettingsData.uiVolume,
            _ => 0,
        };
        return Mathf.Pow(10f, originalVolume / 20);
    }
    
    public void SetAudioMixerVolumeByFloat(int idx, float volume)
    {
        string mixerGroupVolumeParam = idx switch
        {
            1 => "MasterVolume",
            2 => "MusicVolume",
            3 => "SFXVolume",
            4 => "UIVolume",
            _ => "",
        };
        audioMixer.SetFloat(mixerGroupVolumeParam, Mathf.Log10(volume) * 20);
    }

    public void SaveAudioSettings()
    {
        SoundSettingsData.isMuted = AudioListener.volume == 0;
        audioMixer.GetFloat("MasterVolume", out SoundSettingsData.masterVolume);
        audioMixer.GetFloat("MusicVolume", out SoundSettingsData.musicVolume);
        audioMixer.GetFloat("SFXVolume", out SoundSettingsData.sfxVolume);
        audioMixer.GetFloat("UIVolume", out SoundSettingsData.uiVolume);
        SaveSystem.SaveSoundSettingsData();
    }

    public void LowerBGMVolumeUponUI()
    {
        StartCoroutine(BGMFadeCoroutine(_initialVolume * 0.4f, 0.6f));
    }

    public void ResetBGMVolumeToDefault()
    {
        if (_isUnderFadeOut) return;
        StartCoroutine(BGMFadeCoroutine(_initialVolume, 1f));
    }
    
    private IEnumerator BGMFadeCoroutine(float targetVolume, float duration)
    {
        float start = _introAudioSource.volume;
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            _introAudioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            _loopAudioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
    }
}