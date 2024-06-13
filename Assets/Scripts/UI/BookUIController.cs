using System.Collections;
using TMPro;
using UnityEngine;

public class BookUIController : UIControllerBase
{
    // Pages
    public int numPages = 2;
    private int _currPageIdx; // Page 0: Status, Page 1: Legacy
    private GameObject[] _pageObjects;
    private BookPage[] _pages;
    private GameObject[] _tabs;
    //private bool _isFlipOver;
    
    // Options
    private int curSelectedOption;
    private TextMeshProUGUI[] _optionTexts;
    private string[] _optionTextDefaults;
    private bool _isSettingsOpen;
    
    // Confirm
    private GameObject _confirmPanel;
    private bool _isConfirmPanelForMenu; 
    private TextMeshProUGUI _confirmDetailTMP;
    private TextMeshProUGUI[] _confirmOptionsTMPs; // 0: No, 1: Yes
    private string[] _confirmTexts;
    private int _selectedConfirmOption;
    private Color _confirmSelectedColour;
    
    // Animation
    private bool _isClosing;
    private bool _canNavigate;
    private Animator _animator;
    private readonly static int FlipLeft = Animator.StringToHash("FlipLeft");
    private readonly static int FlipRight = Animator.StringToHash("FlipRight");
    private readonly static int Close = Animator.StringToHash("Close");
    private readonly static int Open = Animator.StringToHash("Open");

    // Audio
    private AudioSource _audioSource;
    [SerializeField] private AudioClip pageTurnClip;
    
    // Ref
    private AudioManager _audioManager;
    private UIManager _uiManager;
    private SettingsUIController _settingsUIController;
    
    private void Awake()
    {
        _pageObjects = new GameObject[numPages];
        _pages = new BookPage[numPages];
        _tabs = new GameObject[numPages + 1];
        for (int pageIdx = 0; pageIdx < numPages; pageIdx++)
        {
            _pageObjects[pageIdx] = transform.Find("Page_" + pageIdx).gameObject;
            _pages[pageIdx] = _pageObjects[pageIdx].GetComponent<BookPage>();
            _pages[pageIdx].Init(this);
            _tabs[pageIdx] = transform.Find("Tab_" + pageIdx).gameObject;
            _tabs[pageIdx].GetComponent<BookTab>().Init(this, pageIdx);
        }
        _tabs[2] = transform.Find("Tab_2").gameObject;
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        
        // Confirm panels
        _confirmPanel = transform.parent.Find("ConfirmPanel").gameObject;
        _confirmDetailTMP = _confirmPanel.transform.Find("InfoText").GetComponent<TextMeshProUGUI>();
        _confirmOptionsTMPs = _confirmPanel.transform.Find("Options").GetComponentsInChildren<TextMeshProUGUI>();
        _confirmTexts = new string[2]
        {
            _confirmOptionsTMPs[0].text, _confirmOptionsTMPs[1].text,
        };
        _confirmSelectedColour = new Color(0.9f, 0.7f, 0, 1);
        
        // Options
        _optionTexts = transform.Find("Options").GetComponentsInChildren<TextMeshProUGUI>();
        _optionTextDefaults = new string[_optionTexts.Length];
        for (int i = 0; i < _optionTexts.Length; i++)
        {
            _optionTextDefaults[i] = _optionTexts[i].text;
        }
    }

    private void Start()
    {
        _audioManager = AudioManager.Instance;
        _uiManager = UIManager.Instance;
    }

    private void OnEnable()
    {
        UnselectSelectedOption();
        curSelectedOption = -1;
        _canNavigate = false;
        _isClosing = false;
        _currPageIdx = 0;
        foreach (var tab in _tabs)
        {
            tab.SetActive(false);
        }
        foreach (var page in _pageObjects)
        {
            page.SetActive(false);
        }
        _animator.SetTrigger(Open);
    }

    private IEnumerator DisplayPage(int pageIdx)
    {
        // TODO tab 3? 추가 안하게되면 3번째탭 지우고 BookTab array로 변경, getcomponent 없앨 것
        // Hide previous page
        _pageObjects[_currPageIdx].SetActive(false);
        _tabs[_currPageIdx].GetComponent<BookTab>()._isActiveTab = false;
        _tabs[pageIdx].GetComponent<BookTab>()._isActiveTab = true;
    
        // Flip animation?
        if (_currPageIdx != pageIdx)
        {
            _canNavigate = false;
            _animator.SetTrigger(_currPageIdx < pageIdx ? FlipLeft : FlipRight);
            _audioSource.PlayOneShot(pageTurnClip);
        
            // Wait until _isFlipOver becomes true
            yield return new WaitUntil(() => _canNavigate);
        }
        
        // Display page
        _currPageIdx = pageIdx;
        _pages[pageIdx].OnPageOpen();
        _pageObjects[pageIdx].SetActive(true);
        _canNavigate = true;
    }

    public void OnPointerClickTab(int tabIdx)
    {
        if (_confirmPanel.activeSelf) return;
        if (!_canNavigate) return;
        StartCoroutine(DisplayPage(tabIdx));
    }

    public void OnEndFlip()
    {
        _canNavigate = true;
    }

