using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillDescriptionDisplay : MonoBehaviour
{
    Dictionary<int, SAbilityData> _abilitiesByMetal;
    [SerializeField] GameObject[] selectionButtons;
    List<int> generatedNumbers = new List<int>();

    void Start()
    {
        Init();
        DisplayDescription();
    }

    void Init()
    {
        //it is called EAbilityMetalType.Gold for now but it will later change depending on which metalContractor you encoutnered.
        //_abilitiesByMetals = PlayerAbilityManager.GetAbilitiesByMetals(EAbilityMetalType.Gold);
        _abilitiesByMetal = PlayerAbilityManager.Instance.GetAbilitiesByMetal(EAbilityMetalType.Gold);
    }

    public void DisplayDescription()
    {
        RandomSkill(_abilitiesByMetal.Count);

        for (int i = 0; i < 3; i++)
        {
            TextMeshProUGUI skillDescriptionText = selectionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            skillDescriptionText.text = _abilitiesByMetal[generatedNumbers[i]].Name_EN;
        }
    }
    
    void RandomSkill(int numberOfSkills)
    {
        while (generatedNumbers.Count <= 3)
        {
            int randomNumber = UnityEngine.Random.Range(1, numberOfSkills);
            
            while (generatedNumbers.Contains(randomNumber))
            {
                randomNumber = UnityEngine.Random.Range(1, numberOfSkills);
            }

            generatedNumbers.Add(randomNumber);
        }
    }
}
