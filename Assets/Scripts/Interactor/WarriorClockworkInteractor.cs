using UnityEngine;
using UnityEngine.InputSystem;

public class WarriorClockworkInteractor : Interactor
{
    public EWarrior warrior;
    
    private void Update()
    {
        transform.Rotate(0, 50.0f * Time.deltaTime, 0);
    }
    
    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        UIManager.Instance.OpenWarriorUI(this);
    }
}