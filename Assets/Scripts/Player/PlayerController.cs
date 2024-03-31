using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private int _slayedEnemiesCount = 0;
    
    public float Strength => _baseStrength * _strengthMultiplier;
    public float Armour => _baseArmour * _armourMultiplier;
    public float ArmourPenetration => _baseArmourPenetration * _armourPenetrationMultiplier;
    public float EvasionRate => _baseEvasionRate * _evasionRateMultiplier;
    public float CriticalRate => _baseCritcalRate * _criticalRateMultiplier;

    private float _baseStrength = 0.0f;
    private float _baseArmour = 0.0f;
    private float _baseArmourPenetration = 0.0f;
    private float _baseEvasionRate = 0.0f;
    private float _baseCritcalRate = 0.0f;
    
    private float _strengthMultiplier = 1.0f;
    private float _armourMultiplier = 1.0f;
    private float _armourPenetrationMultiplier = 1.0f;
    private float _evasionRateMultiplier = 1.0f;  
    private float _criticalRateMultiplier = 1.0f;  
    
    // Legacy related (Buffs)
    public ELegacyPreservation EnemyGoldDropBuffPreserv { private set; get; }
    public ELegacyPreservation DruggedEffectBuffPreserv { private set; get; }
    public ELegacyPreservation HypHallucinationPreserv { private set; get; }
    

    // Upgrades
    private Dictionary<EStat, List<(int legacyID, SLegacyStatUpgradeData data)>> _appliedStatUpgrades;

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
        _appliedStatUpgrades = new Dictionary<EStat, List<(int legacyID, SLegacyStatUpgradeData data)>>();

        // Input Binding for Attacks
        _playerInput.actions["Attack_Melee"].performed += _ => playerDamageDealer.OnAttack(0);
        _playerInput.actions["Attack_Range"].performed += _ => playerDamageDealer.OnAttack(1);
        _playerInput.actions["Attack_Dash"].performed += _ => playerDamageDealer.OnAttack(2);
        _playerInput.actions["Attack_Area"].performed += _ => playerDamageDealer.OnAttack(3);

        // Events binding
        GameEvents.restarted += OnRestarted;
        PlayerEvents.ValueChanged += OnValueChanged;
        OnRestarted();
    }

    private void OnRestarted()
    {
        _animator.Rebind();
        _animator.Update(0f);
        _appliedStatUpgrades.Clear();
        HealEfficiency = 1.0f;
        _baseStrength = 0.0f;
        _baseArmour = 0.0f;
        _baseArmourPenetration = 0.0f;
        _baseEvasionRate = 0.0f;
        _strengthMultiplier = 1.0f;
        _armourMultiplier = 1.0f;
        _armourPenetrationMultiplier = 1.0f;
        _evasionRateMultiplier = 1.0f;
        EnemyGoldDropBuffPreserv = ELegacyPreservation.MAX;
        DruggedEffectBuffPreserv = ELegacyPreservation.MAX;
        HypHallucinationPreserv = ELegacyPreservation.MAX; 
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
        
    }

    private void OnValueChanged(ECondition condition, float changeAmount)
    {
        switch (condition)
        {
            case ECondition.SlayedEnemiesCount:
                _slayedEnemiesCount += (int)changeAmount;
                break;
        }
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

    // TODO 구현 방식 고민좀해봐야겠음
    public void UpgradeStat(int legacyID, SLegacyStatUpgradeData upgradeData, ELegacyPreservation preservation)
    {
        // Add to applied stat upgrades
        if (!_appliedStatUpgrades.TryGetValue(upgradeData.Stat, out List<(int legacyID, SLegacyStatUpgradeData data)> list))
        {
            list.Add((legacyID, upgradeData));
        }
        else
        {
            _appliedStatUpgrades.Add(upgradeData.Stat, new List<(int legacyID, SLegacyStatUpgradeData data)>{(legacyID, upgradeData)});
        }

        if (upgradeData.HasUpdateCondition)
        {
            
        }

        if (upgradeData.HasApplyCondition)
        {
            var applyCond = upgradeData.UpgradeApplyCondition;
            if (applyCond.condition == ECondition.PlayerHealth)
            {
                // HP는 현재 ratio로만 다루고 있음.
                PlayerEvents.HPChanged += (changeAmount, hpRatio) =>
                {
                    if (Utility.Compare(hpRatio, applyCond.comparator, applyCond.targetValue))
                    {
                        
                    }
                };
            }
        }
        
        // Update value
        var fieldInfo = GetType().GetField(upgradeData.Stat.ToString());
        var prevValue = (float)fieldInfo.GetValue(this);
        
        // Constant? -> Instant apply
        if (upgradeData.IncreaseMethod == EIncreaseMethod.Constant)
        {
            var newValue = Utility.GetChangedValue(prevValue, upgradeData.IncreaseAmounts[(int)preservation], upgradeData.IncreaseMethod);
            fieldInfo.SetValue(this, newValue);
        }
        // Multiplier? -> Update multiplier
        else
        {
            var multiplierFieldInfo = GetType().GetField(upgradeData.Stat + "Multiplier");
            multiplierFieldInfo.SetValue(this, (float)multiplierFieldInfo.GetValue(this) + upgradeData.IncreaseAmounts[(int)preservation]);
        }
    }
    
    // Legacy - Enum 이름에 해당하는 boolean 값을 찾아서 activate
    public void ActivateBuffByName(EBuffType legacyBuff, ELegacyPreservation preservation)
    {
        // Activate buff
        var fieldInfo = GetType().GetField($"<{legacyBuff}Preserv>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Public);
        fieldInfo.SetValue(this, preservation);
    }
}
