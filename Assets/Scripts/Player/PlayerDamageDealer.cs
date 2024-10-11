using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerDamageDealer : MonoBehaviour, IDamageDealer
{
    private Animator _animator;
    private PlayerMovement _playerMovement;
    private PlayerController _playerController;
    private UIManager _uiManager;
    
    public AttackBase[] AttackBases { get; private set; }
    public float[] attackDamageMultipliers;
    public float totalDamageMultiplier; // Used exclusively for tension effect
    private int _currAttackIdx;
    private int _bufferedAttackIdx;
    public bool IsAttackBufferAvailable { get; private set; }
    private bool _isUnderAttackDelay;

    // Dash
    private Coroutine _dashCooldownCoroutine;
    private Image _dashUIOverlay;
    private float _dashCooldown = 0.6f;

    // Turbela Butterfly
    private GameObject _butterflyPrefab;
    private List<Butterfly> _spawnedButterflies;
    private int ButterflySpawnLimit => (int)Define.TurbelaMaxButterflyStats[(int)_playerController.TurbelaMaxButterflyPreserv];

    // NightShade
    private float _darkGauge;
    public int DarkGaugeMax { get; } = 30;
    private bool IsDarkGaugeFull => _darkGauge == DarkGaugeMax;

    // Legacy
    public ELegacyPreservation[] BindingSkillPreservations { private set; get; }
    private readonly static int AttackIndex = Animator.StringToHash("AttackIndex");
    
    // SFX
    public AudioSource AudioSource { get; private set; }
    public int CurrAttackIdx
    {
        get => _currAttackIdx;
        private set => _currAttackIdx = value;
    }

    private void Start()
    {
        // Get components
        AudioSource = GetComponent<AudioSource>();
        _butterflyPrefab = Resources.Load("Prefabs/Player/SpawnObjects/Butterfly").GameObject();
        _playerController = PlayerController.Instance;
        _uiManager = UIManager.Instance;
        _playerMovement = _playerController.playerMovement;
        _animator = GetComponent<Animator>();
        
        // Initialise values
        attackDamageMultipliers = new float[]{ 1, 1, 1, 1, 1 };
        _spawnedButterflies = new List<Butterfly>();
        BindingSkillPreservations = new ELegacyPreservation[(int)EWarrior.MAX];
        for (int i = 0; i < (int)EWarrior.MAX; i++) BindingSkillPreservations[i] = ELegacyPreservation.MAX;
        var attacks = transform.Find("Attacks");
        AttackBases = new AttackBase[]
        {
            attacks.GetComponent<AttackBase_Melee>(), attacks.GetComponent<AttackBase_Ranged>(),
            attacks.GetComponent<AttackBase_Dash>(), attacks.GetComponent<AttackBase_Area>()
        };
        IsAttackBufferAvailable = true;
        _currAttackIdx = -1;
        _bufferedAttackIdx = -1;
        
        // Bind events
        GameEvents.GameLoadEnded += OnRestarted;
        PlayerEvents.Defeated += OnPlayerDefeated;
        PlayerEvents.StartResurrect += () => OnPlayerDefeated(true);
        GameEvents.CombatSceneChanged += OnCombatSceneChanged;
        
        // Input Binding for Attacks
        var playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Attack_Melee"].performed += OnMeleeAttack;
        playerInput.actions["Attack_Range"].performed += OnRangedAttack;
        playerInput.actions["Attack_Dash"].performed += OnDashAttack;
        playerInput.actions["Attack_Area"].performed += OnAreaAttack;
    }

    public void AssignDashOverlay(Image overlay)
    {
        _dashUIOverlay = overlay;
        _dashUIOverlay.fillAmount = 0;
    }

    public void FlipAttackVFX()
    {
        if (_currAttackIdx <= 0) return;
        AttackBases[_currAttackIdx].FlipVFX();    
    }
    
    private void OnDestroy()
    {
        if (GetComponent<PlayerController>().IsToBeDestroyed) return;
        GameEvents.GameLoadEnded -= OnRestarted;
        PlayerEvents.Defeated -= OnPlayerDefeated;
        PlayerEvents.StartResurrect -= () => OnPlayerDefeated(true);
        GameEvents.CombatSceneChanged -= OnCombatSceneChanged;
        
        var playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Attack_Melee"].performed -= OnMeleeAttack;
        playerInput.actions["Attack_Range"].performed -= OnRangedAttack;
        playerInput.actions["Attack_Dash"].performed -= OnDashAttack;
        playerInput.actions["Attack_Area"].performed -= OnAreaAttack;
    }

    private void OnPlayerDefeated(bool isRealDeath)
    {
        foreach (var butterfly in _spawnedButterflies) Destroy(butterfly.gameObject);
        _spawnedButterflies.Clear();
        _dashUIOverlay.fillAmount = 0.0f;
        IsAttackBufferAvailable = true;
        _currAttackIdx = -1;
        _bufferedAttackIdx = -1;
        ResetNightShadeDarkGauge();
    }
    
    private void OnRestarted()
    {
        // Reset attacks
        for (int i = 0; i < (int)EWarrior.MAX; i++) BindingSkillPreservations[i] = ELegacyPreservation.MAX;
        
        // Initialise damage multipliers
        attackDamageMultipliers = new float[]{ 1, 1, 1, 1, 1 };
        totalDamageMultiplier = 1.0f;

        OnCombatSceneChanged();
    }

    private void OnCombatSceneChanged()
    {
        if (_dashCooldownCoroutine != null) StopCoroutine(_dashCooldownCoroutine);
        _dashUIOverlay = PlayerAttackManager.Instance.GetAttackOverlay(ELegacyType.Dash);
        _dashCooldownCoroutine = null;
        
        // Empty the attack buffer
        _currAttackIdx = -1;
        _bufferedAttackIdx = -1;
        IsAttackBufferAvailable = false;
        _isUnderAttackDelay = false;
        
        // Clear spawned butterflies
        _playerController.evasionRateAdditionAtMax = 0.0f;
        _spawnedButterflies.Clear();
        
        // Reset gauge values
        //ResetNightShadeDarkGauge();
        
        // Reset UI
        _dashUIOverlay = PlayerAttackManager.Instance.GetAttackOverlay(ELegacyType.Dash);
        _dashUIOverlay.fillAmount = 0.0f;
    }
    
    private IEnumerator DashCooldownCoroutine()
    {
        float timer = _dashCooldown;

        // During the cooldown, update UI
        while (timer >= 0)
        {
            _dashUIOverlay.fillAmount = timer / _dashCooldown;
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }
        _dashUIOverlay.fillAmount = 0.0f;
        _dashCooldownCoroutine = null;
    }

    private void OnMeleeAttack(InputAction.CallbackContext context) => OnAttack((int)ELegacyType.Melee);
    private void OnRangedAttack(InputAction.CallbackContext context) => OnAttack((int)ELegacyType.Ranged);
    private void OnDashAttack(InputAction.CallbackContext context) => OnAttack((int)ELegacyType.Dash);
    private void OnAreaAttack(InputAction.CallbackContext context) => OnAttack((int)ELegacyType.Area);
    
    private bool HandleAttackBuffer(int attackIdx)
    {
        // If under another attack or under attack delay, check if can save attack buffer
        if (_currAttackIdx != -1 || _isUnderAttackDelay)
        {
            // Cannot attack immediately
            if (IsAttackBufferAvailable) _bufferedAttackIdx = attackIdx;
            return false;
        }
        // If an attack is saved in the buffer, should play that instead
        return _bufferedAttackIdx == -1;
    }

    private void OnAttack(int attackIdx)
    {
        if (!HandleAttackBuffer(attackIdx)) return;

        // Handle NightShade dash separately
        if (attackIdx == (int)ELegacyType.Dash && AttackBases[attackIdx].ActiveLegacy &&
            AttackBases[attackIdx].ActiveLegacy.warrior == EWarrior.NightShade)
        {
            if (((AttackBase_Dash)AttackBases[attackIdx]).IsCurrentlyNightShadeTeleporting())
            {
                // Can't attack during teleport
            }
            else if (((AttackBase_Dash)AttackBases[attackIdx]).IsCurrentNightShadeTpDash())
            {
                AttackBases[attackIdx].Attack();
            }
            // If this is the first dash with no active cooldown, start dash
            else if (_dashCooldownCoroutine == null)
            {
                _dashCooldownCoroutine = StartCoroutine(DashCooldownCoroutine());
                AttackBases[attackIdx].Attack();
                IsAttackBufferAvailable = false;
            }
            return;
        }

        // Handle other attacks
        if (attackIdx == (int)ELegacyType.Dash)
        {
            // Check if stunned or rooted
            if (_playerMovement.IsRooted) return; 
            
            // Check cooldown
            if (_dashCooldownCoroutine != null) return;
            
            // Dash
            _dashCooldownCoroutine = StartCoroutine(DashCooldownCoroutine());
            _playerMovement.SetDash();
        }
        // For other attacks (melee, range, area), disable player movement
        else
        {
            // For area attack, check availability
            if (attackIdx == (int)ELegacyType.Area)
            {
                bool canAreaAttack = ((AttackBase_Area)AttackBases[attackIdx]).CheckAvailability();
                if (!canAreaAttack) return;
            }
            _playerMovement.DisableMovement(false);
        }

        _isUnderAttackDelay = true;
        _currAttackIdx = attackIdx;
        AttackBases[attackIdx].Attack();
        IsAttackBufferAvailable = false;
        _playerMovement.savedLookDirection = -Mathf.Sign(transform.localScale.x);
    }

    public void OnAttackEnd(ELegacyType attackType)
    {
        // 막은거 풀기
        _currAttackIdx = -1;
        _animator.SetInteger(AttackIndex, -1);
        if (attackType == ELegacyType.Melee) AttackBases[(int)attackType].baseEffector.SetActive(false);
        StartCoroutine(AttackBases[(int)attackType].AttackPostDelayCoroutine());
        _playerMovement.isDashing = false;
        if (attackType != ELegacyType.Ranged) _playerMovement.UpdateLookDirectionOnAttackEnd();
    }

    // Called when attack delay ends
    public void OnAttackEnd_PostDelay()
    {
        _isUnderAttackDelay = false;
        _playerMovement.EnableMovement(false);
        
        // If an attack is saved in the buffer, play
        if (_bufferedAttackIdx != -1)
        {
            //Debug.Log("Buffer has next attack, playing " + (ELegacyType)_bufferedAttackIdx);
            var attackToPlay = _bufferedAttackIdx;
            _bufferedAttackIdx = -1;
            OnAttack(attackToPlay);
        }
    }

    // IDamageDealer Override
    public void DealDamage(IDamageable target, AttackInfo attackInfo)
    {
        // 임시 데미지 정보 생성
        AttackInfo infoToSend = attackInfo.Clone(cloneDamage:true, cloneStatusEffect:true);

        // 방어 관통력 처리
        infoToSend.AttackerArmourPenetration = _playerController.ArmourPenetration;

        // 크리티컬 처리
        bool isCritAttack = false;
        if (Random.value <= _playerController.CriticalRate)
        {
            infoToSend.Damage.TotalAmount *= 2;
            isCritAttack = true;
            _uiManager.DisplayCritPopUp(transform.position);
        }

        // 어둠 게이지 완충 공격: 추가 데미지 및 흡혈 여부
        var isDarkChargedAttack = false;
        if (IsDarkGaugeFull && attackInfo.CanBeDarkAttack)
        {
            isDarkChargedAttack = true;
            infoToSend.Damage.TotalAmount *= 1.5f;
            if (BindingSkillPreservations[(int)EWarrior.NightShade] != ELegacyPreservation.MAX)
            {
                infoToSend.ShouldLeech = true;
            }
            ResetNightShadeDarkGauge();
        }

        // 특수 용사 처리
        if (attackInfo.StatusEffects.Count > 0)
        {
            // 투르벨라 나비 처리
            if (attackInfo.StatusEffects[^1].Effect is EStatusEffect.Swarm or EStatusEffect.Cloud)
            {
                if (Random.value <= attackInfo.StatusEffects[^1].Chance)
                {
                    TurbelaSpawnButterfly();
                }
            }

            // 나이트셰이드 어둠 게이지 처리
            if (attackInfo.StatusEffects[^1].Effect is EStatusEffect.Evade or EStatusEffect.Camouflage)
            {
                // 완충 공격시 게이지 충전 및 숙주 상태이상에 걸리지 않음
                if (isDarkChargedAttack)
                {
                    infoToSend.StatusEffects.RemoveAt(attackInfo.StatusEffects.Count - 1);
                }
                else
                {
                    // 일반 공격
                    float addAmount = 5;
                    // 백어택
                    if (attackInfo.IncomingDirectionX != Mathf.Sign(target.GetGameObject().transform.localScale.x))
                    {
                        addAmount = 15;
                    }
                    // 크리티컬
                    else if (isCritAttack)
                    {
                        addAmount = 10;
                    }
                    Debug.Log("Shade: Base " + addAmount + " Additive: " + _playerController.nightShadeShadeBonusStats[(int)_playerController.NightShadeShadeBonusPreserv]);
                    addAmount += _playerController.nightShadeShadeBonusStats[(int)_playerController.NightShadeShadeBonusPreserv];
                    UpdateNightShadeDarkGauge(addAmount);
                }
            }
        }

        // 장력 게이지 추가 데미지 증가
        infoToSend.Damage.TotalAmount *= totalDamageMultiplier;
        
        // 데미지 전달
        target.TakeDamage(infoToSend);
    }

    public int GetStatusEffectLevel(EWarrior warrior)
    {
        return BindingSkillPreservations[(int)warrior] == ELegacyPreservation.MAX ? 0 : 1;
    }

    public void UpgradeStatusEffectLevel(EWarrior warrior, ELegacyPreservation preservation, float[] stats)
    {
        switch (warrior)
        {
            case EWarrior.Sommer:
                Define.SommerSleepArmourReduceAmounts = stats;
                break;

            case EWarrior.Euphoria:
                break;

            case EWarrior.Turbela:
                Define.TurbelaExtraDamageStats = stats;
                break;
            
            case EWarrior.NightShade:
                Define.NightShadeLeechStats = stats;
                break;
        }
        BindingSkillPreservations[(int)warrior] = preservation;
        // foreach (var attack in AttackBases) attack.UpdateLegacyStatusEffect();
    }

    public void UpdateStatusEffectPreservation(EWarrior warrior)
    {
        BindingSkillPreservations[(int)warrior]++;
        foreach (var attack in AttackBases) 
            attack.UpdateLegacyStatusEffectSpecificWarrior(warrior);
    }

    // Turbela
    public void TurbelaSpawnButterfly()
    {
        if (_spawnedButterflies.Count >= ButterflySpawnLimit) return;

        var obj = Instantiate(_butterflyPrefab).GetComponent<Butterfly>();
        obj.extraRelativeDamage = Define.TurbelaExtraDamageStats[(int)BindingSkillPreservations[(int)EWarrior.Turbela]];
        _spawnedButterflies.Add(obj);

        // 유산 - 흩어져라
        if (_spawnedButterflies.Count == ButterflySpawnLimit)
        {
            _playerController.evasionRateAdditionAtMax = 0.2f;
        }
    }

    public void TurbelaKillButterfly(Butterfly butterflyToKill)
    {
        if (_spawnedButterflies.Count == 0) return;

        // 유산 - 흩어져라
        if (_spawnedButterflies.Count == ButterflySpawnLimit)
        {
            _playerController.evasionRateAdditionAtMax = 0.0f;
        }

        // Kill a random butterfly?
        if (butterflyToKill == null)
            butterflyToKill = _spawnedButterflies[Random.Range(0, _spawnedButterflies.Count)];

        _spawnedButterflies.Remove(butterflyToKill);
        butterflyToKill.StartCoroutine(butterflyToKill.DieCoroutine());
    }

    public void TurbelaBuffButterflies(float duration, float attackSpeedMultiplier)
    {
        foreach (var butterfly in _spawnedButterflies)
        {
            butterfly.StartCoroutine(butterfly.BuffCoroutine(duration, attackSpeedMultiplier));
        }
    }

    // NightShade
    public void UpdateNightShadeDarkGauge(float change)
    {
        _darkGauge = Mathf.Clamp(_darkGauge + change, 0, DarkGaugeMax);
        _uiManager.UpdateDarkGaugeUI(_darkGauge);
    }

    private void ResetNightShadeDarkGauge() => UpdateNightShadeDarkGauge(-DarkGaugeMax);

    // Animation event
    public void ActivateMeleeCollider()
    {
        var meleeBase = (AttackBase_Melee)AttackBases[(int)ELegacyType.Melee];
        meleeBase.ToggleCollider(true);
    }
    
    // Animation event
    public void DeactivateMeleeCollider()
    {
        var meleeBase = (AttackBase_Melee)AttackBases[(int)ELegacyType.Melee];
        meleeBase.ToggleCollider(false);
    }
    
    // Animation event
    public void FireRangeBullet()
    {
        ((AttackBase_Ranged)AttackBases[(int)ELegacyType.Ranged]).Fire();
    }

    public void ReduceGravityScale()
    {
        _playerMovement.isRangedAttacking = true;
        GetComponent<Rigidbody2D>().gravityScale = _playerMovement.DownwardsGravityScale * 0.2f;
    }

    public void RevertGravityScale()
    {
        _playerMovement.isRangedAttacking = false;
        GetComponent<Rigidbody2D>().gravityScale = _playerMovement.DownwardsGravityScale;
    }
    
    // Animation event
    public void OnMeleeComboHit()
    {
        ((AttackBase_Melee)AttackBases[(int)ELegacyType.Melee]).OnComboHit();
    }
    
    // Animation event
    public void EnableSaveNextAttack()
    {
        IsAttackBufferAvailable = true;
    }

    public void Leech(float absoluteDamage)
    {
        float leechRatio = Define.NightShadeLeechStats[(int)BindingSkillPreservations[(int)EWarrior.NightShade]]; 
        _playerController.Heal(absoluteDamage * leechRatio, showVFX:false);
        Debug.Log(absoluteDamage * leechRatio);
        _playerController.SetVFXActive(EStatusEffect.Leech, true);
    }
}
