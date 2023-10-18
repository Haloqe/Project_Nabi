//MakeContract
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Structs.PlayerStructs;
using Player.Abilities.Base;
using UnityEngine.InputSystem;
using System;
using TMPro;
using Managers;
using Enums.PlayerEnums;

//This class opens the Skill Selection UI when player presses F within a range set by sphere 2D collider (temporary collider for testing purpose).
public class MakeContract : MonoBehaviour
{
    public GameObject skillSelectionUI;

    // This variable checks if the player is close enough, i.e., triggering the sphere 2D collider
    public bool playerIsClose;

    //When the player is close and pressed F, set Active skill Selection UI.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && playerIsClose)
        {
            if (skillSelectionUI.activeInHierarchy)
            {
                ZeroDescription();
            }
            else
            {
                skillSelectionUI.SetActive(true);
            }
        }
    }

    public void ZeroDescription()
    {
        skillSelectionUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = true;
        }
    }

    //You can exit the Skill Selection UI if you move far away from the Game Object Contractor or press F again.
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            ZeroDescription();
        }
    }

}