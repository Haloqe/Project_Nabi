using UnityEngine.InputSystem;

public class Superfood : Interactor
{ 
    private readonly int _maxHealthAddition = 15;

    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        var player = PlayerController.Instance;
        
        // 최대 체력 증가
        player.playerDamageReceiver.additionalHealth += _maxHealthAddition;
        UIManager.Instance.DisplayTextPopUp("최대 체력 UP!", player.transform.position);
        
        // 잃은 체력 즉시 회복
        player.Heal(player.playerDamageReceiver.MaxHealth);
        
        Destroy(gameObject);
    }
}