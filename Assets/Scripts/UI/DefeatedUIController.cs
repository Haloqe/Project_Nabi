using System.Collections;
using TMPro;
using UnityEngine;

public class DefeatedUIController : MonoBehaviour
{
    private TextMeshProUGUI _restartTMP;
    private TextMeshProUGUI _backToMainTMP;
    private Color _unselectedColour;
    private Color _selectedColour;
    private bool _isCurrSelectedRestartBtn;
    private string _restartText;
    private string _backToMainText;
    
    private void Awake()
    {
        _restartTMP = transform.Find("RetryButton").GetComponentInChildren<TextMeshProUGUI>();
        _backToMainTMP = transform.Find("BackToMainButton").GetComponentInChildren<TextMeshProUGUI>();
        _restartTMP.GetComponentInParent<DefeatedButton>().Init(this, true);
        _backToMainTMP.GetComponentInParent<DefeatedButton>().Init(this, false);
        _unselectedColour = new Color(0.358f, 0.358f, 0.358f, 1f);
        _selectedColour = Color.white;
        _restartText = _restartTMP.text;
        _backToMainText = _backToMainTMP.text;
        OnButtonHovered(true);
    }

    private IEnumerator ColourChangeCoroutine()
    {
        var grey = new Color(_unselectedColour.r/2, _unselectedColour.g/2, _unselectedColour.b/2, 1);
        float duration = 1;
        var targetTMP = _isCurrSelectedRestartBtn ? _restartTMP : _backToMainTMP;
        
        // Alternate between two colours
        while (true)
        {
            for (float time = 0; time < duration * 2; time += Time.unscaledDeltaTime)
            {
                float progress = Mathf.PingPong(time, duration) / duration;
                targetTMP.color = Color.Lerp(_selectedColour, grey, progress);
                yield return null;
            }
        }
    }

    public void OnNavigate(Vector2 value)
    {
        if (value.y == 0) return;
        OnButtonHovered(!_isCurrSelectedRestartBtn);
    }

    private void OnButtonHovered(bool isRestartBtn)
    {
        OnButtonUnhovered(!isRestartBtn);
        if (isRestartBtn)
        {
            _restartTMP.text = $"> {_restartText} <";
            _restartTMP.color = _selectedColour;
        }
        else
        {
            _backToMainTMP.text = $"> {_backToMainText} <";
            _backToMainTMP.color = _selectedColour;
        }
        _isCurrSelectedRestartBtn = isRestartBtn;
        StartCoroutine(nameof(ColourChangeCoroutine));
    }

    private void OnButtonUnhovered(bool isRestartBtn)
    {
        StopCoroutine(nameof(ColourChangeCoroutine));
        if (isRestartBtn)
        {
            _restartTMP.text = _restartText;
            _restartTMP.color = _unselectedColour;
        }
        else
        {
            _backToMainTMP.text = _backToMainText;
            _backToMainTMP.color = _unselectedColour;
        }
    }

    public void OnSubmit()
    {
        if (_isCurrSelectedRestartBtn) GameManager.Instance.ContinueGame();
        else GameManager.Instance.LoadMainMenu();
    }

    public void OnPointerEnter(bool isRestartBtn) => OnButtonHovered(isRestartBtn);
    public void OnPointerExit(bool isRestartBtn) => OnButtonUnhovered(isRestartBtn);
}