using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public class PlayerDamageDealer : MonoBehaviour, IDamageDealer
{
    private Animator _animator;
    private PlayerMovement _playerMovement;
    public AttackBase[] AttackBases { get; private set; }
    public float[] attackDamageMultipliers;
    public int CurrAttackIdx = -1;
    public bool IsUnderAttackDelay = false;
    
    // Turbela Butterfly
    public List<Butterfly> spawnedButterflies;
    private int _butterflySpawnLimit = 6;
    
    // Legacy
    public ELegacyPreservation[] BindingSkillPreservations { private set; get; }
    private readonly static int AttackIndex = Animator.StringToHash("AttackIndex");

    private void Start()
    {
        attackDamageMultipliers = new float[] {1,1,1,1,1};
        spawnedButterflies = new List<Butterfly>();
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
        attackDamageMultipliers = new float[] {1,1,1,1,1};
        foreach (var attack in AttackBases) attack.Reset();
        CurrAttackIdx = -1;
        foreach (var butterfly in spawnedButterflies) Destroy(butterfly);
        spawnedButterflies.Clear();
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
    public void DealDamage(IDamageable target, AttackInfo damageInfo)
    {
        AttackInfo infoToSend = damageInfo.Clone();
        
        // 방어 관통력 처리
        infoToSend.AttackerArmourPenetration = PlayerController.Instance.ArmourPenetration;
        
        // 크리티컬 처리
        if (Random.value <= PlayerController.Instance.CriticalRate)
        {
            infoToSend.Damage.TotalAmount *= 2;
        }
        
        // 투르벨라 나비 처리
        if (damageInfo.StatusEffects.Count > 0 && 
            damageInfo.StatusEffects[^1].Effect is EStatusEffect.Swarm or EStatusEffect.Cloud)
        {
            if (Random.value <= damageInfo.StatusEffects[^1].Chance)
            {
                SpawnTurbelaButterfly();
            }
        }
            
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
                // todo
                break;
        }
        BindingSkillPreservations[(int)warrior] = preservation;
        // foreach (var attack in AttackBases) attack.UpdateLegacyStatusEffect();
    }
    
    public void SpawnTurbelaButterfly()
    {
        if (spawnedButterflies.Count >= _butterflySpawnLimit) return;
        
        var butterfly = Resources.Load("Prefabs/Player/SpawnObjects/Butterfly").GameObject();
        var obj = Instantiate(butterfly).GetComponent<Butterfly>();
        obj.attackTwiceChance = Define.TurbelaButterflyAttackTwiceChances[(int)BindingSkillPreservations[(int)EWarrior.Turbela]];
        spawnedButterflies.Add(obj);
    }
}