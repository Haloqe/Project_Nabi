using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    static public PlayerController Instance { get; private set; }
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private PlayerCombat _playerCombat;
    static int _testNumCalls = 0;
    private bool _isNYScene;

    private void Awake()
    {
        Instance = this;
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerCombat>();
    }

    private void Start()
    {
        // TEMP for debug
        _isNYScene = SceneManager.GetActiveScene().name == "Scene_NY";
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
        if (_isNYScene) OnTestAction_NY(value);
        else OnTestAction_SOOA(value);
    }

    private void OnTestAction_NY(InputValue value)
    {

    }

    private void OnTestAction_SOOA(InputValue value)
    {
        if (_testNumCalls == 0)
        {
            PlayerAbilityManager.Instance.BindActiveAbility(0, 1); // Mansa Musa
            PlayerAbilityManager.Instance.BindActiveAbility(1, 7); // Perfect Purification
            _testNumCalls++;
        }
    }
}
