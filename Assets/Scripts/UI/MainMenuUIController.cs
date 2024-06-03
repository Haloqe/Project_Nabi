using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MainMenuUIController : UIControllerBase
{
    // References
    private GameObject _newGameConfirmPanel;
    
    // Confirm
    private TextMeshProUGUI[] _newGameConfirmTMPs; // 0: No, 1: Yes
    private string[] _newGameConfirmTexts;
    private int _selectedConfirmOption;
    private Color _confirmSelectedColour;
    
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
    
    // SFX
    private AudioSource _audioSource;
    
    private void Awake()
    {
        // Colours
        _selectedColour = new Color(0.391f, 0.696f, 0.707f, 1f);
        _unselectedColour = Color.white;
        _unavailableColour = new Color(0.330f, 0.330f, 0.330f, 1f);
        _confirmSelectedColour = new Color(0.9f, 0.7f, 0, 1);
        
        // Initialise
        _options = transform.Find("Options").GetComponentsInChildren<TextMeshProUGUI>();
        for (int i = 0; i < _options.Length; i++)
        {
            _options[i].GetComponent<MainMenuButton>().Init(this, i);
            _options[i].color = _unselectedColour;
        }
        _selectedOptionIdx = 0;
        _audioSource = GetComponent<AudioSource>();

        // Confirm panels
        _newGameConfirmPanel = gameObject.transform.Find("NewGameConfirmPanel").gameObject;
        _newGameConfirmTMPs = _newGameConfirmPanel.transform.Find("Options").GetComponentsInChildren<TextMeshProUGUI>();
        _newGameConfirmTexts = new string[2]
        {
            _newGameConfirmTMPs[0].text, _newGameConfirmTMPs[1].text
        };
    }

    private void Start()
    {
        _hasSaveData = GameManager.Instance.PlayerMetaData.isDirty;
        _options[1].color = _hasSaveData ? _unselectedColour : _unavailableColour;
        SelectOption(_hasSaveData ? 1 : 0, false);
    }

    private void SelectOption(int newOptionIdx, bool shouldPlaySound = true)
    {
        if (_underTransition) return;
        if (newOptionIdx == 1 && !_hasSaveData) return;
        
        if (_selectedOptionIdx != newOptionIdx && shouldPlaySound) _audioSource.Play();
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

    public override void OnNavigate(Vector2 value)
    {
        if (_newGameConfirmPanel.activeSelf)
        {
            OnNavigateConfirmPanel(value.x);
            return;
        }
        
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
    
    public override void OnSubmit()
    {
        if (_underTransition) return;

        if (_newGameConfirmPanel.activeSelf)
        {
            _newGameConfirmPanel.SetActive(false);
            if (_selectedConfirmOption == 1) // Restart
            {
                StartTransition();
            }
            return;
        }
        
        switch (_selectedOptionIdx)
        {
            case 0: // New Game
                if (_hasSaveData)
                {
                    _newGameConfirmPanel.SetActive(true);
                    SelectConfirmOption(0);
                    return;
                }
                StartTransition();
                break;
            
            case 1: // Continue
                StartTransition();
                break;
            
            case 2: // Settings
                // TODO
                
                break;
            
            case 3: // Quit
                GameManager.Instance.QuitGame();
                break;
        }
    }
    
    public override void OnClose()
    {
        if (_newGameConfirmPanel.activeSelf) _newGameConfirmPanel.SetActive(false);
    }
    
    public override void OnTab()
    {
        return;
    }

    public void OnPointerEnter(int optionIdx) => SelectOption(optionIdx);
    public void OnPointerClick() => OnSubmit();

    private void StartTransition()
    {
        SoundManager.Instance.StopBgm();
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

    
    // Confirm panel
    private void OnNavigateConfirmPanel(float x)
    {
        if (x == 0) return;
        
        _audioSource.Play();
        UnselectPreviousConfirmOption();
        if (x < 0) // left
        {
            SelectConfirmOption(_selectedConfirmOption == 1 ? 0 : 1);
        }
        else // right
        {
            SelectConfirmOption(_selectedConfirmOption == 0 ? 1 : 0);
        }
    }

    private void SelectConfirmOption(int idx)
    {
        _newGameConfirmTMPs[idx].color = _confirmSelectedColour;
        _newGameConfirmTMPs[idx].text = $"> {_newGameConfirmTexts[idx]} <";
        _selectedConfirmOption = idx;
    }
    
    private void UnselectPreviousConfirmOption()
    {
        _newGameConfirmTMPs[_selectedConfirmOption].color = Color.white;
        _newGameConfirmTMPs[_selectedConfirmOption].text = _newGameConfirmTexts[_selectedConfirmOption];
    }
}
