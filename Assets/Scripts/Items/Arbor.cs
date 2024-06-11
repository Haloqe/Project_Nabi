using UnityEngine.InputSystem;

public class Arbor : Interactor
{
    public PlayerTensionController tensionController;
    public EArborType arborType;
    
    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        tensionController.ChangeArbor(arborType);
        tensionController.PlayArborEquipAudio();
        Destroy(gameObject);
    }
}