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
        }
    }

    private void Start()
    {
        _hasSaveData = GameManager.Instance.HasSaveData;
        _options[1].color = _hasSaveData? _unselectedColour : _unavailableColour;
        SelectOption(0);
    }

    private void SelectOption(int selectedIdx)
    {
        if (_underTransition) return;
        if (selectedIdx == 1 && !_hasSaveData) return;
        UnselectCurrentOption();
        _selectedOptionIdx = selectedIdx;
        _options[_selectedOptionIdx].color = _selectedColour;
        _options[_selectedOptionIdx].text = "> " + _options[_selectedOptionIdx].text;
        StartCoroutine(nameof(ColourChangeCoroutine));
    }

    private void UnselectCurrentOption()
    {
        StopCoroutine(nameof(ColourChangeCoroutine));
        _options[_selectedOptionIdx].color = _unselectedColour;
        _options[_selectedOptionIdx].text = _options[_selectedOptionIdx].text.Substring(2);
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
                // TODO
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
        GetComponent<Animator>().SetTrigger("Transition");
        _underTransition = true;
    }

    public void OnEndTransition()
    {
        switch (_selectedOptionIdx)
        {
            case 0: // New Game
                // TODO Opening animation
                GameManager.Instance.LoadInGame();
                break;
            
            case 1: // Continue
                // TODO
                break;
            
            case 2: // Settings
                // TODO
                break;
        }
    }
}
