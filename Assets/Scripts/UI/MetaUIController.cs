using System;
using TMPro;
using UnityEngine;

public class MetaUIController : UIControllerBase
{
    private PlayerMetaData _metaData;
    public PlayerInventory PlayerInventory { get; private set; } 
    public Sprite selectedLevelSlotSprite;
    public Sprite unselectedLevelSlotSprite;
    private int _selectedPanelIdx;
    private MetaPanelUI[] _metaPanels;
    private Animator _animator;
    private bool _readyToNavigate;

    [SerializeField] private TextMeshProUGUI warningObjTMP;
    private readonly static int Display = Animator.StringToHash("Display");

    public void BindEvents()
    {
        GameEvents.Restarted += ApplySavedUpgrades;
        PlayerEvents.SpawnedFirstTime += Initialise;
    }

    private void Initialise()
    {
        _readyToNavigate = false;
        _metaData = GameManager.Instance.PlayerMetaData;
        PlayerInventory = PlayerController.Instance.playerInventory;
        _animator = GetComponentInChildren<Animator>();
        _metaPanels = GetComponentsInChildren<MetaPanelUI>();
        for (int i = 0; i < 5; i++)
        {
            _metaPanels[i].metaIndex = i;
        }
        ApplySavedUpgrades();
    }

    private void OnDestroy()
    {
        GameEvents.Restarted -= ApplySavedUpgrades;
        PlayerEvents.SpawnedFirstTime -= Initialise;
    }
    
    private void OnEnable()
    {
        _animator.Rebind();
        _animator.Update(0f);
        _selectedPanelIdx = -1;
        for (int i = 0; i < 5; i++) _metaPanels[i].OnMetaUIOpen();
        SelectPanel(0);
        _readyToNavigate = true;
    }
    
    private void Start()
    {
        _selectedPanelIdx = -1;
        SelectPanel(0);
        _readyToNavigate = true;
    }

    private void ApplySavedUpgrades()
    {
        Debug.Log("ApplySavedUpgrades");
        int[] upgrades = GameManager.Instance.PlayerMetaData.metaUpgradeLevels;
        for (int i = 0; i < 5; i++)
        {
            for (int lv = 0; lv <= upgrades[i]; lv++)
                ApplyMetaUpgrade(i, lv);
        }
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
        warningObjTMP.text = Define.Localisation == ELocalisation.ENG ? "You already have this upgrade" : "이미 보유한 업그레이드예요";
    }
    
    public void DisplayUnreachableLevelWarning()
    {
        _animator.SetTrigger(Display);
        warningObjTMP.text = Define.Localisation == ELocalisation.ENG ? "Please upgrade the lower tier first" : "더 낮은 등급의 업그레이드를 먼저 해주세요";
    }
    
    public void DisplayNotEnoughShardsWarning()
    {
        _animator.SetTrigger(Display);
        warningObjTMP.text = Define.Localisation == ELocalisation.ENG ? "You don’t have enough Soul Shards" : "영혼 조각이 부족해요";
    }

    public void ApplyMetaUpgrade(int metaIndex, int unlockedLevel)
    {
        switch ((EMetaUpgrade)metaIndex)
        {
            case EMetaUpgrade.BetterLegacyPreserv: // 희귀한 유산을 얻을 확률이 증가한다
                // Handled separately - no need for a direct update
                break;
            
            case EMetaUpgrade.Resurrection: // 사망시 더 낮은 체력으로 1회 부활한다
                if (unlockedLevel == 0) 
                    PlayerController.Instance.playerDamageReceiver.canResurrect = true;
                break;
            
            case EMetaUpgrade.HealthAddition: // 최대 체력이 증가한다
                PlayerController.Instance.playerDamageReceiver.additionalHealth += Define.MetaHealthAdditions[unlockedLevel];
                PlayerController.Instance.playerDamageReceiver.ChangeHealthByAmount(0);
                break;
            
            case EMetaUpgrade.CritRateAddition: // 크리티컬 확률이 증가한다
                PlayerController.Instance.AddCriticalRate(Define.MetaCriticalRateAdditions[unlockedLevel]);
                break;
            
            case EMetaUpgrade.StrengthAddition: // 공격력이 증가한다
                PlayerController.Instance.strengthAddition += Define.MetaAttackDamageAdditions[unlockedLevel];
                PlayerEvents.StrengthChanged.Invoke();
                break;
        }
    }
}