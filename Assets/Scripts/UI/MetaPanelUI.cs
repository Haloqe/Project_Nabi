using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MetaPanelUI : MonoBehaviour
{
    [SerializeField] private MetaUIController baseUI;
    [SerializeField] private GameObject selectedOutline;
    [FormerlySerializedAs("unSelectedOutline")] [SerializeField] private GameObject unselectedOutline;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image[] levelSlots;
    [SerializeField] private GameObject[] levelDescriptions;
    [SerializeField] private int[] levelUnlockCosts;
    public int metaIndex;
    private int _unlockedLevel;
    private int _selectedLevel;
    
    public void OnMetaUIOpen()
    {
        _unlockedLevel = GameManager.Instance.PlayerMetaData.metaUpgradeLevelsTemporary[metaIndex];
        _selectedLevel = 0;
        
        unselectedOutline.SetActive(true);
        selectedOutline.SetActive(false);

        var lockedColour = new Color(0.85f, 0.83f, 0.8f, 1f);
        for (int i = 0; i < 3; i++)
        {
            levelSlots[i].color = i <= _unlockedLevel ? Color.white : lockedColour;
        }
        if (_unlockedLevel == -1)
        {
            levelDescriptions[_selectedLevel].SetActive(false);
            costText.text = Define.Localisation == ELocalisation.ENG ? "No upgrades" : "보유한 업그레이드 없음";
        }
        else
        {
            SelectLevelSlot(_unlockedLevel);
            levelSlots[_selectedLevel].sprite = baseUI.unselectedLevelSlotSprite;
        }
    }

    public void OnSelectMetaPanel()
    {
        unselectedOutline.SetActive(false);
        selectedOutline.SetActive(true);
        SelectLevelSlot(_unlockedLevel == -1 ? 0 : _unlockedLevel);
    }
    
    public void OnUnselectMetaPanel()
    {
        unselectedOutline.SetActive(true);
        selectedOutline.SetActive(false);
        if (_unlockedLevel == -1)
        {
            levelDescriptions[_selectedLevel].SetActive(false);
            costText.text = Define.Localisation == ELocalisation.ENG ? "No upgrades" : "보유한 업그레이드 없음";
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
        levelDescriptions[_selectedLevel].SetActive(false);
        _selectedLevel = level;
        
        // Select new
        levelSlots[_selectedLevel].sprite = baseUI.selectedLevelSlotSprite;
        levelDescriptions[_selectedLevel].SetActive(true);
        costText.text = levelUnlockCosts[_selectedLevel] + (Define.Localisation == ELocalisation.ENG ? " Soul Shards" : " 영혼 조각");
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
        GameManager.Instance.PlayerMetaData.metaUpgradeLevelsTemporary[metaIndex] = ++_unlockedLevel;
        baseUI.ApplyMetaUpgrade(metaIndex, _unlockedLevel);
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
