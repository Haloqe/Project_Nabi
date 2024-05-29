using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Interactor : MonoBehaviour
{
    private InputAction _interactAction;
    protected bool IsInteracting;
    private bool _isInteractionBound;
    private int _playerInteractingPartsCount;

    protected virtual void Start()
    {
        var player = PlayerController.Instance;
        if (player != null) BindInteraction();
        PlayerEvents.Spawned += BindInteraction;
    }

    protected virtual void OnDestroy()
    {
        PlayerEvents.Spawned -= BindInteraction;
    }

    private void BindInteraction()
    {
        if (_isInteractionBound) return;
        _interactAction = PlayerController.Instance.playerInput.actions["Player/Interact"];
        _isInteractionBound = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_playerInteractingPartsCount == 0)
            {
                _interactAction.performed += OnInteract;
            }
            _playerInteractingPartsCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInteractingPartsCount--;
            if (_playerInteractingPartsCount == 0)
            {
                _interactAction.performed -= OnInteract;
                IsInteracting = false;
            }
        }
    }

    protected abstract void OnInteract(InputAction.CallbackContext obj);
}
