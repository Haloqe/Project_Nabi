using UnityEngine.InputSystem;

public class NPC_Queen : Interactor
{
    private UIManager _uiManager;

    private void Awake()
    {
        _uiManager = UIManager.Instance;
    }
    
    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        _uiManager.OpenMetaUpgradeUI();
    }
}
