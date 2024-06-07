using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Interactor : MonoBehaviour
{
    protected InputAction InteractAction;
    protected bool IsInteracting;
    private bool _isInteractionBound;
    private int _playerInteractingPartsCount;
    [SerializeField] protected GameObject descriptionUI;

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
        InteractAction = PlayerController.Instance.playerInput.actions["Player/Interact"];
        _isInteractionBound = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_playerInteractingPartsCount == 0)
            {
                InteractAction.performed += OnInteract;
                ShowDescriptionUI();
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
                InteractAction.performed -= OnInteract;
                IsInteracting = false;
                HideDescriptionUI();
            }
        }
    }

    private void ShowDescriptionUI()
    {
        if (descriptionUI == null) return;
        descriptionUI.SetActive(true);
    }

    private void HideDescriptionUI()
    {
        if (descriptionUI == null) return;
        descriptionUI.SetActive(false);
    }

    protected abstract void OnInteract(InputAction.CallbackContext obj);
}
