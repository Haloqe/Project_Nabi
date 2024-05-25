using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetaPanelUI : MonoBehaviour
{
    [SerializeField] private MetaUIController baseUI;
    [SerializeField] private GameObject selectedOutline;
    [SerializeField] private GameObject unSelectedOutline;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image[] levelSlots;
    [SerializeField] private string[] levelDescriptions;
    [SerializeField] private int[] levelUnlockCosts;
    public int metaIndex;
    public PlayerMetaInfo MetaInfo;
    private int _unlockedLevel;
    private int _selectedLevel;
    
    public void Awake()
    {
        _unlockedLevel = MetaInfo.MetaUpgradeLevels[metaIndex];
        _selectedLevel = 0;
        
        unSelectedOutline.SetActive(true);
        selectedOutline.SetActive(false);
        
        for (int i = 0; i <= _unlockedLevel; i++)
        {
            levelSlots[i].color = Color.white;
        }
        if (_unlockedLevel == -1)
        {
            descText.text = "";
            costText.text = "보유한 업그레이드 없음";
        }
        else
        {
            SelectLevelSlot(_unlockedLevel);
            levelSlots[_selectedLevel].sprite = baseUI.unselectedLevelSlotSprite;
        }
    }

    public void OnSelectMetaPanel()
    {
        unSelectedOutline.SetActive(false);
        selectedOutline.SetActive(true);
        SelectLevelSlot(_unlockedLevel == -1 ? 0 : _unlockedLevel);
    }
    
    public void OnUnselectMetaPanel()
    {
        unSelectedOutline.SetActive(true);
        selectedOutline.SetActive(false);
        if (_unlockedLevel == -1)
        {
            descText.text = "";
            costText.text = "보유한 업그레이드 없음";
        }
        else
        {
            SelectLevelSlot(_unlockedLevel);
        }
        levelSlots[_selectedLevel].sprite = baseUI.unselectedLevelSlotSprite;
    }

    private void SelectLevelSlot(int level)
    {
        // Deselect previous
        levelSlots[_selectedLevel].sprite = baseUI.unselectedLevelSlotSprite;
        _selectedLevel = level;
        
        // Select new
        levelSlots[_selectedLevel].sprite = baseUI.selectedLevelSlotSprite;
        descText.text = _selectedLevel + 1 + "단계 | " + levelDescriptions[_selectedLevel];
        costText.text = levelUnlockCosts[_selectedLevel] + " 영혼 조각";
    }

    public void OnSubmit()
    {
        // Already unlocked?
        if (_unlockedLevel >= _selectedLevel)
        {
            baseUI.DisplayAlreadyUnlockedWarning();
        }
        // Unreachable level?
        else if (_unlockedLevel + 1 != _selectedLevel)
        {
            baseUI.DisplayUnreachableLevelWarning();
        }
        else
        {
            // Has enough shards?
            bool upgradeSucceed = baseUI.PlayerInventory.TryBuyWithSoulShard(levelUnlockCosts[_selectedLevel]);
            if (!upgradeSucceed)
            {
                baseUI.DisplayNotEnoughShardsWarning();
            }
            else
            {
                UnlockUpgrade();
            }
        }
    }

    private void UnlockUpgrade()
    {
        levelSlots[_selectedLevel].color = Color.white;
        MetaInfo.MetaUpgradeLevels[metaIndex] = ++_unlockedLevel;
    }
    
    public void OnNavigate(Vector2 value)
    {
        if (value.x < 0)
        {
            if (_selectedLevel == 0) return;
            SelectLevelSlot(_selectedLevel - 1);
        }
        else if (value.x > 0)
        {
            if (_selectedLevel == 2) return;
            SelectLevelSlot(_selectedLevel + 1);
        }
    }
}
