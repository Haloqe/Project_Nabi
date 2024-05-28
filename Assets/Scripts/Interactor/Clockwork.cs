using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Clockwork : Interactor
{
    public EWarrior warrior;
    public bool isPristineClockwork;
    [SerializeField] private GameObject pristineEffect;
    
    private void Update()
    {
        transform.Rotate(0, 0, -50.0f * Time.deltaTime);
    }

    private void Start()
    {
        pristineEffect.SetActive(isPristineClockwork);
    }

    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        if (IsInteracting) return;
        IsInteracting = UIManager.Instance.OpenWarriorUI(this, isPristineClockwork);
    }
}