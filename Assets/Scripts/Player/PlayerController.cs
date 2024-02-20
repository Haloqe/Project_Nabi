using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : Singleton<PlayerController>
{
    // Reference to other player components
    private Animator _animator;
    private PlayerInput _playerInput;
    public PlayerMovement playerMovement;
    public PlayerDamageReceiver playerDamageReceiver;
    public PlayerDamageDealer playerDamageDealer;
    public PlayerInventory playerInventory;
    
    // Centrally controlled variables
    public float HealEfficiency = 1.0f;
    
    protected override void Awake()
    {
        base.Awake();
        if (_toBeDestroyed) return;
        
        _playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        playerDamageReceiver = GetComponent<PlayerDamageReceiver>();
        playerDamageDealer = GetComponent<PlayerDamageDealer>();
        playerInventory = GetComponent<PlayerInventory>();
        _animator = GetComponent<Animator>();
        
        // Input Binding for Attacks
        _playerInput.actions["Attack_Melee"].performed += _ => playerDamageDealer.OnAttack(0);
        _playerInput.actions["Attack_Range"].performed += _ => playerDamageDealer.OnAttack(1);
        _playerInput.actions["Attack_Dash"].performed += _ => playerDamageDealer.OnAttack(2);
        _playerInput.actions["Attack_Area"].performed += _ => playerDamageDealer.OnAttack(3);
        
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
        playerMovement.SetMoveDirection(value.Get<Vector2>());
    }

    void OnJump(InputValue value)
    {
        playerMovement.SetJump(value.isPressed);
    }

    private int count = -1;
    void OnTestAction(InputValue value)
    {
        playerInventory.ChangeGoldByAmount(100);
        switch (count)
        {
         case -1:
             PlayerAttackManager.Instance.CollectLegacy(9, ELegacyPreservation.Weathered);
             break;
         case 0:
         case 1:
         case 2:
             playerDamageDealer.AttackBases[(int)ELegacyType.Ranged].UpdateLegacyPreservation((ELegacyPreservation)(count+1));
             break;
        }
        count++;
    }
    
    private void OnOpenMap(InputValue value)
    {
        UIManager.Instance.ToggleMap();
    }
    
    
    // Heal is exclusively used for increase of health from food items
    public void Heal(float amount)
    {
        playerDamageReceiver.ChangeHealthByAmount(amount * HealEfficiency, false);
    }
}
