using System.Collections.Generic;
using UnityEngine;

public class NightShadeCollider : MonoBehaviour
{
    private PlayerController _playerController;
    private PlayerMovement _playerMovement;
    private List<Collider2D> _chasingEnemies;
    
    private void Awake()
    {
        _playerController = PlayerController.Instance;
        _playerMovement = _playerController.playerMovement;
        _chasingEnemies = new List<Collider2D>();
    }

    private void OnEnable()
    {
        _chasingEnemies.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Add enemy collider to the list
        _chasingEnemies.Add(other);
        if (_chasingEnemies.Count > 1) return;
        
        // If this is the first enemy, start chasing
        // Turn on effect and adjust speed
        _playerController.SetVFXActive(EStatusEffect.Fast, true);
        _playerMovement.moveSpeedMultiplier = _playerController.NightShadeFastChaseStats[(int)_playerController.NightShadeFastChasePreserv];
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Remove enemy collider from the list
        _chasingEnemies.Remove(other);
        if (_chasingEnemies.Count > 0) return;
        
        // If this is the last enemy, end chasing
        // Turn off effect and adjust speed
        _playerController.SetVFXActive(EStatusEffect.Fast, false);
        _playerMovement.moveSpeedMultiplier = 1.0f;
    }
}
