using Enums.PlayerEnums;
using Managers;
using Structs.PlayerStructs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SkillDescriptionDisplay : MonoBehaviour
{
    Dictionary<int, SAbilityData>[] _abilitiesByMetals;
    [SerializeField] GameObject[] selectionButtons;
    [SerializeField] int[] generatedNumbers = new int[5];

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
            RandomSkill(abilityDict.Count);

            //The Name_EN will change to Des_EN when the description is ready.
            if (abilityDict.TryGetValue(RandomSkill(abilityDict.Count), out SAbilityData ability1))
            {
                TextMeshProUGUI skillDescriptionText = selectionButtons[0].GetComponentInChildren<TextMeshProUGUI>();
                skillDescriptionText.text = ability1.Name_EN;
            }

            if (abilityDict.TryGetValue(RandomSkill(abilityDict.Count), out SAbilityData ability2))
            {
                TextMeshProUGUI skillDescriptionText = selectionButtons[1].GetComponentInChildren<TextMeshProUGUI>();
                skillDescriptionText.text = ability2.Name_EN;
            }

            if (abilityDict.TryGetValue(RandomSkill(abilityDict.Count), out SAbilityData ability3))
            {
                TextMeshProUGUI skillDescriptionText = selectionButtons[2].GetComponentInChildren<TextMeshProUGUI>();
                skillDescriptionText.text = ability3.Name_EN;
            }
            

        }
    }
    
    int RandomSkill(int numberOfSkills)
    {
        int randomNumber = UnityEngine.Random.Range(1, numberOfSkills + 1);

        return randomNumber;

        /*
        for (int i = 0; i < 3; i++)
        {
            int randomNumber = UnityEngine.Random.Range(1, numberOfSkills + 1);

            
            // Check if the generated number already exists in the array
            while (Array.IndexOf(generatedNumbers, randomNumber) != -1)
            {
                randomNumber = UnityEngine.Random.Range(1, numberOfSkills + 1);
            }

            Debug.Log(randomNumber);
            generatedNumbers.Append(randomNumber);
            Debug.Log("randomNumber : " + generatedNumbers[i]);
        }*/

    }
}