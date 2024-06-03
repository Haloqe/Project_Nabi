using System.Collections;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
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
    
    // Boss map
    [SerializeField] private AudioClip bossBgmIntro;
    [SerializeField] private AudioClip bossBgmLoop;

    protected override void Awake()
    {
        base.Awake();
        if (IsToBeDestroyed) return;
        
        GameEvents.MainMenuLoaded += OnMainMenuLoaded;
        GameEvents.GameLoadEnded += OnGameLoadEnded;
        PlayerEvents.Defeated += StopBgm;
        
        _bgmPlayer = new GameObject("BGMPlayer");
        _introAudioSource = _bgmPlayer.AddComponent<AudioSource>();
        _loopAudioSource = _bgmPlayer.AddComponent<AudioSource>();
        _introAudioSource.playOnAwake = false;
        _loopAudioSource.playOnAwake = false;
        _introAudioSource.loop = false;
        _loopAudioSource.loop = true;
        DontDestroyOnLoad(_bgmPlayer);
    }
    
    private void OnMainMenuLoaded()
    {
        _introAudioSource.clip = mainMenuIntro;
        _loopAudioSource.clip = mainMenuLoop;
        _introAudioSource.volume = 1f;
        _loopAudioSource.volume = 1f;
        StartBgmLoop();
    }

    private void OnGameLoadEnded()
    {
        if (GameManager.Instance.ActiveScene == ESceneType.Boss)
        {
            _introAudioSource.clip = bossBgmIntro;
            _loopAudioSource.clip = bossBgmLoop;
            _introAudioSource.volume = 0.6f;
            _loopAudioSource.volume = 0.6f;
            StartBgmLoop();
        }
    }

    private void StartBgmLoop()
    {
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
        float start = _introAudioSource.volume;
        float currentTime = 0;
        float duration = 1f;
        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            Debug.Log(currentTime);
            _introAudioSource.volume = Mathf.Lerp(start, 0, currentTime / duration);
            _loopAudioSource.volume = Mathf.Lerp(start, 0, currentTime / duration);
            yield return null;
        }
        _introAudioSource.Stop();
        _loopAudioSource.Stop();
    }
}