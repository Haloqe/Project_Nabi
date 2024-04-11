using UnityEngine;
using UnityEngine.InputSystem;

public class Clockwork : Interactor
{
    public EWarrior warrior;
    
    private void Update()
    {
        transform.Rotate(0, 0, -50.0f * Time.deltaTime);
    }
    
    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        if (_isInteracting) return;
        _isInteracting = UIManager.Instance.OpenWarriorUI(this);
    }
}