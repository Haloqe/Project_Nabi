using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerController : Singleton<PlayerController>
{
    // Reference to other player components
    private Animator _animator;
    private PlayerInput _playerInput;
    public PlayerMovement playerMovement;
    public PlayerDamageReceiver playerDamageReceiver;
    public PlayerDamageDealer playerDamageDealer;
    public PlayerInventory playerInventory;
    public GameObject nightShadeCollider;
    
    // Centrally controlled variables
    public float DefaultGravityScale { get; private set; }
    private int _slayedEnemiesCount = 0;
    public float HpCriticalThreshold { get; private set; }
    [NamedArray(typeof(EStatusEffect))] public GameObject[] statusEffects;
    [NamedArray(typeof(EBuffs))] public GameObject[] buffEffects;
    
    // Stats
    public float Strength => _baseStrength * _strengthMultiplier;
    public float Armour => _baseArmour * _armourMultiplier;
    public float ArmourPenetration => _baseArmourPenetration * _armourPenetrationMultiplier;
    public float HealEfficiency => _baseHealEfficiency * _healEfficiencyMultiplier;
    public float EvasionRate => _baseEvasionRate + _evasionRateAddition + evasionRateAdditionAtMax;
    public float CriticalRate => _baseCritcalRate + _criticalRateAddition;

    private float _baseStrength = 3.0f;
    private float _baseArmour = 0.0f;
    private float _baseArmourPenetration = 0.0f;
    private float _baseEvasionRate = 0.0f;
    private float _baseCritcalRate = 0.0f;
    private float _baseHealEfficiency = 1.0f;
    
    // 확률은 +, 일반 숫자값은 *
    private float _strengthMultiplier = 1.0f;
    private float _armourMultiplier = 1.0f;
    private float _armourPenetrationMultiplier = 1.0f;
    private float _healEfficiencyMultiplier = 1.0f;
    private float _evasionRateAddition = 0.0f;  
    private float _criticalRateAddition = 0.0f;  
    public float evasionRateAdditionAtMax = 0.0f;
    
    // Legacy related (Buffs)
    public ELegacyPreservation EuphoriaEnemyGoldDropBuffPreserv { private set; get; }
    public ELegacyPreservation EuphoriaEcstasyUpgradePreserv { private set; get; }
    public ELegacyPreservation SommerHypHallucinationPreserv { private set; get; }
    public ELegacyPreservation TurbelaMaxButterflyPreserv { private set; get; }
    public ELegacyPreservation TurbelaDoubleSpawnPreserv { private set; get; }
    public ELegacyPreservation TurbelaButterflyCritPreserv { private set; get; }
    public ELegacyPreservation NightShadeFastChasePreserv { private set; get; }
    public ELegacyPreservation NightShadeShadeBonusPreserv { private set; get; }

    // Upgrades
    private Dictionary<EStat, List<(int legacyID, SLegacyStatUpgradeData data)>> _appliedStatUpgrades;
    private List<EnemyBase>[] _ecstasyAffected;
    
    // NightShade
    private List<EnemyBase> _shadowHosts;
    private readonly int _shadowHostLimit = 3;
    private readonly float _shadowHostAutoUpdateInterval = 2f;
    private readonly float _shadowHostAutoUpdateAmount = 1.5f;
    public float[] nightShadeFastChaseStats;
    public float[] nightShadeShadeBonusStats;
    
    protected override void Awake()
    {
        base.Awake();
        if (IsToBeDestroyed) return;

        // Initialise values
        HpCriticalThreshold = 0.33f;
        nightShadeShadeBonusStats = new float[]{0,0,0,0,0};
        DefaultGravityScale = 3.0f;
        
        // Get player components
        _playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        playerDamageReceiver = GetComponent<PlayerDamageReceiver>();
        playerDamageDealer = GetComponent<PlayerDamageDealer>();
        playerInventory = GetComponent<PlayerInventory>();
        _animator = GetComponent<Animator>();
        _appliedStatUpgrades = new Dictionary<EStat, List<(int legacyID, SLegacyStatUpgradeData data)>>();
        _shadowHosts = new List<EnemyBase>();
        nightShadeCollider = GetComponentInChildren<NightShadeCollider>(includeInactive: true).gameObject;
        
        // Events binding
        GameEvents.Restarted += OnRestarted;
        PlayerEvents.ValueChanged += OnValueChanged;
        InGameEvents.EnemySlayed += OnEnemySlayed;
        _playerInput.actions["Jump"].started += OnStartJump;
        _playerInput.actions["Jump"].canceled += OnReleaseJump;
        
        OnRestarted();
    }

    private void OnDestroy()
    {
        GameEvents.Restarted -= OnRestarted;
        PlayerEvents.ValueChanged -= OnValueChanged;
        InGameEvents.EnemySlayed -= OnEnemySlayed;
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
        _evasionRateAddition = 0.0f;
        _healEfficiencyMultiplier = 1.0f;
        evasionRateAdditionAtMax = 0.0f;
        
        // Initialise legacy preservations
        EuphoriaEnemyGoldDropBuffPreserv = ELegacyPreservation.MAX;
        EuphoriaEcstasyUpgradePreserv = ELegacyPreservation.MAX;
        SommerHypHallucinationPreserv = ELegacyPreservation.MAX; 
        TurbelaMaxButterflyPreserv = ELegacyPreservation.MAX; 
        TurbelaDoubleSpawnPreserv = ELegacyPreservation.MAX; 
        TurbelaButterflyCritPreserv = ELegacyPreservation.MAX;
        NightShadeFastChasePreserv = ELegacyPreservation.MAX;
        NightShadeShadeBonusPreserv = ELegacyPreservation.MAX;

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
        nightShadeCollider.SetActive(false);
    }

    void OnMove(InputValue value)
    {
        playerMovement.SetMoveDirection(value.Get<Vector2>());
    }

    // void OnJump(InputValue value)
    // {
    //     playerMovement.SetJump(value.isPressed);
    // }

    void OnStartJump(InputAction.CallbackContext obj)
    {
        playerMovement.StartJump();
    }

    void OnReleaseJump(InputAction.CallbackContext obj)
    {
        playerMovement.StopJump();
    }

    int count = 0;
    void OnTestAction(InputValue value)
    {
        playerInventory.AddFlower(1);
        playerInventory.AddFlower(1);
        playerInventory.AddFlower(1);
        playerInventory.AddFlower(1);
        playerInventory.AddFlower(1);
        // playerInventory.AddFlower(2);
        // playerInventory.AddFlower(3);
        // playerInventory.AddFlower(4);
        //playerDamageReceiver.ChangeHealthByAmount(-1000);
        //playerInventory.ChangeGoldByAmount(600);
        // if (count == 0)
        // {
        //     playerInventory.AddFlower((int)EFlowerType.IncendiaryFlower);
        // }
        // else
        // {
        //     playerDamageReceiver.ChangeHealthByAmount(-1000);
        // }
        // count++;
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
    
    private void OnOpenMap(InputValue value)
    {
        UIManager.Instance.OpenMap();
    }
    
    private void OnOpenBook(InputValue value)
    {
        UIManager.Instance.OpenBook();
    }

    // Heal is exclusively used for increase of health from food items
    public void Heal(float amount)
    {
        buffEffects[(int)EBuffs.Heal].SetActive(true);
        playerDamageReceiver.ChangeHealthByAmount(amount * HealEfficiency, false);
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
        
        string enumString = upgradeData.Stat.ToString();
        string decapitalizedEnumString = char.ToLower(enumString[0]) + enumString.Substring(1);
        
        // Update value - multiplier
        if (upgradeData.isMultiplier)
        {
            var multiplierFieldInfo = GetType().GetField($"_{decapitalizedEnumString}Multiplier", BindingFlags.NonPublic | BindingFlags.Instance);
            multiplierFieldInfo.SetValue(this, (float)multiplierFieldInfo.GetValue(this) + upgradeData.IncreaseAmounts[(int)preservation]);
        }
        // Update value - addition
        else
        {
            var additionFieldInfo = GetType().GetField($"_{decapitalizedEnumString}Addition", BindingFlags.NonPublic | BindingFlags.Instance);
            additionFieldInfo.SetValue(this, (float)additionFieldInfo.GetValue(this) + upgradeData.IncreaseAmounts[(int)preservation]);
        }
        
        // Display text
        string[] upText = {" Up!", " 업!"};
        var text = Define.StatNames[(int)Define.Localisation, (int)upgradeData.Stat] + upText[(int)Define.Localisation];
        UIManager.Instance.DisplayTextPopUp(text, transform.position + new Vector3(0, 2.3f, 0), transform);
        
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
    
    public void SetVFXActive(EStatusEffect effect, bool setActive) => SetVFXActive((int)effect, setActive);

    public void SetVFXActive(int effectIdx, bool setActive)
    {
        if (statusEffects[effectIdx] == null) return;
        statusEffects[effectIdx].SetActive(setActive);
    }

    public void AddCriticalRate(float value)
    {
        _criticalRateAddition = Mathf.Max(0, _criticalRateAddition + value);
    }
}
