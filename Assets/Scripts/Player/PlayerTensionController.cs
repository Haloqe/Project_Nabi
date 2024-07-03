using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerTensionController : MonoBehaviour
{
    // UI Components
    private Slider _tensionGaugeSlider;
    private TextMeshProUGUI _tensionGaugeText;
    private Image _tensionGaugeFillImage;
    private Outline _tensionGaugeOutline;
    
    // UI Colours
    private Color _fillNormalColour;
    private Color _fillRecoveryColour;
    private Color _overloadedColour;
    
    // Features
    private float _slowedTimeScale;
    private int _incrementStep;
    private float[] _critAdditionByStates;
    private float[] _damageMultiplierByStates;
    
    private int _tension;
    private int _maxTension;
    private ETensionState _tensionState;

    private float _overloadDuration;
    private float _recoveryDuration;
    
    // SFX
    private AudioSource _audioSource;
    
    // Arbor
    private EArborType _arborType;
    private GameObject _curArborDescriptionUI;
    [SerializeField] private Image curArborIcon;
    [FormerlySerializedAs("arborIcons")] [SerializeField] [NamedArray(typeof(EArborType))] private Sprite[] arborIconSprites; 
    [SerializeField] [NamedArray(typeof(EArborType))] private GameObject[] arborDescriptionUIs;

    private bool _eventsBound = false;
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _tensionGaugeSlider = GetComponentInChildren<Slider>();
        _tensionGaugeText = GetComponentInChildren<TextMeshProUGUI>();
        _tensionGaugeFillImage = _tensionGaugeSlider.transform.Find("Fill Area").GetComponentInChildren<Image>();
        _tensionGaugeOutline = _tensionGaugeSlider.transform.GetComponentInChildren<Outline>();
        _fillNormalColour = new Color(0.886f, 0.6f, 0.06f, 1f);
        _fillRecoveryColour = new Color(0.3301887f, 0.3301887f, 0.3301887f, 1f);
        _overloadedColour = new Color(0.83f, 0, 0, 1);
        _slowedTimeScale = 0.5f;
        
        //PlayerEvents.SpawnedFirstTime += Initialise;
        PlayerEvents.StartResurrect += () => OnPlayerDefeated(true);
        PlayerEvents.Defeated += OnPlayerDefeated;
        PlayerEvents.SpawnedFirstTime += OnRestarted;
        GameEvents.Restarted += OnRestarted;
        GameEvents.CombatSceneChanged += OnCombatSceneChanged;
        _eventsBound = true;
    }
    
    private void OnDestroy()
    {
        if (!_eventsBound) return;
        PlayerEvents.SpawnedFirstTime -= OnRestarted;
        PlayerEvents.StartResurrect -= () => OnPlayerDefeated(true);
        PlayerEvents.Defeated -= OnPlayerDefeated;
        GameEvents.Restarted -= OnRestarted;
        GameEvents.CombatSceneChanged -= OnCombatSceneChanged;
    }

    private void OnCombatSceneChanged()
    {
        if (GameManager.Instance.ActiveScene == ESceneType.Boss)
            ResetTension();
    }
    
    private void OnRestarted()
    {
        _incrementStep = 1;
        _maxTension = 20; 
        _recoveryDuration = 1.0f; 
        _tensionGaugeOutline.effectColor = _fillNormalColour;
        _tensionGaugeText.color = Color.white;
        _curArborDescriptionUI = arborDescriptionUIs[(int)EArborType.Default];
        ResetTension();
        ChangeArbor(EArborType.Default);
        SetTensionState(ETensionState.Innate);
    }

    private void ResetArborUpgrades()
    {
        _overloadDuration = 4.0f;
        _slowedTimeScale = 0.5f;
        _critAdditionByStates = new float[]{0.0f, 0.05f, 0.15f, 0.0f};
        _damageMultiplierByStates = new float[]{1.0f, 1.1f, 1.2f, 1.0f};
        PlayerController.Instance.updateTensionUponHit = false;
    }
    
    private void OnPlayerDefeated(bool isRealDeath)
    {
        StopAllCoroutines();
        ResetTension();
        if (Time.timeScale != 1)
        {
            Time.timeScale = 1;
            InGameEvents.TimeRevertNormal.Invoke();
        }
        SetTensionState(ETensionState.Innate);
    }

    public void PlayArborEquipAudio()
    {
        _audioSource.Play();
    }
    
    public void ChangeArbor(EArborType newArborType)
    {
        Debug.Log("Change arbor to default");
        // Remove old arbor
        if (_arborType != EArborType.Default && newArborType != EArborType.Default)
        {
            var oldArbor = Instantiate(EnemyManager.Instance.GetArborOfType(_arborType),
                PlayerController.Instance.transform.position, Quaternion.identity);
            oldArbor.GetComponent<Arbor>().tensionController = this;
        }
        ResetArborUpgrades();
        
        // Update UI
        HideArborDescriptionUI();
        _arborType = newArborType;
        _curArborDescriptionUI = arborDescriptionUIs[(int)_arborType];
        curArborIcon.sprite = arborIconSprites[(int)_arborType];
        
        // Get new arbor
        switch (_arborType)
        {
            case EArborType.Default:
                break;
            
            case EArborType.Curiosity:
                _slowedTimeScale = 0.3f;
                break;
            
            case EArborType.Serenity:
                _critAdditionByStates = new float[]{0.0f, 0.1f, 0.2f, 0.0f};
                break;
            
            case EArborType.Regret:
                _damageMultiplierByStates = new float[]{1.0f, 1.15f, 1.25f, 1.0f};
                break;
            
            case EArborType.Paranoia:
                PlayerController.Instance.updateTensionUponHit = true;
                break;
        }
    }

    private void ResetTension() => SetTensionValue(0);
    public void IncrementTension()
    {
        // Cannot change tension value during overloaded/recovery state
        if (_tensionState is ETensionState.Overloaded or ETensionState.Recovery) return;
        if (GameManager.Instance.isRunningTutorial) return;
        
        // Otherwise, increment tension
        SetTensionValue(_tension + _incrementStep);   
    }

    private void SetTensionValue(int value)
    {
        // Update UI
        _tension = value;
        _tensionGaugeSlider.value = (float)value / _maxTension;
        _tensionGaugeText.text = $"{value}/{_maxTension}";
        
        // Update tension state if needed
        // Overheated state
        if (_tension == _maxTension / 2)
        {
            SetTensionState(ETensionState.Overheated);
        }
        // Overloaded state
        else if (_tension == _maxTension)
        {
            SetTensionState(ETensionState.Overloaded);
        }
    }

    private void SetTensionState(ETensionState newState)
    {
        // Remove previous state effects
        PlayerController.Instance.AddCriticalRate(-_critAdditionByStates[(int)_tensionState]);

        // Apply new state effects
        _tensionState = newState;
        PlayerController.Instance.playerDamageDealer.totalDamageMultiplier = _damageMultiplierByStates[(int)newState];
        PlayerController.Instance.AddCriticalRate(_critAdditionByStates[(int)newState]);

        switch (newState)
        {
            case ETensionState.Innate:
                _tensionGaugeFillImage.color = _fillNormalColour;
                break;
            
            case ETensionState.Overheated:
                PlayerController.Instance.overheatedVFX.SetActive(true);
                break;
            
            case ETensionState.Overloaded:
                // Update UI and VFX
                PlayerController.Instance.overloadedVFX.SetActive(true);
                _tensionGaugeText.text = "OVERLOADED"; 
                _tensionGaugeText.color = _overloadedColour;
                _tensionGaugeOutline.effectColor = _overloadedColour;
                
                // Start slowdown
                Time.timeScale = _slowedTimeScale;
                InGameEvents.TimeSlowDown.Invoke();
                
                // Wait for overload duration
                StartCoroutine(OverloadDelayCoroutine());
                break;
            
            case ETensionState.Recovery:
                // Update UI and VFX
                _tensionGaugeText.text = "UNDER RECOVERY";   
                _tensionGaugeFillImage.color = _fillRecoveryColour;
                _tensionGaugeText.color = Color.white;
                _tensionGaugeOutline.effectColor = _fillNormalColour;
                
                // End slowdown
                Time.timeScale = 1;
                InGameEvents.TimeRevertNormal.Invoke();
                
                // Wait for recovery duration
                StartCoroutine(RecoveryDelayCoroutine());
                break;
        }
    }

    private IEnumerator OverloadDelayCoroutine()
    {
        // Wait for the overload to end
        yield return new WaitForSecondsRealtime(_overloadDuration);
        
        // End overload
        SetTensionState(ETensionState.Recovery);
    }

    private IEnumerator RecoveryDelayCoroutine()
    {
        // Wait for the recovery to end
        yield return new WaitForSecondsRealtime(_recoveryDuration);
        
        // End recovery
        ResetTension();
        SetTensionState(ETensionState.Innate);
    }

    public void DisplayArborDescriptionUI()
    {
        _curArborDescriptionUI.SetActive(true);
    }
    
    public void HideArborDescriptionUI()
    {
        _curArborDescriptionUI.SetActive(false);
    }
}
