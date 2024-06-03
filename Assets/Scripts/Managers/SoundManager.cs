using System.Collections;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    // Audio sources
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
    
    private void Start()
    {
        GameEvents.MainMenuLoaded += OnMainMenuLoaded;
        GameEvents.GameLoadEnded += OnGameLoadEnded;
        PlayerEvents.Defeated += StopBgm;
    }

    private void AddBgmPlayer()
    {
        var BGMPlayer = new GameObject("BGMPlayer");
        _introAudioSource = BGMPlayer.AddComponent<AudioSource>();
        _loopAudioSource = BGMPlayer.AddComponent<AudioSource>();
        _introAudioSource.playOnAwake = false;
        _loopAudioSource.playOnAwake = false;
        _introAudioSource.loop = false;
        _loopAudioSource.loop = true;
    }
    
    private void OnMainMenuLoaded()
    {
        AddBgmPlayer();
        


    }
    
    private void OnGameLoadEnded()
    {
        AddBgmPlayer();
        if (GameManager.Instance.ActiveScene == ESceneType.Boss)
        {
            _introAudioSource.clip = bossBgmIntro;
            _loopAudioSource.clip = bossBgmLoop;
            _introAudioSource.volume = 0.7f;
            _loopAudioSource.volume = 0.7f;
        }
        else return; //temp
        
        // Calculate a clip’s exact duration
        double introDuration = (double)_introAudioSource.clip.samples / _introAudioSource.clip.frequency;
        
        // Queue the next clip to play when the current one ends
        _introAudioSource.Play();
        _loopAudioSource.PlayScheduled(AudioSettings.dspTime + introDuration);
    }

    public void StopBgm()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        float currentTime = 0;
        float start = _introAudioSource.volume;
        float duration = 2.7f;
        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            _introAudioSource.volume = Mathf.Lerp(start, 0, currentTime / duration);
            _loopAudioSource.volume = Mathf.Lerp(start, 0, currentTime / duration);
            yield return null;
        }
        _introAudioSource.Stop();
        _loopAudioSource.Stop();
    }
}