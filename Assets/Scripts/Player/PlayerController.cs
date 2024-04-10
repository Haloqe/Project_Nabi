using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.Mathematics;
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

    //(Variables for FlowerBomb)
    private int[] _flowerNumbers = new int[5];
    private int _currentSelectedFlower = 1;
    [SerializeField] TextMeshProUGUI[] flowerText = new TextMeshProUGUI[5];

    // Reference to UI objects
    public GameObject evadeTextUI;
    public GameObject generalTextUI;
    
    // Centrally controlled variables
    private int _slayedEnemiesCount = 0;
    
    // Stats
    public float Strength => _baseStrength * _strengthMultiplier;
    public float Armour => _baseArmour * _armourMultiplier;
    public float ArmourPenetration => _baseArmourPenetration * _armourPenetrationMultiplier;
    public float EvasionRate => _baseEvasionRate * _evasionRateMultiplier + evasionRateAdditionAtMax;
    public float CriticalRate => _baseCritcalRate * _criticalRateMultiplier;
    public float HealEfficiency => _baseHealEfficiency * _healEfficiencyMultiplier;

    private float _baseStrength = 1.0f;
    private float _baseArmour = 0.0f;
    private float _baseArmourPenetration = 0.0f;
    private float _baseEvasionRate = 0.0f; // TEMP 
    private float _baseCritcalRate = 0.0f;
    private float _baseHealEfficiency = 1.0f;
    
    private float _strengthMultiplier = 1.0f;
    private float _armourMultiplier = 1.0f;
    private float _armourPenetrationMultiplier = 1.0f;
    private float _evasionRateMultiplier = 1.0f;  
    private float _criticalRateMultiplier = 1.0f;  
    private float _healEfficiencyMultiplier = 1.0f;
    public float evasionRateAdditionAtMax = 0.0f;
    
    // Legacy related (Buffs)
    public ELegacyPreservation EuphoriaEnemyGoldDropBuffPreserv { private set; get; }
    public ELegacyPreservation EuphoriaEcstasyUpgradePreserv { private set; get; }
    public ELegacyPreservation SommerHypHallucinationPreserv { private set; get; }
    public ELegacyPreservation TurbelaMaxButterflyPreserv { private set; get; }
    public ELegacyPreservation TurbelaDoubleSpawnPreserv { private set; get; }
    public ELegacyPreservation TurbelaButterflyCritPreserv { private set; get; }

    // Upgrades
    private Dictionary<EStat, List<(int legacyID, SLegacyStatUpgradeData data)>> _appliedStatUpgrades;
    private List<EnemyBase>[] _ecstasyAffected;
    
    // NightShade
    private List<EnemyBase> _shadowHosts;
    private readonly int _shadowHostLimit = 3;
    private readonly float _shadowHostAutoUpdateInterval = 2f;
    private readonly float _shadowHostAutoUpdateAmount = 1.5f;
    

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
        _shadowHosts = new List<EnemyBase>();
        
        // Input Binding for Attacks
        _playerInput.actions["Attack_Melee"].performed += _ => playerDamageDealer.OnAttack(0);
        _playerInput.actions["Attack_Range"].performed += _ => playerDamageDealer.OnAttack(1);
        _playerInput.actions["Attack_Dash"].performed += _ => playerDamageDealer.OnAttack(2);
        _playerInput.actions["Attack_Area"].performed += _ => playerDamageDealer.OnAttack(3);

        // Events binding
        GameEvents.restarted += OnRestarted;
        PlayerEvents.ValueChanged += OnValueChanged;
        InGameEvents.EnemySlayed += OnEnemySlayed;
        OnRestarted();
    }

    private void Start()
    {
        _ecstasyAffected = new List<EnemyBase>[EnemyManager.Instance.NumEnemyTypes];
        for (int i = 0; i < EnemyManager.Instance.NumEnemyTypes; i++)
            _ecstasyAffected[i] = new List<EnemyBase>();
    }

    private void OnRestarted()
    {
        StopAllCoroutines();
        
        // Initialise animator
        _animator.Rebind();
        _animator.Update(0f);
        
        // Initialise player stats
        _appliedStatUpgrades.Clear();
        _strengthMultiplier = 1.0f;
        _armourMultiplier = 1.0f;
        _armourPenetrationMultiplier = 1.0f;
        _evasionRateMultiplier = 1.0f;
        _healEfficiencyMultiplier = 1.0f;
        evasionRateAdditionAtMax = 0.0f;
        
        // Initialise legacy preservations
        EuphoriaEnemyGoldDropBuffPreserv = ELegacyPreservation.MAX;
        EuphoriaEcstasyUpgradePreserv = ELegacyPreservation.MAX;
        SommerHypHallucinationPreserv = ELegacyPreservation.MAX; 
        TurbelaMaxButterflyPreserv = ELegacyPreservation.MAX; 
        TurbelaDoubleSpawnPreserv = ELegacyPreservation.MAX; 
        TurbelaButterflyCritPreserv = ELegacyPreservation.MAX;

        // Reset ecstasy effect
        if (_ecstasyAffected != null)
        {
            foreach (var enemyList in _ecstasyAffected)
            {
                enemyList.Clear();
            }
        }
        
        // Initialise NightShade data
        _shadowHosts.Clear();
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
    
    private void OnEnemySlayed(EnemyBase slayedEnemy)
    {
        PlayerEvents.ValueChanged.Invoke(ECondition.SlayedEnemiesCount, +1);
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

    public void UpgradeStat(int legacyID, SLegacyStatUpgradeData upgradeData, ELegacyPreservation preservation)
    {
        if (upgradeData.HasUpdateCondition || upgradeData.HasApplyCondition)
        {
            // TODO 피의 갑주
            return;
        }
        
        // Add to applied stat upgrades
        // if (!_appliedStatUpgrades.TryGetValue(upgradeData.Stat, out List<(int legacyID, SLegacyStatUpgradeData data)> list))
        // {
        //     list.Add((legacyID, upgradeData));
        // }
        // else
        // {
        //     _appliedStatUpgrades.Add(upgradeData.Stat, new List<(int legacyID, SLegacyStatUpgradeData data)>{(legacyID, upgradeData)});
        // }
        
        // Update value - multiplier
        // 피의 갑주를 제외한 모든 stat update는 현재 multiplier 형식이기에 통일하였음
        string enumString = upgradeData.Stat.ToString();
        string decapitalizedEnumString = char.ToLower(enumString[0]) + enumString.Substring(1);
        var multiplierFieldInfo = GetType().GetField($"_{decapitalizedEnumString}Multiplier", BindingFlags.NonPublic | BindingFlags.Instance);
        multiplierFieldInfo.SetValue(this, (float)multiplierFieldInfo.GetValue(this) + upgradeData.IncreaseAmounts[(int)preservation]);
        
        // Display text
        string[] upText = {" Up!", " 업!"};
        var text = Define.StatNames[(int)Define.Localisation, (int)upgradeData.Stat] + upText[(int)Define.Localisation];
        DisplayTextPopUp(text, true);
        
        // if (upgradeData.HasApplyCondition)
        // {
        //     var applyCond = upgradeData.UpgradeApplyCondition;
        //     if (applyCond.condition == ECondition.PlayerHealth)
        //     {
        //         // HP는 현재 ratio로만 다루고 있음.
        //         PlayerEvents.HPChanged += (changeAmount, hpRatio) =>
        //         {
        //             if (Utility.Compare(hpRatio, applyCond.comparator, applyCond.targetValue))
        //             {
        //                 
        //             }
        //         };
        //     }
        // }
    }
    
    // Legacy - Enum 이름에 해당하는 boolean 값을 찾아서 activate
    public void ActivateBuffByName(EBuffType legacyBuff, ELegacyPreservation preservation)
    {
        // Activate buff
        var fieldInfo = GetType().GetField($"<{legacyBuff}Preserv>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Public);
        fieldInfo.SetValue(this, preservation);
    }

    public void ApplyEcstasyBuff(EnemyBase appliedEnemy)
    {
        // 유포리아식 위계질서가 없으면 처리 X
        int preserv = (int)playerDamageDealer.BindingSkillPreservations[(int)EWarrior.Euphoria];
        if (preserv is (int)ELegacyPreservation.MAX) return;

        // 이미 버프를 받았으면 처리 X
        var id = appliedEnemy.EnemyData.ID;
        if (_ecstasyAffected[id].Contains(appliedEnemy)) return;
        _ecstasyAffected[id].Add(appliedEnemy);
        if (_ecstasyAffected[id].Count > 1) return;
        
        // Apply buff
        var buffValue = EuphoriaEcstasyUpgradePreserv == ELegacyPreservation.MAX ? 
            Define.EcstasyBuffStats[id][preserv] : Define.EcstasyUpgradedBuffStats[id][preserv];
        switch (appliedEnemy.EnemyData.ID)
        {
            case 0: // VoidMantis: 방어력 버프
                _armourMultiplier += buffValue;
                break;
            
            case 1: // Insectivore: 원거리 공격 버프
                playerDamageDealer.attackDamageMultipliers[(int)EPlayerAttackType.Ranged] += buffValue;
                break;
        }
    }

    public void RemoveEcstasyBuff(EnemyBase appliedEnemy)
    {
        // 유포리아식 위계질서가 없으면 처리 X
        int preserv = (int)playerDamageDealer.BindingSkillPreservations[(int)EWarrior.Euphoria];
        if (preserv is (int)ELegacyPreservation.MAX) return;

        // Return if this is not the last affected enemy
        var id = appliedEnemy.EnemyData.ID;
        if (!_ecstasyAffected[id].Contains(appliedEnemy)) return;
        _ecstasyAffected[id].Remove(appliedEnemy);
        if (_ecstasyAffected[id].Count > 0) return;
        
        // Remove the applied effect
        switch (appliedEnemy.EnemyData.ID)
        {
            case 0: // VoidMantis
                _armourMultiplier = 1.0f;
                break;
            
            case 1: // Insectivore
                playerDamageDealer.attackDamageMultipliers[(int)EPlayerAttackType.Ranged] = 1.0f;
                break;
        }
    }

    public bool AddShadowHost(EnemyBase enemy)
    {
        // Max number of shadow hosts reached?
        if (_shadowHosts.Count >= _shadowHostLimit) return false;
        
        // Already a shadow host?
        if (_shadowHosts.Contains(enemy)) return false;
        
        // Add enemy to the list
        _shadowHosts.Add(enemy);
        StartCoroutine(nameof(ShadowHostAutoUpdateCoroutine));
        return true;
    }

    public void RemoveShadowHost(EnemyBase enemy)
    {
        _shadowHosts.Remove(enemy);
        StopCoroutine(nameof(ShadowHostAutoUpdateCoroutine));   
    }

    // Update dark gauge every few seconds from shadow hosts
    private IEnumerator ShadowHostAutoUpdateCoroutine()
    {
        var wait = new WaitForSeconds(_shadowHostAutoUpdateInterval);
        while (true)
        {
            yield return wait;
            playerDamageDealer.UpdateNightShadeDarkGauge(_shadowHostAutoUpdateAmount);
        }
    }
    
    // TODO move to ui manager
    public void DisplayTextPopUp(string text, bool isAttached)
    {
        var ui = Instantiate(generalTextUI, transform.position + new Vector3(0, 2.3f, 0), quaternion.identity).GetComponent<TextUI>();
        ui.Init(isAttached ? transform : null, text);
    }
}
