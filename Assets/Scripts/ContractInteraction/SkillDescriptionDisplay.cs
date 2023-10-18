using Enums.PlayerEnums;
using Managers;
using Structs.PlayerStructs;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillDescriptionDisplay : MonoBehaviour
{
    Dictionary<int, SAbilityData>[] _abilitiesByMetals;
    [SerializeField] GameObject[] selectionButtons;

    void Start()
    {
        Init();
        DisplayDescription();
    }

    void Init()
    {
        //it is called EAbilityMetalType.Gold for now but it will later change depending on which metalContractor you encoutnered.
        _abilitiesByMetals = PlayerAbilityManager.GetAbilitiesByMetals(EAbilityMetalType.Gold);
    }

    public void DisplayDescription()
    {
        foreach (Dictionary<int, SAbilityData> abilityDict in _abilitiesByMetals)
        {
            if (abilityDict.TryGetValue(1, out SAbilityData ability))
            {
                TextMeshProUGUI skillDescriptionText = selectionButtons[0].GetComponentInChildren<TextMeshProUGUI>();
                skillDescriptionText.text = ability.Des_EN;
                Debug.Log(skillDescriptionText.text);
                
            }
        }
    }
}