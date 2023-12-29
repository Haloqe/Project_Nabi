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
    private PlayerCombat _playerCombat;
    private PlayerAttack _playerAttack;
    static int _testNumCalls = 0;
    private bool _isNYScene;

    // TEMP
    CinemachineVirtualCamera _playerVirtualCamera;
    [SerializeField] TextMeshProUGUI _camSizeText;

    public void SetCamSize(float t)
    {
        float size = 6 * (1 - t) + 13 * t;
        _playerVirtualCamera.m_Lens.OrthographicSize = size;
        _camSizeText.text = size.ToString();
    }

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("PlayerController::Awake");
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerCombat>();
        _playerAttack = GetComponent<PlayerAttack>();
    }

    private void Start()
    {
        // TEMP for debug
        _playerVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        _isNYScene = SceneManager.GetActiveScene().name == "Scene_NY";

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
        //if (_testNumCalls == 0)
        //{
        //    PlayerAbilityManager.Instance.BindActiveAbility(0, 1); // Mansa Musa
        //    PlayerAbilityManager.Instance.BindActiveAbility(1, 7); // Perfect Purification
        //    _testNumCalls++;
        //}
        //else if (_testNumCalls == 1)
        //{
        //    PlayerAbilityManager.Instance.BindActiveAbility(0, 2); // Bear the Crown
        //    _testNumCalls++;
        //}
        //else if (_testNumCalls == 2)
        //{
        //    PlayerAbilityManager.Instance.BindActiveAbility(2, 1); // Mansa Musa
        //    _testNumCalls++;
        //}

        
    }
}
