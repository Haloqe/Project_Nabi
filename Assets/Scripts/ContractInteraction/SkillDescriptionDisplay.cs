using System.Collections.Generic;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public class SkillDescriptionDisplay : MonoBehaviour
{
    Dictionary<int, SAbilityData> _abilitiesByMetal;
    [SerializeField] GameObject[] selectionButtons;

    //[SerializeField] Sprite defaultSelectionSprite;
    [SerializeField] Sprite chosenSelectionSprite;

    List<int> generatedNumbers = new List<int>();

    public GameObject test;

    void Start()
    {
        Init();
        DisplayDescription();
    }

    void Init()
    {
        setButtonState(true);
        //it is calling EAbilityMetalType.Gold for now but it will later change depending on which metalContractor you encoutnered.
        _abilitiesByMetal = PlayerAbilityManager.Instance.GetAbilitiesByMetal(EAbilityMetalType.Gold);

    }

    //Display the description of randomly chosen skills in a given metal.
    public void DisplayDescription()
    {
        RandomSkill(_abilitiesByMetal.Count);

        for (int i = 0; i < 3; i++)
        {
            TextMeshProUGUI skillDescriptionText = selectionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            Debug.Log(_abilitiesByMetal[generatedNumbers[i]].Name_EN);
            skillDescriptionText.text = _abilitiesByMetal[generatedNumbers[i]].Name_EN;
        }
    }

    public void OnButtonSelected(int index)
    {
        Image buttonImage;

        buttonImage = selectionButtons[index].GetComponent<Image>();
        buttonImage.sprite = chosenSelectionSprite;
        
        //bind on key 0 by default for now
        //PlayerAbilitymanager.Instance.BindActiveAbility (some operation to get key, generatedNumbers[index]);
        PlayerAbilityManager.Instance.BindActiveAbility(0, 6);
        
    }

    //Randomly choose a skill index that is (1)non-duplicate and (2)not bound already.
    void RandomSkill(int numberOfSkills)
    {
        while (generatedNumbers.Count <= 3)
        {
            int randomNumber = UnityEngine.Random.Range(1, numberOfSkills);


            while (generatedNumbers.Contains(randomNumber) && !PlayerAbilityManager.Instance.CheckabilityAlreadyBound(randomNumber))
            {
                randomNumber = UnityEngine.Random.Range(1, numberOfSkills);
            }

            generatedNumbers.Add(randomNumber);
        }
    }

    void setButtonState(bool state)
    {
        for (int i = 0; i < 3; i++)
        {
            Button button = selectionButtons[i].GetComponent<Button>();
            button.interactable = state;
        }
    }
}

