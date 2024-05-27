using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class PlayerDamageDealer : MonoBehaviour, IDamageDealer
{
    private Animator _animator;
    private PlayerMovement _playerMovement;
    public AttackBase[] AttackBases { get; private set; }
    public float[] attackDamageMultipliers;
    public int CurrAttackIdx = -1;
    public bool IsUnderAttackDelay = false;
    
    // Turbela Butterfly
    private GameObject _butterflyPrefab;
    private List<Butterfly> _spawnedButterflies;
    private int ButterflySpawnLimit => (int)Define.TurbelaMaxButterflyStats[(int)PlayerController.Instance.TurbelaMaxButterflyPreserv];
    
    // NightShade
    private float _darkGauge;
    private bool IsDarkGaugeFull => _darkGauge == 100;
    
    // Legacy
    public ELegacyPreservation[] BindingSkillPreservations { private set; get; }
    private readonly static int AttackIndex = Animator.StringToHash("AttackIndex");

    private void Start()
    {
        _butterflyPrefab = Resources.Load("Prefabs/Player/SpawnObjects/Butterfly").GameObject();
        attackDamageMultipliers = new float[] {1,1,1,1,1};
        _spawnedButterflies = new List<Butterfly>();
        BindingSkillPreservations = new ELegacyPreservation[(int)EWarrior.MAX];
        for (int i = 0; i < (int)EWarrior.MAX; i++) BindingSkillPreservations[i] = ELegacyPreservation.MAX;
        _animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();
        AttackBases = new AttackBase[]
        {
            GetComponent<AttackBase_Melee>(),
            GetComponent<AttackBase_Ranged>(),
            GetComponent<AttackBase_Dash>(),
            GetComponent<AttackBase_Area>()
        };
        GameEvents.restarted += OnRestarted;
    }
    
    private void OnRestarted()
    {
        ResetNightShadeDarkGauge();
        attackDamageMultipliers = new float[] {1,1,1,1,1};
        foreach (var attack in AttackBases) attack.Reset();
        CurrAttackIdx = -1;
        foreach (var butterfly in _spawnedButterflies) Destroy(butterfly);
        _spawnedButterflies.Clear();
        for (int i = 0; i < (int)EWarrior.MAX; i++) BindingSkillPreservations[i] = ELegacyPreservation.MAX;
    }

    private bool CanAttack(int attackIdx)
    {
        // TODO Movement check
        return CurrAttackIdx == -1 && !IsUnderAttackDelay;
    }

    public void OnAttack(int attackIdx)
    {
        if (CanAttack(attackIdx))
        {
            //방향전환 막기
            //이동.점프는?
            if (attackIdx != (int)ELegacyType.Dash)
                _playerMovement.DisableMovement(false);
            else
                _playerMovement.SetDash();

            IsUnderAttackDelay = true;
            CurrAttackIdx = attackIdx;
            AttackBases[attackIdx].Attack();
        }
    }

    public void OnAttackEnd(ELegacyType attackType)
    {
        // 막은거 풀기
        CurrAttackIdx = -1;
        _animator.SetInteger(AttackIndex, CurrAttackIdx);
        if (attackType == ELegacyType.Melee) AttackBases[(int)attackType].VFXObject.SetActive(false);
        StartCoroutine(AttackBases[(int)attackType].AttackPostDelayCoroutine());
        _playerMovement._isDashing = false;
    }

    // Called when attack delay ends
    public void OnAttackEnd_PostDelay()
    {
        IsUnderAttackDelay = false;
        _playerMovement.EnableMovement(false);
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
        }
        
        // 어둠 게이지 완충 공격
        var isDarkChargedAttack = false;
        if (IsDarkGaugeFull && attackInfo.CanBeDarkAttack)
        {
            isDarkChargedAttack = true;
            infoToSend.Damage.TotalAmount *= 1.5f;
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
                    attackInfo.StatusEffects.RemoveAt(attackInfo.StatusEffects.Count - 1);
                }
                else
                {
                    // 일반 공격
                    var addAmount = 5;
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
                    UpdateNightShadeDarkGauge(addAmount);
                }
            }
        }
        
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
            PlayerController.Instance.evasionRateAdditionAtMax = 0.2f;
        }
    }

    public void TurbelaKillButterfly(Butterfly butterflyToKill)
    {
        if (_spawnedButterflies.Count == 0) return;
        
        // 유산 - 흩어져라
        if (_spawnedButterflies.Count == ButterflySpawnLimit)
        {
            PlayerController.Instance.evasionRateAdditionAtMax = 0.0f;
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
}