using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Interactor : MonoBehaviour
{
    private InputAction _interactAction;
    protected bool _isInteracting;

    protected virtual void Start()
    {
        _interactAction = FindObjectOfType<PlayerInput>().actions["Player/Interact"];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _interactAction.performed += OnInteract;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _interactAction.performed -= OnInteract;
        }
    }

    protected abstract void OnInteract(InputAction.CallbackContext obj);
}
