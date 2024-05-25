using TMPro;
using UnityEngine;

public class MetaUIController : MonoBehaviour
{
    private PlayerMetaInfo MetaInfo;
    public PlayerInventory PlayerInventory { get; private set; } 
    public Sprite selectedLevelSlotSprite;
    public Sprite unselectedLevelSlotSprite;
    private int _selectedPanelIdx;
    private MetaPanelUI[] _metaPanels;
    private Animator _animator;

    [SerializeField] private TextMeshProUGUI warningObjTMP;
    private readonly static int Display = Animator.StringToHash("Display");

    private void Awake()
    {
        MetaInfo = GameManager.Instance.PlayerMetaInfo;
        PlayerInventory = PlayerController.Instance.playerInventory;
        _animator = GetComponentInChildren<Animator>();
        _metaPanels = GetComponentsInChildren<MetaPanelUI>();
        for (int i = 0; i < 5; i++)
        {
            _metaPanels[i].MetaInfo = MetaInfo;
            _metaPanels[i].metaIndex = i;
        }
    }

    private void OnEnable()
    {
        _selectedPanelIdx = -1;
        SelectPanel(0);
    }

    private void OnDisable()
    {
        _metaPanels[_selectedPanelIdx].OnUnselectMetaPanel();
    }

    public void OnNavigate(Vector2 value)
    {
        _metaPanels[_selectedPanelIdx].OnNavigate(value);
    }

    public void OnTab()
    {
        SelectPanel(_selectedPanelIdx == 4 ? 0 : _selectedPanelIdx + 1);
    }

    public void OnSubmit()
    {
        _metaPanels[_selectedPanelIdx].OnSubmit();
    }

    private void SelectPanel(int panelIdx)
    {
        // Unselect previous panel
        if (_selectedPanelIdx != -1)
        {
            _metaPanels[_selectedPanelIdx].OnUnselectMetaPanel();
        }
        
        // Select new panel
        _selectedPanelIdx = panelIdx;
        _metaPanels[_selectedPanelIdx].OnSelectMetaPanel();
    }

    // Warning popups
    public void DisplayAlreadyUnlockedWarning()
    {
        _animator.SetTrigger(Display);
        warningObjTMP.text = "이미 보유한 업그레이드예요";
    }
    
    public void DisplayUnreachableLevelWarning()
    {
        _animator.SetTrigger(Display);
        warningObjTMP.text = "더 낮은 등급의 업그레이드를 먼저 해주세요";
    }
    
    public void DisplayNotEnoughShardsWarning()
    {
        _animator.SetTrigger(Display);
        warningObjTMP.text = "영혼 조각이 부족해요";
    }
}