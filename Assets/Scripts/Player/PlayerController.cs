using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    static public PlayerController Instance { get; set; }
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private PlayerCombat _playerCombat;
    static int _testNumCalls = 0;

    // TEMP TEST

    private void Awake()
    {
        Instance = this;
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerCombat>();
    }

    void OnMove(InputValue value)
    {
        _playerMovement.SetMoveDirection(value.Get<Vector2>());
    }

    void OnJump(InputValue value)
    {
        _playerMovement.SetJump(value.isPressed);
    }        

    void OnTestAction(InputValue value)
    {
        if (_testNumCalls == 0)
        {
            PlayerAbilityManager.Instance.BindActiveAbility(1, 7);
            _testNumCalls++;
        }        
    }
}
