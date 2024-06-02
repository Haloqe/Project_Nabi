using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MainMenuUIController : MonoBehaviour
{
    // 0: New game, 1: Continue, 2: Settings, 3: Quit
    private TextMeshProUGUI[] _options;
    private int _selectedOptionIdx;
    private bool _hasSaveData;
    private bool _underTransition;
    
    // Colours
    private Color _selectedColour;
    private Color _unselectedColour;
    private Color _unavailableColour;
    private Coroutine _colourChangeCoroutine;
    
    private void Awake()
    {
        // Colours
        _selectedColour = new Color(0.391f, 0.696f, 0.707f, 1f);
        _unselectedColour = Color.white;
        _unavailableColour = new Color(0.330f, 0.330f, 0.330f, 1f); 
        
        // Initialise
        _options = transform.Find("Options").GetComponentsInChildren<TextMeshProUGUI>();
        for (int i = 0; i < _options.Length; i++)
        {
            _options[i].GetComponent<MainMenuButton>().Init(this, i);
            _options[i].color = _unselectedColour;
        }
        _selectedOptionIdx = 0;
    }

    private void Start()
    {
        _hasSaveData = GameManager.Instance.PlayerMetaData.isDirty;
        _options[1].color = _hasSaveData ? _unselectedColour : _unavailableColour;
        SelectOption(_hasSaveData ? 1 : 0);
    }

    private void SelectOption(int newOptionIdx)
    {
        if (_underTransition) return;
        if (newOptionIdx == 1 && !_hasSaveData) return;
        
        UnselectCurrentOption();
        _options[newOptionIdx].color = _selectedColour;
        _options[newOptionIdx].text = "> " + _options[newOptionIdx].text;
        _selectedOptionIdx = newOptionIdx;
        _colourChangeCoroutine = StartCoroutine(ColourChangeCoroutine());
    }

    private void UnselectCurrentOption()
    {
        if (_colourChangeCoroutine != null) StopCoroutine(_colourChangeCoroutine);
        _options[_selectedOptionIdx].text = _options[_selectedOptionIdx].text.Substring(2);
        _options[_selectedOptionIdx].color = _unselectedColour;
    }
    
    private IEnumerator ColourChangeCoroutine()
    {
        var added = _selectedColour + _unselectedColour;
        var mid = new Color(added.r/2, added.g/2, added.b/2, 1);
        float duration = 1;
        var targetTMP = _options[_selectedOptionIdx];
        
        // Alternate between two colours
        while (true)
        {
            for (float time = 0; time < duration * 2; time += Time.unscaledDeltaTime)
            {
                float progress = Mathf.PingPong(time, duration) / duration;
                targetTMP.color = Color.Lerp(_selectedColour, mid, progress);
                yield return null;
            }
        }
    }

    public void OnNavigate(Vector2 value)
    {
        switch (value.y)
        {
            // Left or Right
            case 0:
                return;
            // Up
            case > 0:
                {
                    if (_selectedOptionIdx == 0) SelectOption(3);
                    else if (_selectedOptionIdx == 2 && !_hasSaveData) SelectOption(0);
                    else SelectOption(_selectedOptionIdx - 1);
                    break;
                }
            // Down
            case < 0:
                {
                    if (_selectedOptionIdx == 3) SelectOption(0);
                    else if (_selectedOptionIdx == 0 && !_hasSaveData) SelectOption(2);
                    else SelectOption(_selectedOptionIdx + 1);
                    break;
                }
        }
    }
    
    public void OnSubmit()
    {
        if (_underTransition) return;
        if (_selectedOptionIdx != 3) StartTransition();
        
        switch (_selectedOptionIdx)
        {
            case 0: // New Game
                // TODO: 세이브파일 있으면 덮어쓸지 경고창 띄우기
                break;
            
            case 1: // Continue
                break;
            
            case 2: // Settings
                // TODO
                break;
            
            case 3: // Quit
                GameManager.Instance.QuitGame();
                break;
        }
    }

    public void OnPointerEnter(int optionIdx) => SelectOption(optionIdx);
    public void OnPointerClick() => OnSubmit();

    private void StartTransition()
    {
        GetComponent<Animator>().enabled = true;
        _underTransition = true;
    }

    public void OnEndTransition()
    {
        switch (_selectedOptionIdx)
        {
            case 0: // New Game
                // TODO Opening cutscene
                GameManager.Instance.StartNewGame();
                break;
            
            case 1: // Continue
                GameManager.Instance.ContinueGame();
                break;
        }
    }
}
