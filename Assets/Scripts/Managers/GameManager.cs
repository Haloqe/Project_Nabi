using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public bool ShouldRandomGenerateMap = false;
    public GameObject Player;

    // Level Details
    public int PrevStage = 0;
    public int CurrStage = 0;

    // Used for changing active scene through portals
    private Vector3 _playerPrevPosition { get; set; }
    private Vector3 _playerNextPosition { get; set; }


    public void SaveStageInfo()
    {
        PrevStage = CurrStage;
        //PlayerPrevPosition = PlayerController.Instance.transform.position;
    }

    private void Start()
    {
        if (ShouldRandomGenerateMap) LevelManager.Instance.Generate();
    }

}
