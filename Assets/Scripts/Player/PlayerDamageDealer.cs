using System;
using UnityEngine;

public class PlayerDamageDealer : MonoBehaviour, IDamageDealer
{
    private Animator _animator;
    private PlayerMovement _playerMovement;
    public AttackBase[] AttackBases { get; set; }
    public int CurrAttackIdx = -1;
    public bool IsUnderAttackDelay = false;
    
    // Legacy
    private int[] _statusEffectLevels;
    
    private void Start()
    {
        _statusEffectLevels = new int[(int)EWarrior.MAX];
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
        foreach (var attack in AttackBases) attack.Reset();
        CurrAttackIdx = -1;
    }

    public bool CanAttack(int attackIdx)
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
            {
                _playerMovement.SetDash();
            }

            IsUnderAttackDelay = true;
            CurrAttackIdx = attackIdx;
            AttackBases[attackIdx].Attack();
        }
    }

    public void OnAttackEnd(ELegacyType attackType)
    {
        // 막은거 풀기
        CurrAttackIdx = -1;
        _animator.SetInteger("AttackIndex", CurrAttackIdx);
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
        infoToSend.AttackerArmourPenetration = PlayerController.Instance.ArmourPenetration;
        target.TakeDamage(infoToSend);
    }

    public int GetStatusEffectLevel(EWarrior warrior)
    {
        return _statusEffectLevels[(int)warrior];
    }
}