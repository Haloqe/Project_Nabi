using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InteractorHold : MonoBehaviour
{
    private InputAction _interactAction;
    private int _playerInteractingPartsCount;

    protected virtual void Start()
    {
        _interactAction = FindObjectOfType<PlayerInput>().actions["Player/Interact_Hold"];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_playerInteractingPartsCount == 0)
            {
                OnEnterArea();
                _interactAction.started += OnInteractStarted;
                _interactAction.performed += OnInteractPerformed;
                _interactAction.canceled += OnInteractCanceled;
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
                OnExitArea();
                _interactAction.started -= OnInteractStarted;
                _interactAction.performed -= OnInteractPerformed;
                _interactAction.canceled -= OnInteractCanceled;
            }
        }
    }

    protected abstract void OnEnterArea();
    protected abstract void OnExitArea();
    protected abstract void OnInteractStarted(InputAction.CallbackContext obj);
    protected abstract void OnInteractPerformed(InputAction.CallbackContext obj);
    protected abstract void OnInteractCanceled(InputAction.CallbackContext obj);
}
