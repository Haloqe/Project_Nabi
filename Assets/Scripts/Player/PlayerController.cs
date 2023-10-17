using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    static public PlayerController Instance { get; set; }
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    static int _testNumCalls = 0;

    private void Awake()
    {
        Instance = this;
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
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
            PlayerAbilityManager.Instance.BindActiveAbility(0, 1); // bind ability id [1] to key [0]
        }
        else if (_testNumCalls == 1)
        {
            PlayerAbilityManager.Instance.BindActiveAbility(0, 2); // bind ability id [2] to key [0]
        }
        else
        {
            PlayerAbilityManager.Instance.BindActiveAbility(0, 1); // bind ability id [1] to key [0]
            PlayerAbilityManager.Instance.BindActiveAbility(1, 2); // bind ability id [2] to key [1]
        }
        _testNumCalls++;            
    }
}
