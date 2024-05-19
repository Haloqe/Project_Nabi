using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.UI;

public class PlayerDamageDealer : MonoBehaviour, IDamageDealer
{
    private Animator _animator;
    private PlayerMovement _playerMovement;
    private PlayerController _playerController;
    
    public AttackBase[] AttackBases { get; private set; }
    public float[] attackDamageMultipliers;
    public float totalDamageMultiplier; // Used exclusively for tension effect
    public int CurrAttackIdx = -1;
    public int NextAttackIdx = -1;
    private bool _canSaveNextAttack;
    public bool IsUnderAttackDelay = false;

    // Dash
    private Coroutine _dashCooldownCoroutine;
    private Image _dashUIOverlay;

    // Turbela Butterfly
    private GameObject _butterflyPrefab;
    private List<Butterfly> _spawnedButterflies;
    private int ButterflySpawnLimit => (int)Define.TurbelaMaxButterflyStats[(int)_playerController.TurbelaMaxButterflyPreserv];

    // NightShade
    private float _darkGauge;
    private bool IsDarkGaugeFull => _darkGauge == 100;

    // Legacy
    public ELegacyPreservation[] BindingSkillPreservations { private set; get; }
    private readonly static int AttackIndex = Animator.StringToHash("AttackIndex");

    private void Start()
    {
        _dashUIOverlay = PlayerAttackManager.Instance.GetAttackOverlay(ELegacyType.Dash);
        _butterflyPrefab = Resources.Load("Prefabs/Player/SpawnObjects/Butterfly").GameObject();
        attackDamageMultipliers = new float[]{ 1, 1, 1, 1, 1 };
        _spawnedButterflies = new List<Butterfly>();
        BindingSkillPreservations = new ELegacyPreservation[(int)EWarrior.MAX];
        for (int i = 0; i < (int)EWarrior.MAX; i++) BindingSkillPreservations[i] = ELegacyPreservation.MAX;
        _animator = GetComponent<Animator>();
        _playerController = PlayerController.Instance;
        _playerMovement = _playerController.playerMovement;
        var attacks = transform.Find("Attacks");
        AttackBases = new AttackBase[]
        {
            attacks.GetComponent<AttackBase_Melee>(), attacks.GetComponent<AttackBase_Ranged>(),
            attacks.GetComponent<AttackBase_Dash>(), attacks.GetComponent<AttackBase_Area>()
        };
        GameEvents.Restarted += OnRestarted;
        _dashUIOverlay.fillAmount = 0.0f;
        _canSaveNextAttack = true;
    }

    private void OnRestarted()
    {
        ResetNightShadeDarkGauge();
        attackDamageMultipliers = new float[]{ 1, 1, 1, 1, 1 };
        foreach (var attack in AttackBases) attack.Reset();
        CurrAttackIdx = -1;
        NextAttackIdx = -1;
        foreach (var butterfly in _spawnedButterflies) Destroy(butterfly);
        _spawnedButterflies.Clear();
        for (int i = 0; i < (int)EWarrior.MAX; i++) BindingSkillPreservations[i] = ELegacyPreservation.MAX;
        if (_dashCooldownCoroutine != null) StopCoroutine(_dashCooldownCoroutine);
        _dashUIOverlay.fillAmount = 0.0f;
        _canSaveNextAttack = true;
        totalDamageMultiplier = 1.0f;
    }

    private IEnumerator DashCooldownCoroutine()
    {
        float dashCooldown = 0.6f;
        float timer = dashCooldown;

        // During the cooldown, update UI
        while (timer >= 0)
        {
            _dashUIOverlay.fillAmount = timer / dashCooldown;
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }
        _dashUIOverlay.fillAmount = 0.0f;
        _dashCooldownCoroutine = null;
    }