    public void OnTabOpenClose()
    {
        if (_isClosing)
        {
            _pageObjects[_currPageIdx].SetActive(false);
        }
        else
        {
            foreach (var tab in _tabs)
            {
                tab.gameObject.SetActive(true);
            }
            foreach (var page in _pages)
            {
                page.OnBookOpen();
            }
            StartCoroutine(DisplayPage(0));
        }
    }

    private void StartCloseBookAnimation()
    {
        _isClosing = true;
        _animator.SetTrigger(Close);
    }

    public void CloseBook()
    {
        _audioManager.ResetBGMVolumeToDefault();
        _uiManager.CloseFocusedUI();
    }
    
    public void OnReset()
    {
        if (_isSettingsOpen) _settingsUIController.OnReset();
    }
    
    public override void OnNavigate(Vector2 value)
    {
        if (!_canNavigate) return;
        
        // Confirm panel
        if (_confirmPanel.activeSelf)
        {
            OnNavigateConfirmPanel(value.x);
            return;
        }
        
        // Settings
        if (_isSettingsOpen)
        {
            _settingsUIController.OnNavigate(value);
            return;
        }
        _pages[_currPageIdx].OnNavigate(value);
    }

    public override void OnClose()
    {
        if (_isClosing) return;
        
        // Confirm panel
        if (_confirmPanel.activeSelf)
        {
            _confirmPanel.SetActive(false);
            return;
        }
        // Settings
        if (_isSettingsOpen)
        {
            _settingsUIController.OnClose();
            return;
        }
        _audioManager.LowerBGMVolumeUponUI();
        _canNavigate = false;
        StartCloseBookAnimation();
    }
    
    public override void OnTab()
    {
        if (!_canNavigate) return;

        if (_isSettingsOpen)
        {
            _settingsUIController.OnTab();
            return;
        }
        
        //if (!_isFlipOver) return;
        _canNavigate = false;
        int nextPage = _currPageIdx + 1 == numPages ? 0 : _currPageIdx + 1;
        UnselectSelectedOption();
        StartCoroutine(DisplayPage(nextPage));
    }
    
    public override void OnSubmit()
    {
        if (!_canNavigate) return;
        
        // Confirm Panel
        if (_confirmPanel.activeSelf)
        {
            _confirmPanel.SetActive(false);
            if (_selectedConfirmOption == 1) 
            {
                _confirmPanel.SetActive(false);
                if (_isConfirmPanelForMenu)
                {
                    PlayerEvents.Defeated.Invoke(false);
                    GameManager.Instance.LoadMainMenuDelayed();
                }
                else GameManager.Instance.QuitGame();
            }
            return;
        }
        
        // Settings
        if (_isSettingsOpen)
        {
            GameManager.Instance.isFirstRun = false;
            _settingsUIController.OnSubmit();
            return;
        }
        
        switch (curSelectedOption)
        {
            case 0: // Back
                OnClose();
                break;
            
            case 1: // Main menu
                _confirmPanel.SetActive(true);
                _isConfirmPanelForMenu = true;
                _confirmDetailTMP.text = "이번 회차의 플레이 기록이 삭제됩니다.\n메인 메뉴로 돌아갈까요?";
                break;
            
            case 2: // Settings
                _settingsUIController = UIManager.Instance.OpenSettings();
                _settingsUIController.backgroundImage.color = new Color(0, 0, 0, 1);
                _settingsUIController.parentUI = this;
                _isSettingsOpen = true;
                break;
            
            case 3: // Quit
                _confirmPanel.SetActive(true);
                _isConfirmPanelForMenu = false;
                _confirmDetailTMP.text = "이번 회차의 플레이 기록이 삭제됩니다.\n게임을 종료할까요?";
                break;
        }
    }

    public void OnSubmitOption()
    {
    }
    
    // Options
    public void UnselectSelectedOption()
    {
        if (curSelectedOption == -1) return;
        _optionTexts[curSelectedOption].text = _optionTextDefaults[curSelectedOption];
        _optionTexts[curSelectedOption].color = Color.white;
        curSelectedOption = -1;
    }

    public void NavigateOptions(float x)
    {
        int prevSelected = curSelectedOption;
        UnselectSelectedOption();
        
        // Left
        if (x < 0)
        {
            if (prevSelected == 0) SelectOption(3);
            else SelectOption(prevSelected - 1);
        }
        // Right
        else if (x > 0)
        {
            if (prevSelected == 3) SelectOption(0);
            else SelectOption(prevSelected + 1);
        }
    }

    public void SelectOption(int idx)
    {
        _optionTexts[idx].text = $"> {_optionTextDefaults[idx]} <";
        _optionTexts[idx].color = new Color(1f, 0.7f, 0f, 1);
        curSelectedOption = idx;
    }

    public override void OnSettingsClosed()
    {
        _isSettingsOpen = false;
        UIManager.Instance.UpdateCombatKeyBindText();
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
        _confirmOptionsTMPs[idx].color = _confirmSelectedColour;
        _confirmOptionsTMPs[idx].text = $"> {_confirmTexts[idx]} <";
        _selectedConfirmOption = idx;
    }
    
    private void UnselectPreviousConfirmOption()
    {
        _confirmOptionsTMPs[_selectedConfirmOption].color = Color.white;
        _confirmOptionsTMPs[_selectedConfirmOption].text = _confirmTexts[_selectedConfirmOption];
    }
}
