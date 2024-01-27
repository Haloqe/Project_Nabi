using Cinemachine;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : Singleton<PlayerController>
{
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private PlayerDamageReceiver _playerCombat;
    private PlayerDamageDealer _playerAttack;
    
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("PlayerController::Awake");
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerDamageReceiver>();
        _playerAttack = GetComponent<PlayerDamageDealer>();
    }

    private void Start()
    {
        // Input Binding for Attacks
        _playerInput.actions["Attack_Melee"].performed += _ => _playerAttack.OnAttack(0);
        _playerInput.actions["Attack_Range"].performed += _ => _playerAttack.OnAttack(1);
        _playerInput.actions["Attack_Dash"].performed += _ => _playerAttack.OnAttack(2);
        _playerInput.actions["Attack_Area"].performed += _ => _playerAttack.OnAttack(3);
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
        switch (++count)
        {
            case 0:
                PlayerAttackManager.Instance.CollectLegacy(0); // melee
                break;
            case 1:
                PlayerAttackManager.Instance.CollectLegacy(1); // range
                break;
            case 2:
                PlayerAttackManager.Instance.CollectLegacy(2); // dash
                break;
        }

        // count++;
        // for (int attackIdx = 0; attackIdx < 3; attackIdx++)
        // {
        //     PlayerAttackManager.Instance.UpdateAttackVFX((EWarrior)count, (ELegacyType)attackIdx);
        // }
        // if (count == 3)
        // {
        //     count = -1;
        //     PlayerAttackManager.Instance.ResetAttackVFXs();
        // }
    }
}
