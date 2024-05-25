using System.Collections;
using UnityEngine;

public class BookUIController : MonoBehaviour
{
    private bool _isClosing;
    
    // Pages
    public int numPages = 2;
    private int _currPageIdx; // Page 0: Status, Page 1: Legacy
    private GameObject[] _pageObjects;
    private BookPage[] _pages;
    private GameObject[] _tabs;
    private bool _isFlipOver;
    
    // Animation
    private Animator _animator;
    private Coroutine _pageTurnCoroutine;
    private readonly static int FlipLeft = Animator.StringToHash("FlipLeft");
    private readonly static int FlipRight = Animator.StringToHash("FlipRight");
    private readonly static int Close = Animator.StringToHash("Close");
    private readonly static int Open = Animator.StringToHash("Open");

    private void Awake()
    {
        _pageObjects = new GameObject[numPages];
        _pages = new BookPage[numPages];
        _tabs = new GameObject[numPages + 1];
        for (int pageIdx = 0; pageIdx < numPages; pageIdx++)
        {
            _pageObjects[pageIdx] = transform.Find("Page_" + pageIdx).gameObject;
            _pages[pageIdx] = _pageObjects[pageIdx].GetComponent<BookPage>();
            _pages[pageIdx].Init();
            _tabs[pageIdx] = transform.Find("Tab_" + pageIdx).gameObject;
            _tabs[pageIdx].GetComponent<BookTab>().Init(this, pageIdx);
        }
        _tabs[2] = transform.Find("Tab_2").gameObject;
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _isFlipOver = false;
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
        // TODO tab 3? 추가 안하게되면 3번째탭 지우고 BookTab array로 변경할 것
        // Hide previous page
        _pageObjects[_currPageIdx].SetActive(false);
        _tabs[_currPageIdx].GetComponent<BookTab>()._isActiveTab = false;
        _tabs[pageIdx].GetComponent<BookTab>()._isActiveTab = true;
    
        // Flip animation?
        if (_currPageIdx != pageIdx)
        {
            _animator.SetTrigger(_currPageIdx < pageIdx ? FlipLeft : FlipRight);
        
            // Wait until _isFlipOver becomes true
            yield return new WaitUntil(() => _isFlipOver);
        }
        
        // Display page
        _isFlipOver = false;
        _currPageIdx = pageIdx;
        _pages[pageIdx].OnPageOpen();
        _pageObjects[pageIdx].SetActive(true);
        _pageTurnCoroutine = null;
        _isFlipOver = true;
    }

    public void OnPointerClickTab(int tabIdx)
    {
        if (_pageTurnCoroutine != null) return;
        if (!_isFlipOver) return;
        _pageTurnCoroutine = StartCoroutine(DisplayPage(tabIdx));
    }

    public void OnEndFlip()
    {
        _isFlipOver = true;
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

    public void StartCloseBookAnimation()
    {
        if (_isClosing) return;
        _isClosing = true;
        _animator.SetTrigger(Close);
    }

    public void CloseBook()
    {
        UIManager.Instance.CloseFocusedUI();
    }
    
    public void OnNavigate(Vector2 value)
    {
        _pages[_currPageIdx].OnNavigate(value);
    }

    public void OnTab()
    {
        if (!_isFlipOver) return;
        _isFlipOver = false;
        int nextPage = _currPageIdx + 1 == numPages ? 0 : _currPageIdx + 1;
        StartCoroutine(DisplayPage(nextPage));
    }
}
