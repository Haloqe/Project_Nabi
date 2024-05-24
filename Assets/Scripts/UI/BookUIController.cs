using System.Collections;
using UnityEngine;

public class BookUIController : MonoBehaviour
{
    private bool _isClosing;
    
    // Pages
    public int numPages = 2;
    private int _currPageIdx; // Page 0: Status, Page 1: Legacy
    private GameObject[] _pages;
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
        _pages = new GameObject[numPages];
        _tabs = new GameObject[numPages + 1];
        for (int pageIdx = 0; pageIdx < numPages; pageIdx++)
        {
            _pages[pageIdx] = transform.Find("Page_" + pageIdx).gameObject;
            _tabs[pageIdx] = transform.Find("Tab_" + pageIdx).gameObject;
            _tabs[pageIdx].GetComponent<BookTab>().Init(this, pageIdx);
        }
        _tabs[2] = transform.Find("Tab_2").gameObject;
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _isClosing = false;
        _currPageIdx = 0;
        foreach (var tab in _tabs)
        {
            tab.SetActive(false);
        }
        foreach (var page in _pages)
        {
            page.SetActive(false);
        }
        _animator.SetTrigger(Open);
    }

    private IEnumerator DisplayPage(int pageIdx)
    {
        // TODO tab 3? 추가 안하게되면 3번째탭 지우고 BookTab array로 변경할 것
        // Hide previous page
        _pages[_currPageIdx].SetActive(false);
        _tabs[_currPageIdx].GetComponent<BookTab>()._isActiveTab = false;
        _tabs[pageIdx].GetComponent<BookTab>()._isActiveTab = true;
    
        // Flip animation?
        if (_currPageIdx != pageIdx)
        {
            _isFlipOver = false;
            _animator.SetTrigger(_currPageIdx < pageIdx ? FlipLeft : FlipRight);
        
            // Wait until _isFlipOver becomes true
            yield return new WaitUntil(() => _isFlipOver);
        }
        
        // Display page
        _currPageIdx = pageIdx;
        _pages[pageIdx].SetActive(true);
        _pageTurnCoroutine = null;
    }

    public void OnPointerClickTab(int tabIdx)
    {
        if (_pageTurnCoroutine != null) return;
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
            _pages[_currPageIdx].SetActive(false);
        }
        else
        {
            foreach (var tab in _tabs)
            {
                tab.gameObject.SetActive(true);
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
}
