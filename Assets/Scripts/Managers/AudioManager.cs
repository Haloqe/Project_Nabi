using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class AudioManager : Singleton<AudioManager>
{
    public SettingsData_Sound SoundSettingsData { get; private set; }
    private SettingsData_Sound _defaultSoundSettingsData;
    
    // Audio mixer groups
    [SerializeField] private AudioMixer audioMixer;
    private AudioMixerGroup[] _audioMixerGroups;
    
    // Audio sources
    private GameObject _bgmPlayer;
    private AudioSource _introAudioSource;
    private AudioSource _loopAudioSource;
    private AudioSource _uiAudioSource;
    
    // Audio clips
    // UI
    [Space(15)][Header("User Interface")]
    [SerializeField] private AudioClip uiNavigateSound;
    [SerializeField] private AudioClip uiConfirmSound;
    
    // Main menu
    [Space(15)][Header("Main Menu")]
    [SerializeField] private AudioClip mainMenuIntro;
    [SerializeField] private AudioClip mainMenuLoop;
    
    // Meta progress map
    [Space(15)][Header("InGame")]
    [SerializeField] private AudioClip metaIntro;
    [SerializeField] private AudioClip metaLoop;
    
    // Combat map
    [SerializeField] private AudioClip combatIntro; 
    [SerializeField] private AudioClip combatLoop; 
    [SerializeField] private AudioClip combatIntro2; 
    [SerializeField] private AudioClip combatLoop2; 
    
    // Boss map
    [Space(15)][Header("Boss")]
    [SerializeField] private AudioClip midBossIntro;
    [SerializeField] private AudioClip midBossLoop;
    [SerializeField] private AudioClip bossIntro;
    [SerializeField] private AudioClip bossLoop;

    // Audio settings
    private float _initialVolume;
    private bool _isUnderFadeOut;
    
    protected override void Awake()
    {
        base.Awake();
        if (IsToBeDestroyed) return;
        
        GameEvents.MainMenuLoaded += OnMainMenuLoaded;
        GameEvents.GameLoadEnded += OnGameLoadEnded;
        PlayerEvents.Defeated += OnPlayerDefeated;
        
        _audioMixerGroups = new AudioMixerGroup[]
        {
            audioMixer.FindMatchingGroups("Master")[0],
            audioMixer.FindMatchingGroups("Master/Background Music")[0],
            audioMixer.FindMatchingGroups("Master/Enemy")[0],
            audioMixer.FindMatchingGroups("Master/Others")[0],
            audioMixer.FindMatchingGroups("Master/UI")[0],
        };
        
        _bgmPlayer = new GameObject("AudioPlayer");
        _introAudioSource = _bgmPlayer.AddComponent<AudioSource>();
        _loopAudioSource = _bgmPlayer.AddComponent<AudioSource>();
        _uiAudioSource = _bgmPlayer.AddComponent<AudioSource>();
        _introAudioSource.outputAudioMixerGroup = _audioMixerGroups[(int)EAudioType.BGM];
        _loopAudioSource.outputAudioMixerGroup = _audioMixerGroups[(int)EAudioType.BGM];
        _uiAudioSource.outputAudioMixerGroup = _audioMixerGroups[(int)EAudioType.UI];
        _introAudioSource.playOnAwake = false;
        _loopAudioSource.playOnAwake = false;
        _introAudioSource.loop = false;
        _loopAudioSource.loop = true;
        DontDestroyOnLoad(_bgmPlayer);
    }

    private void OnPlayerDefeated(bool isRealDeath)
    {
        StopBgm(1f);    
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
        audioMixer.GetFloat("EnemyVolume", out _defaultSoundSettingsData.enemyVolume);
        audioMixer.GetFloat("OthersVolume", out _defaultSoundSettingsData.othersVolume);
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
            audioMixer.SetFloat("EnemyVolume", SoundSettingsData.enemyVolume);
            audioMixer.SetFloat("OthersVolume", SoundSettingsData.othersVolume);
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
        switch (GameManager.Instance.ActiveScene)
        {
            case ESceneType.DebugCombatMap:
                StartBgmLoop(combatIntro, combatLoop, 0.5f);
                break;
            
            case ESceneType.CombatMap0:
                StartBgmLoop(null, metaLoop, 0.2f);
                break;
            
            case ESceneType.CombatMap1:
                StartBgmLoop(combatIntro2, combatLoop2, 0.5f);
                break;
            
            case ESceneType.MidBoss:
                StartBgmLoop(midBossIntro, midBossLoop, 0.6f);
                break;
            
            case ESceneType.Boss:
                StartCoroutine(DelayedPlayCoroutine(bossIntro, bossLoop, 0.65f, 1.3f));
                break;
        }
    }

    private IEnumerator DelayedPlayCoroutine(AudioClip intro, AudioClip loop, float volume, float delayDuration)
    {
        yield return new WaitForSecondsRealtime(delayDuration);
        StartBgmLoop(intro, loop, volume);
    }

    private void StartBgmLoop(AudioClip intro, AudioClip loop, float volume)
    {
        if (_isUnderFadeOut)
        {
            StopAllCoroutines();
            _isUnderFadeOut = false;
        }
        
        // Set clips and volume
        _introAudioSource.clip = intro;
        _loopAudioSource.clip = loop;
        _introAudioSource.volume = 0.1f;
        _loopAudioSource.volume = 0.1f;
        _initialVolume = volume;
        
        // Direct loop?
        if (intro == null)
        {
            _loopAudioSource.Play();
        }
        // Intro -> Loop?
        else
        {
            // Calculate a clip’s exact duration
            double introDuration = (double)_introAudioSource.clip.samples / _introAudioSource.clip.frequency;
        
            // Queue the next clip to play when the current one ends
            _introAudioSource.Play();
            _loopAudioSource.PlayScheduled(AudioSettings.dspTime + introDuration); 
        }
        StartCoroutine(BGMFadeCoroutine(volume, 0.6f));
    }

    public void StopBgm(float fadeOutDuration)
    {
        if (_loopAudioSource == null) return;
        StartCoroutine(FadeOutCoroutine(fadeOutDuration));
    }

    private IEnumerator FadeOutCoroutine(float fadeOutDuration)
    {
        _isUnderFadeOut = true;
        float start = _loopAudioSource.volume;
        float currentTime = 0;
        float duration = fadeOutDuration;
        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            _introAudioSource.volume = Mathf.Lerp(start, 0.1f, currentTime / duration);
            _loopAudioSource.volume = Mathf.Lerp(start, 0.1f, currentTime / duration);
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
            0 => SoundSettingsData.masterVolume,
            1 => SoundSettingsData.musicVolume,
            2 => SoundSettingsData.enemyVolume,
            3 => SoundSettingsData.othersVolume,
            4 => SoundSettingsData.uiVolume,
            _ => 0,
        };
        return Mathf.Pow(10f, originalVolume / 20);
    }
    
    public void SetAudioMixerVolumeByFloat(int idx, float volume)
    {
        string mixerGroupVolumeParam = idx switch
        {
            0 => "MasterVolume",
            1 => "MusicVolume",
            2 => "EnemyVolume",
            3 => "OthersVolume",
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
        audioMixer.GetFloat("EnemyVolume", out SoundSettingsData.enemyVolume);
        audioMixer.GetFloat("OthersVolume", out SoundSettingsData.othersVolume);
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

    public void StartCombatBgm()
    {
        if (GameManager.Instance.ActiveScene == ESceneType.CombatMap0)
        {
            StartBgmLoop(combatIntro, combatLoop, 0.5f);
        }
        else
        {
            StartBgmLoop(combatIntro2, combatLoop2, 0.5f);
        }
    }

    private AudioMixerGroup GetAudioMixerGroup(EAudioType audioType)
    {
        return _audioMixerGroups[(int)audioType];
    }
    
    public void PlayOneShotClip(AudioClip clip, float volume, EAudioType audioType)
    {
        GameObject obj = new GameObject("OneShotAudio");
        AudioSource audioSource = obj.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = GetAudioMixerGroup(audioType);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(obj, clip.length + 0.1f);
    }

    public void PlayUINavigateSound()
    {
        _uiAudioSource.PlayOneShot(uiNavigateSound);
    }
    
    public void PlayUIConfirmSound()
    {
        _uiAudioSource.PlayOneShot(uiConfirmSound);
    }
}