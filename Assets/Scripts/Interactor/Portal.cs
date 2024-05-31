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
    public EPortalType portalType;
    private Vector3 _destination;
    public HiddenRoom connectedHiddenRoom;
    private readonly static float[] HiddenRoomChanceByLevel = new float[]
    { 
        0.50f, 0.95f, 1.0f,
    };

    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        if (IsInteracting) return;
        IsInteracting = true;
        _player = PlayerController.Instance;
        _uiManager = UIManager.Instance;
        
        // Get destination location
        switch (portalType)
        {
            case EPortalType.CombatToSecret:
                SetHiddenRoomTeleportPosition();
                break;
            
            case EPortalType.SecretToCombat:
                break;
            
            case EPortalType.MetaToCombat:
                _destination = LevelManager.Instance.CombatSpawnPoint.position;
                break;
            
            case EPortalType.CombatToBoss:
                TeleportToBossMap();
                return;
        }

        StartCoroutine(TeleportCoroutine());
    }

    private void TeleportToBossMap()
    {
        _player.playerMovement.ResetEnteredGroundCount();
        GameManager.Instance.LoadBossMap();
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
        var playerSpriteRenderer = _player.gameObject.GetComponent<SpriteRenderer>();
        
        // Start teleport
        _player.DisablePlayerInput();
        
        // Fade in
        var color = playerSpriteRenderer.color;
        while (playerSpriteRenderer.color.a > 0)
        {
            color.a -= 1f * Time.unscaledDeltaTime;
            playerSpriteRenderer.color = color;
            yield return null;
        }
        
        // Teleport
        _player.transform.position = _destination;
        yield return null;
        OnPlayerEnterNextRoom();
        yield return null;
        
        // Fade out
        while (playerSpriteRenderer.color.a < 1)
        {
            color.a += 1f * Time.unscaledDeltaTime;
            playerSpriteRenderer.color = color;
            yield return null;
        }
        
        // End teleport
        _player.EnablePlayerInput();
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
                connectedHiddenRoom.OnExit();
                break;
            
            case EPortalType.MetaToCombat:
                _uiManager.EnableMap();
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
                // Do something
                break;
        }
        IsInteracting = false;
    }

    public void SetDestination(Vector3 pos) => _destination = pos;
}
