using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    // Level Details
    public int PrevStage = 0;
    public int CurrStage = 0;

    // Used for changing active scene through portals
    public Vector3 PlayerPrevPosition { get; set; }
    public Vector3 PlayerNextPosition { get; set; }


    public void SaveStageInfo()
    {
        PrevStage = CurrStage;
        //PlayerPrevPosition = PlayerController.Instance.transform.position;
    }

}
