using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Portal : Interactor
{
    private PlayerController _player;
    private UIManager _uiManager;
    public EPortalType portalType;
    private Vector3 _destination;
    public SecretRoom connectedSecretRoom;
    private readonly static float[] _secretRoomChanceByLevel = new float[]
    {
        //0.1f, 0.8f + 0.1f, 1.0f,
        0.0f, 0.0f, 1.0f,
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
                _uiManager.DisableMap();
                SetSecretRoomTeleportPosition();
                break;
            
            case EPortalType.SecretToCombat:
                break;
            
            case EPortalType.CombatToMeta:
                
                break;
            
            case EPortalType.MetaToCombat:
                
                break;
        }

        StartCoroutine(TeleportCoroutine());
    }

    private void SetSecretRoomTeleportPosition()
    {
        // Randomly select room level to teleport to
        int roomLevel = Array.FindIndex(_secretRoomChanceByLevel, possibility => possibility >= Random.value);
        
        // Randomly select secret room
        List<SecretRoom> rooms = LevelManager.Instance.GetSecretRooms(roomLevel);
        connectedSecretRoom = rooms[Random.Range(0, rooms.Count)];
        connectedSecretRoom.PrewarmRoom();

        // Teleport
        _destination = connectedSecretRoom.transform.position;
    }

    private IEnumerator TeleportCoroutine()
    {
        var playerSpriteRenderer = _player.gameObject.GetComponent<SpriteRenderer>();
        
        // Start teleport
        _player.DisablePlayerInput();
        
        // Fade in
        while (playerSpriteRenderer.color.a > 0)
        {
            var color = playerSpriteRenderer.color;
            playerSpriteRenderer.color = new Color(color.r, color.g, color.b, color.a - 1f * Time.unscaledDeltaTime);
            yield return null;
        }
        
        // Teleport
        _player.transform.position = _destination;
        yield return null;
        yield return null;
        
        // Fade out
        while (playerSpriteRenderer.color.a < 1)
        {
            var color = playerSpriteRenderer.color;
            playerSpriteRenderer.color = new Color(color.r, color.g, color.b, color.a + 1f * Time.unscaledDeltaTime);
            yield return null;
        }
        
        // End teleport
        _player.EnablePlayerInput();
        OnEndTeleport();
    }

    private void OnEndTeleport()
    {
        switch (portalType)
        {
            case EPortalType.CombatToSecret:
                connectedSecretRoom.OnEnter(transform.position);
                Destroy(gameObject);
                break;
            
            case EPortalType.SecretToCombat:
                connectedSecretRoom.OnExit();
                break;
            
            case EPortalType.CombatToMeta:
                break;
            
            case EPortalType.MetaToCombat:
                // Do something
                break;
        }
        IsInteracting = false;
    }

    public void SetDestination(Vector3 pos) => _destination = pos;
}
