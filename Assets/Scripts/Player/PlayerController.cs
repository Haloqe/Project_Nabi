using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>
{
    private Animator _animator;
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private PlayerDamageReceiver _playerCombat;
    private PlayerDamageDealer _playerAttack;
    
    protected override void Awake()
    {
        base.Awake();
        if (_toBeDestroyed) return;
        
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerDamageReceiver>();
        _playerAttack = GetComponent<PlayerDamageDealer>();
        _animator = GetComponent<Animator>();
        
        // Input Binding for Attacks
        _playerInput.actions["Attack_Melee"].performed += _ => _playerAttack.OnAttack(0);
        _playerInput.actions["Attack_Range"].performed += _ => _playerAttack.OnAttack(1);
        _playerInput.actions["Attack_Dash"].performed += _ => _playerAttack.OnAttack(2);
        _playerInput.actions["Attack_Area"].performed += _ => _playerAttack.OnAttack(3);
        
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
        if (count == -1)
        {
            PlayerAttackManager.Instance.UpdateAttackVFX(EWarrior.Vernon, ELegacyType.Melee);
            PlayerAttackManager.Instance.UpdateAttackVFX(EWarrior.Vernon, ELegacyType.Ranged);
            PlayerAttackManager.Instance.UpdateAttackVFX(EWarrior.Vernon, ELegacyType.Dash);
            // PlayerAttackManager.Instance.CollectLegacy(0); // melee
            // PlayerAttackManager.Instance.CollectLegacy(1); // range
            // PlayerAttackManager.Instance.CollectLegacy(2); // dash
            count++;
        }
    }
    
    private void OnOpenMap(InputValue value)
    {
        UIManager.Instance.ToggleMap();
    }
}
