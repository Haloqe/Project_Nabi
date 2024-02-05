using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Portal : Interactor
{
    [SerializeField] private EPortalType PortalType;

    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        // Save current level and player position
        GameManager.Instance.SaveStageInfo();

        // Move to next area
        switch (PortalType)
        {
            case EPortalType.GeneralStore:
                {
                    LoadGeneralStore();
                }
                break;
            case EPortalType.FoodStore:
                {
                    LoadFoodStore();
                }
                break;
            case EPortalType.NextLevel:
                {
                    LoadNextLevel();
                }
                break;
        }


    }

    private void LoadGeneralStore()
    {
        string currStage = GameManager.Instance.CurrStage.ToString().PadLeft(2, '0');
        string sceneName = "InGame_Stage_" + currStage + "_GeneralStore";
        if (!Utility.LoadSceneByNameSafe(sceneName)) return;
    }

    private void LoadFoodStore()
    {
        string currStage = GameManager.Instance.CurrStage.ToString().PadLeft(2, '0');
        string sceneName = "InGame_Stage_" + currStage + "_FoodStore";
        if (!Utility.LoadSceneByNameSafe(sceneName)) return;
    }

    // TODO
    private void LoadNextLevel()
    {
        string currStage = (GameManager.Instance.CurrStage + 1).ToString().PadLeft(2, '0');
        string sceneName = "InGame_Stage_" + currStage;
        Debug.AssertFormat(Utility.LoadSceneByNameSafe(sceneName), "Cannot find next level!");
    }
}