    public void OnAttack(int attackIdx)
    {
        // No attack can be done if under another attack or under attack delay
        if (CurrAttackIdx != -1 || IsUnderAttackDelay)
        {
            // Attack buffer
            if (!_canSaveNextAttack)
            {
                Debug.Log("!!! Cannot save attack, animation not near end");
                return;
            }
            NextAttackIdx = attackIdx;
            Debug.Log("Attack saved in buffer!");
            return;
        }
        // If an attack is saved in the buffer, should play that instead
        if (NextAttackIdx != -1) return;

        // Handle NightShade dash separately
        if (attackIdx == (int)ELegacyType.Dash && AttackBases[attackIdx].activeWarrior == EWarrior.NightShade)
        {
            if (((AttackBase_Dash)AttackBases[attackIdx]).IsCurrentNightShadeTpDash())
            {
                AttackBases[attackIdx].Attack();
            }
            // If this is the first dash with no active cooldown, start dash
            else if (_dashCooldownCoroutine == null)
            {
                _dashCooldownCoroutine = StartCoroutine(DashCooldownCoroutine());
                AttackBases[attackIdx].Attack();
                _canSaveNextAttack = false;
            }
            return;
        }

        // Handle other attacks
        // For dash, check cooldown
        if (attackIdx == (int)ELegacyType.Dash)
        {
            if (_dashCooldownCoroutine == null)
            {
                _dashCooldownCoroutine = StartCoroutine(DashCooldownCoroutine());
                _playerMovement.SetDash();
            }
            else return;
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

        IsUnderAttackDelay = true;
        CurrAttackIdx = attackIdx;
        AttackBases[attackIdx].Attack();
        _canSaveNextAttack = false;
    }

    public void OnAttackEnd(ELegacyType attackType)
    {
        // 막은거 풀기
        Debug.Log("Attack end: curr idx set to -1");
        CurrAttackIdx = -1;
        _animator.SetInteger(AttackIndex, -1);
        if (attackType == ELegacyType.Melee) AttackBases[(int)attackType].VFXObject.SetActive(false);
        StartCoroutine(AttackBases[(int)attackType].AttackPostDelayCoroutine());
        _playerMovement.isDashing = false;
    }

    // Called when attack delay ends
    public void OnAttackEnd_PostDelay()
    {
        IsUnderAttackDelay = false;
        _playerMovement.EnableMovement(false);
        
        // If an attack is saved in the buffer, play
        if (NextAttackIdx != -1)
        {
            Debug.Log("Buffer has next attack, playing " + (ELegacyType)NextAttackIdx);
            var attackToPlay = NextAttackIdx;
            NextAttackIdx = -1;
            OnAttack(attackToPlay);
        }
    }

    // IDamageDealer Override
    public void DealDamage(IDamageable target, AttackInfo attackInfo)
    {
        // 임시 데미지 정보 생성
        AttackInfo infoToSend = attackInfo.Clone();

        // 방어 관통력 처리
        infoToSend.AttackerArmourPenetration = PlayerController.Instance.ArmourPenetration;

        // 크리티컬 처리
        bool isCritAttack = false;
        if (Random.value <= PlayerController.Instance.CriticalRate)
        {
            infoToSend.Damage.TotalAmount *= 2;
            isCritAttack = true;
            UIManager.Instance.DisplayCritPopUp(transform.position + new Vector3(0, 2.3f, 0));
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
                    Debug.Log("Shade: Base " + addAmount + " Additive: " + _playerController.NightShadeShadeBonusStats[(int)_playerController.NightShadeShadeBonusPreserv]);
                    addAmount += _playerController.NightShadeShadeBonusStats[(int)_playerController.NightShadeShadeBonusPreserv];
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
        _darkGauge = Mathf.Clamp(_darkGauge + change, 0, 100);
        UIManager.Instance.UpdateDarkGaugeUI(_darkGauge);
    }

    private void ResetNightShadeDarkGauge() => UpdateNightShadeDarkGauge(-100);

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
    
    // Animation event
    public void OnMeleeComboHit()
    {
        ((AttackBase_Melee)AttackBases[(int)ELegacyType.Melee]).OnComboHit();
    }
    
    // Animation event
    public void EnableSaveNextAttack()
    {
        // Debug.Log("Can save attack");
        _canSaveNextAttack = true;
    }
}
