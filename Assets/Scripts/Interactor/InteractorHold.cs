using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InteractorHold : MonoBehaviour
{
    private InputAction _interactAction;

    private void Awake()
    {
        _interactAction = FindObjectOfType<PlayerInput>().actions["Player/Interact_Hold"];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnEnterArea();
            _interactAction.started += OnInteractStarted;
            _interactAction.performed += OnInteractPerformed;
            _interactAction.canceled += OnInteractCanceled;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnExitArea();
            _interactAction.started -= OnInteractStarted;
            _interactAction.performed -= OnInteractPerformed;
            _interactAction.canceled -= OnInteractCanceled;
        }
    }

    protected abstract void OnEnterArea();
    protected abstract void OnExitArea();
    protected abstract void OnInteractStarted(InputAction.CallbackContext obj);
    protected abstract void OnInteractPerformed(InputAction.CallbackContext obj);
    protected abstract void OnInteractCanceled(InputAction.CallbackContext obj);
}
