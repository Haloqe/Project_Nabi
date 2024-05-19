using System.Collections;
using TMPro;
using UnityEngine;
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
    
    // References
    private PlayerController _player;
    private GameObject _overloadedVFX;
    private GameObject _overheatedVFX;
    
    // Features
    private float _slowDownAmount;
    private int _incrementStep;
    private float[] _critAdditionByStates;
    private float[] _damageMutiplierByStates;
    
    private int _tension;
    private int _maxTension;
    private EArborType _arborType;
    private ETensionState _tensionState;

    private float _overloadDuration;
    private float _recoveryDuration;
    
    
    private void Awake()
    {
        GameEvents.Restarted += OnRestarted;
        _tensionGaugeSlider = GetComponentInChildren<Slider>();
        _tensionGaugeText = GetComponentInChildren<TextMeshProUGUI>();
        _tensionGaugeFillImage = _tensionGaugeSlider.transform.Find("Fill Area").GetComponentInChildren<Image>();
        _tensionGaugeOutline = _tensionGaugeSlider.transform.GetComponentInChildren<Outline>();
        _fillNormalColour = new Color(0.886f, 0.6f, 0.06f, 1f);
        _fillRecoveryColour = new Color(0.3301887f, 0.3301887f, 0.3301887f, 1f);
        _overloadedColour = new Color(0.83f, 0, 0, 1);
        _slowDownAmount = 0.4f;
    }

    private void Start()
    {
        _player = PlayerController.Instance;
        _overheatedVFX = _player.transform.Find("OverheatedVFX").gameObject;
        _overloadedVFX = _player.transform.Find("OverloadedVFX").gameObject;
        OnRestarted();
    }

    private void OnRestarted()
    {
        _incrementStep = 1;
        _maxTension = 4;//50;
        _arborType = EArborType.Default;
        _overloadDuration = 4.0f;
        _recoveryDuration = 3.0f;
        _critAdditionByStates = new float[]{0.0f, 0.05f, 0.15f, 0.0f};
        _damageMutiplierByStates = new float[]{1.0f, 1.1f, 1.2f, 1.0f};
        _tensionGaugeOutline.effectColor = _fillNormalColour;
        _tensionGaugeText.color = Color.white;
        ResetTension();
        SetTensionState(ETensionState.Innate);
    }

    public void ChangeArbor(EArborType newArborType)
    {
        // Remove old arbor
        // TODO: 축 바닥에 버리고, 영구히 사라지는 효과
        
        // TODO: Get new arbor
        switch (_arborType)
        {
            
        }
    }

    private void ResetTension() => SetTensionValue(0);
    public void IncrementTension()
    {
        // Cannot change tension value during overloaded/recovery state
        if (_tensionState is ETensionState.Overloaded or ETensionState.Recovery) return;
        
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
        _player.AddCriticalRate(-_critAdditionByStates[(int)_tensionState]);

        // Apply new state effects
        _tensionState = newState;
        _player.playerDamageDealer.totalDamageMultiplier = _damageMutiplierByStates[(int)newState];
        _player.AddCriticalRate(_critAdditionByStates[(int)newState]);

        switch (newState)
        {
            case ETensionState.Innate:
                _tensionGaugeFillImage.color = _fillNormalColour;
                break;
            
            case ETensionState.Overheated:
                _overheatedVFX.SetActive(true);
                break;
            
            case ETensionState.Overloaded:
                // Update UI and VFX
                _overloadedVFX.SetActive(true);
                _tensionGaugeText.text = "OVERLOADED"; 
                _tensionGaugeText.color = _overloadedColour;
                _tensionGaugeOutline.effectColor = _overloadedColour;
                
                // Start slowdown
                Time.timeScale = _slowDownAmount;
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
}
