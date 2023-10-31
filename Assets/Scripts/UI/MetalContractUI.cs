using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MetalContractUI : MonoBehaviour
{
    private bool _isInitialised; 
    private Color _selectedColor;
    private Color _unselectedColor;
    private int _numUsedSlots = 5;
    private int _hoveredSlotIdx = -1;
    private List<SAbilityData> _abilitiesToDisplay;
    public MetalContractItem ActiveContractItem;
    [SerializeField] private Image[] _abilityIcons;
    [SerializeField] private TextMeshProUGUI _abilityNameText;
    [SerializeField] private TextMeshProUGUI _metalNameText;
    [SerializeField] private TextMeshProUGUI _abilityDescText;


    private void Awake()
    {
        _selectedColor = Color.white;
        _unselectedColor = new Color(0.45f, 0.45f, 0.45f);
        _abilitiesToDisplay = new List<SAbilityData>();
    }

    private void OnEnable()
    {
        // TODO FIX 여기서 하지 말기 지금은 디버깅용. 맵 로딩시 같이 처리하기.
        if (!_isInitialised)
        {
            Initialize(); 
            _isInitialised = true;
        }

        foreach (var icon in _abilityIcons)
        {
            icon.color = _unselectedColor;
        }
        _hoveredSlotIdx = -1;
        _abilityDescText.text = "";
        _abilityNameText.text = "";
    }

    public void Initialize()
    {
        // TODO select a metal to display
        EAbilityMetalType metal = EAbilityMetalType.Gold;

        // TODO change the look of the contract item 

        _metalNameText.text = metal.ToString();

        // Retreive a list of abilities that are not collected yet
        var possibleAbilities = PlayerAbilityManager.Instance.GetAbilitiesByMetal(metal, true);
        _numUsedSlots = Mathf.Clamp(possibleAbilities.Count, possibleAbilities.Count, 5);

        // Select random abilities to display
        var rnd = new System.Random();
        _abilitiesToDisplay = possibleAbilities.OrderBy(x => rnd.Next()).Take(_numUsedSlots).ToList();

        // Change ability icon
        for (int i = 0; i < _numUsedSlots; i++)
        {
            _abilityIcons[i].sprite = PlayerAbilityManager.Instance.GetAbilityIconSprite(_abilitiesToDisplay[i].Id);
        }
    }

    public void OnMouseEnterIcon(int index)
    {
        if (index >= _numUsedSlots || index == _hoveredSlotIdx) return;
        
        // Change icon color
        if (_hoveredSlotIdx != -1)
        {
            _abilityIcons[_hoveredSlotIdx].color = _unselectedColor;
        }
        _abilityIcons[index].color = _selectedColor;
        _hoveredSlotIdx = index;

        // Change text
        _abilityNameText.text = _abilitiesToDisplay[index].Name_KO;
        _abilityDescText.text = _abilitiesToDisplay[index].Des_KO;
    }

    public void OnMouseClickIcon(int index)
    {
        PlayerAbilityManager.Instance.CollectAbility(_abilitiesToDisplay[index].Id);
        Destroy(ActiveContractItem.gameObject);
        UIManager.Instance.CloseFocusedUI();
    }
}