using TMPro;
using UnityEngine;

public class MetaUIController : UIControllerBase
{
    private PlayerMetaData _metaData;
    private PlayerController _playerController;
    public PlayerInventory PlayerInventory { get; private set; } 
    public Sprite selectedLevelSlotSprite;
    public Sprite unselectedLevelSlotSprite;
    private int _selectedPanelIdx;
    private MetaPanelUI[] _metaPanels;
    private Animator _animator;
    private bool _readyToNavigate;

    [SerializeField] private TextMeshProUGUI warningObjTMP;
    private readonly static int Display = Animator.StringToHash("Display");

    private void Awake()
    {
        _readyToNavigate = false;
        _metaData = GameManager.Instance.PlayerMetaData;
        _playerController = PlayerController.Instance;
        PlayerInventory = _playerController.playerInventory;
        _animator = GetComponentInChildren<Animator>();
        _metaPanels = GetComponentsInChildren<MetaPanelUI>();
        for (int i = 0; i < 5; i++)
        {
            _metaPanels[i].MetaData = _metaData;
            _metaPanels[i].metaIndex = i;
        }
    }

    private void OnEnable()
    {
        _animator.Rebind();
        _animator.Update(0f);
        _selectedPanelIdx = -1;
        SelectPanel(0);
        _readyToNavigate = true;
    }
    
    private void Start()
    {
        _selectedPanelIdx = -1;
        SelectPanel(0);
        _readyToNavigate = true;
    }

    private void OnDisable()
    {
        _metaPanels[_selectedPanelIdx].OnUnselectMetaPanel();
    }

    public override void OnNavigate(Vector2 value)
    {
        if (!_readyToNavigate) return;
        _metaPanels[_selectedPanelIdx].OnNavigate(value);
    }

    public override void OnClose()
    {
        return;
    }
    
    public override void OnTab()
    {
        if (!_readyToNavigate) return;
        SelectPanel(_selectedPanelIdx == 4 ? 0 : _selectedPanelIdx + 1);
    }

    public override void OnSubmit()
    {
        if (!_readyToNavigate) return;
        _metaPanels[_selectedPanelIdx].OnSubmit();
    }

    private void SelectPanel(int panelIdx)
    {
        if (panelIdx == _selectedPanelIdx) return;
        
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
        warningObjTMP.text = Define.Localisation == ELocalisation.ENG ? "???" : "이미 보유한 업그레이드예요";
    }
    
    public void DisplayUnreachableLevelWarning()
    {
        _animator.SetTrigger(Display);
        warningObjTMP.text = Define.Localisation == ELocalisation.ENG ? "???" : "더 낮은 등급의 업그레이드를 먼저 해주세요";
    }
    
    public void DisplayNotEnoughShardsWarning()
    {
        _animator.SetTrigger(Display);
        warningObjTMP.text = Define.Localisation == ELocalisation.ENG ? "???" : "영혼 조각이 부족해요";
    }

    public void ApplyMetaUpgrade(int metaIndex, int unlockedLevel)
    {
        switch ((EMetaUpgrade)metaIndex)
        {
            case EMetaUpgrade.BetterLegacyPreserv: // 희귀한 유산을 얻을 확률이 증가한다
                // Handled separately - no need for a direct update
                break;
            
            case EMetaUpgrade.Resurrection: // 사망시 더 낮은 체력으로 1회 부활한다
                if (unlockedLevel == 0) _playerController.playerDamageReceiver.canResurrect = true;
                break;
            
            case EMetaUpgrade.HealthAddition: // 최대 체력이 증가한다
                _playerController.playerDamageReceiver.additionalHealth += Define.MetaHealthAdditions[unlockedLevel];
                _playerController.playerDamageReceiver.ChangeHealthByAmount(0);
                break;
            
            case EMetaUpgrade.CritRateAddition: // 크리티컬 확률이 증가한다
                _playerController.AddCriticalRate(Define.MetaCriticalRateAdditions[unlockedLevel]);
                break;
            
            case EMetaUpgrade.StrengthAddition: // 공격력이 증가한다
                _playerController.strengthAddition += Define.MetaAttackDamageAdditions[unlockedLevel];
                PlayerEvents.StrengthChanged.Invoke();
                break;
        }
    }
}