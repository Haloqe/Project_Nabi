using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Structs.PlayerStructs;
using Player.Abilities.Base;
using UnityEngine.InputSystem;
using System;
using Random = UnityEngine.Random;
using TMPro;
using Managers;
using Enums.PlayerEnums;

public class MakeContract : MonoBehaviour
{
    public GameObject skillSelectionUI;
    public Text skillDescriptionText;
    public string[] skillDescription;
    
    public bool playerIsClose;
    public float wordSpeed = 0.2f;
    //public static Dictionary<int, SAbilityData>[] abilities = PlayerAbilityManager.GetAbilitiesByMetals(EAbilityMetalType.Gold);
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F) && playerIsClose)
        {
            if(skillSelectionUI.activeInHierarchy)
            {
                InitDescription();
            }
            else
            {
                skillSelectionUI.SetActive(true);
                DisplayDescription();
            }
        }
    }

    public void InitDescription()
    {
        skillSelectionUI.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D other){
        if(other.tag == "Player")
        {
            playerIsClose = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other){
        if(other.tag == "Player")
        {
            playerIsClose = false;
            InitDescription();
        }
    }

    IEnumerator DisplayDescription()
    {
        int skillToDisplay = RandomSkill();

        Dictionary<int, SAbilityData>[] abilities = PlayerAbilityManager.GetAbilitiesByMetals(EAbilityMetalType.Gold);
        foreach (Dictionary<int, SAbilityData> abilityDict in abilities)
        {
            if (abilityDict.TryGetValue(skillToDisplay, out SAbilityData ability))
            {
                foreach (char letter in ability.Des_EN.ToCharArray())
                {
                    skillDescriptionText.text += letter;
                    yield return new WaitForSeconds(wordSpeed);
                }
            }
        }
    }

    public int RandomSkill()
    {
        int randomIndex = Random.Range(1,8);
        return randomIndex;
    }
}
