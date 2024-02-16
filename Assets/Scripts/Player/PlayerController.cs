using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>
{
    // Reference to other player components
    private Animator _animator;
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private PlayerDamageReceiver _playerDamageReceiver;
    private PlayerDamageDealer _playerDamageDealer;
    public PlayerInventory playerInventory;
    
    protected override void Awake()
    {
        base.Awake();
        if (_toBeDestroyed) return;
        
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerDamageReceiver = GetComponent<PlayerDamageReceiver>();
        _playerDamageDealer = GetComponent<PlayerDamageDealer>();
        playerInventory = GetComponent<PlayerInventory>();
        _animator = GetComponent<Animator>();
        
        // Input Binding for Attacks
        _playerInput.actions["Attack_Melee"].performed += _ => _playerDamageDealer.OnAttack(0);
        _playerInput.actions["Attack_Range"].performed += _ => _playerDamageDealer.OnAttack(1);
        _playerInput.actions["Attack_Dash"].performed += _ => _playerDamageDealer.OnAttack(2);
        _playerInput.actions["Attack_Area"].performed += _ => _playerDamageDealer.OnAttack(3);
        
        // Events binding
        GameEvents.restarted += OnRestarted;
    }
    
    private void OnRestarted()
    {
        _animator.Rebind();
        _animator.Update(0f);
    }

    void OnMove(InputValue value)
    {
        _playerMovement.SetMoveDirection(value.Get<Vector2>());
    }

    void OnJump(InputValue value)
    {
        _playerMovement.SetJump(value.isPressed);
    }

    private int count = -1;
    void OnTestAction(InputValue value)
    {
        playerInventory.ChangeGoldByAmount(100);
    }
    
    private void OnOpenMap(InputValue value)
    {
        UIManager.Instance.ToggleMap();
    }
    
    
    // Heal is exclusively used for increase of health from food items
    public void Heal(float amount)
    {
        _playerDamageReceiver.ChangeHealthByAmount(amount, false);
    }
}
