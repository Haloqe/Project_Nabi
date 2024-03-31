using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.SocialPlatforms.Impl;

public class PlayerController : Singleton<PlayerController>
{
    // Reference to other player components
    private Animator _animator;
    private PlayerInput _playerInput;
    public PlayerMovement playerMovement;
    public PlayerDamageReceiver playerDamageReceiver;
    public PlayerDamageDealer playerDamageDealer;
    public PlayerInventory playerInventory;
    //(Variables for FlowerBomb)
    private int[] _flowerNumbers = new int[5];
    private int _currentSelectedFlower = 1;
    [SerializeField] TextMeshProUGUI[] flowerText = new TextMeshProUGUI[5];

    // Centrally controlled variables
    public float HealEfficiency = 1.0f;
    public float Strength = 0.0f;
    public float Armour = 0.0f;
    public float ArmourPenetration = 0.0f;

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
                playerDamageDealer.AttackBases[(int)ELegacyType.Ranged].UpdateLegacyPreservation((ELegacyPreservation)(count + 1));
                break;
        }
        count++;
    }
    
    //Selection of Bombs
    void OnBomb0_Select(InputValue value)
    {
        SelectFlowers(0);
    }
    void OnBomb1_Select(InputValue value)
    {
        SelectFlowers(1);
    }

    void OnBomb2_Select(InputValue value)
    {
        SelectFlowers(2);
    }
    void OnBomb3_Select(InputValue value)
    {
        SelectFlowers(3);
    }
    void OnBomb4_Select(InputValue value)
    {
        SelectFlowers(4);
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

    // Store the number of flower bombs the player owns
    public void AddToFlower(int flowerIndex)
    {
        _flowerNumbers[flowerIndex]++;
        flowerText[flowerIndex].text = _flowerNumbers[flowerIndex].ToString();
    }

    // Decrease the number of flower bombs the player owns
    public void DecreaseToFlower(int flowerIndex)
    {
        _flowerNumbers[flowerIndex]--;
        flowerText[flowerIndex].text = _flowerNumbers[flowerIndex].ToString();
    }

    // Return the number of flower bombs currently stored
    public int GetNumberOfFlowers(int flowerIndex)
    {
        return _flowerNumbers[flowerIndex];
    }

    //Select a certain flower ready to be used
    public void SelectFlowers(int flowerIndex)
    {
        //if the number of flowers in stock is not 0, select.
        if (_flowerNumbers[flowerIndex] != 0)
        {
            _currentSelectedFlower = flowerIndex;
            Debug.Log("Flower Number" + flowerIndex + "is selected!");
            FindObjectOfType<AttackBase_Area>().SwitchVFX();
        }
    }

    //Return the current selected flower
    public int GetCurrentSelectedFlower()
    {
        return _currentSelectedFlower;
    }
    public void UpgradeStats(SLegacyStatUpgradeData[] upgradeData, ELegacyPreservation preservation)
    {
        foreach (var data in upgradeData)
        {
            switch (data.Stat)
            {
                case EStat.ArmourPenetration:
                    ArmourPenetration = Utility.GetChangedValue(ArmourPenetration, data.IncreaseAmounts[(int)preservation], data.IncreaseMethod);
                    break;
                
                // TODO
                default:
                    break;
            }
        }
    }
}
