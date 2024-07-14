using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Portal : Interactor
{
    public bool _isHidden;
    private PlayerController _player;
    private UIManager _uiManager;
    private AudioManager _audioManager;
    public EPortalType portalType;
    private Vector3 _destination;
    public HiddenRoom connectedHiddenRoom;
    private GameObject _cameras;
    private readonly static float[] HiddenRoomChanceByLevel = new float[]
    { 
        0.2f, 0.65f, 1f,
    };

    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        if (IsInteracting) return;
        IsInteracting = true;
        _player = PlayerController.Instance;
        _uiManager = UIManager.Instance;
        _audioManager = AudioManager.Instance;
        
        // Get destination location
        switch (portalType)
        {
            case EPortalType.CombatToSecret:
                _audioManager.StopBgm(0.5f);
                SetHiddenRoomTeleportPosition();
                break;
            
            case EPortalType.SecretToCombat:
                
                break;
            
            case EPortalType.MetaToCombat:
                _audioManager.StopBgm(0.5f);
                _destination = LevelManager.Instance.CombatSpawnPoint;
                break;
            
            case EPortalType.CombatToMidBoss:
                _audioManager.StopBgm(2f);
                break;
            
            case EPortalType.MidBossToCombat:
                _audioManager.StopBgm(2f);
                break;
                
            case EPortalType.CombatToBoss:
                _audioManager.StopBgm(2f);
                break;
        }

        StartCoroutine(TeleportCoroutine());
    }

    private void SetHiddenRoomTeleportPosition()
    {
        // Randomly select room level to teleport to
        int roomLevel = Array.FindIndex(HiddenRoomChanceByLevel, possibility => possibility >= Random.value);
        
        // Randomly select secret room
        List<HiddenRoom> rooms = LevelManager.Instance.GetHiddenRooms(roomLevel);
        connectedHiddenRoom = rooms[Random.Range(0, rooms.Count)];
        connectedHiddenRoom.PrewarmRoom();

        // Teleport
        _destination = connectedHiddenRoom.transform.position;
    }

    private IEnumerator TeleportCoroutine()
    {
        // Start teleport
        _player.DisablePlayerInput();
        yield return StartCoroutine(_player.FadeOutCoroutine());
        
        // Teleport
        // Case 1: Loading a new scene
        switch (portalType)
        {
            // Case 2: Teleporting within a scene
            case EPortalType.CombatToSecret:
            case EPortalType.SecretToCombat:
                _player.transform.position = _destination;
                yield return null;
                break;
            
            case EPortalType.MetaToCombat:
                _player.transform.position = _destination;
                CameraManager.Instance.SwapCamera(CameraManager.Instance.AllVirtualCameras[1]);
                yield return null;
                break;
            
            case EPortalType.CombatToMidBoss:
                _player.playerMovement.ResetEnteredGroundCount();
                GameManager.Instance.LoadMidBossMap();
                yield break;
            
            case EPortalType.MidBossToCombat:
                _player.playerMovement.ResetEnteredGroundCount();
                GameManager.Instance.LoadPostMidBossCombatMap();
                CameraManager.Instance.SwapCamera(CameraManager.Instance.AllVirtualCameras[1]);
                yield break;
                
            case EPortalType.CombatToBoss:
                _player.playerMovement.ResetEnteredGroundCount();
                GameManager.Instance.LoadBossMap();
                yield break;
        }
        
        // End teleport in Case 2
        OnPlayerEnterNextRoom();
        yield return StartCoroutine(_player.FadeInCoroutine());
        OnEndTeleport();
    }

    private void OnPlayerEnterNextRoom()
    {
        switch (portalType)
        {
            case EPortalType.CombatToSecret:
                _uiManager.DisableMap();
                connectedHiddenRoom.OnEnter(transform.position);
                break;
            
            case EPortalType.SecretToCombat:
                _uiManager.EnableMap();
                _audioManager.StartCombatBgm();
                connectedHiddenRoom.OnExit();
                break;
            
            case EPortalType.MetaToCombat:
                _uiManager.EnableMap();
                _audioManager.StartCombatBgm();
                break;
        }
    }
    
    private void OnEndTeleport()
    {
        switch (portalType)
        {
            case EPortalType.CombatToSecret:
                Destroy(_isHidden? gameObject.transform.parent.gameObject : gameObject);
                break;
            
            case EPortalType.SecretToCombat:
                break;
            
            case EPortalType.MetaToCombat:
                break;
        }
        IsInteracting = false;
    }

    public void SetDestination(Vector3 pos) => _destination = pos;
}
