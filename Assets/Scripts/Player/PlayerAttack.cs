using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour, IDamageDealer
{
    private Animator _animator;
    private PlayerMovement _playerMovement;
    private AttackBase[] _attacks;
    public int CurrAttackIdx = -1;
    public bool IsUnderAttackDelay = false;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();
        _attacks = new AttackBase[]
        {
            GetComponent<AttackBase_Melee>(),
            GetComponent<AttackBase_Range>(),
            GetComponent<AttackBase_Dash>(),
            GetComponent<AttackBase_Area>()
        };
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
            IsUnderAttackDelay = true;
            CurrAttackIdx = attackIdx;
            _attacks[attackIdx].Attack();
            GetComponent<PlayerMovement>().DisableMovement(false);
        }
    }

    public void OnAttackEnd(ELegacyType attackType)
    {
        // 막은거 풀기
        CurrAttackIdx = -1;
        _animator.SetInteger("AttackIndex", CurrAttackIdx);
        StartCoroutine(_attacks[(int)attackType].AttackPostDelayCorountine());
    }

    // Called when attack delay ends
    public void OnAttackEnd_Post()
    {
        IsUnderAttackDelay = false;
        GetComponent<PlayerMovement>().EnableMovement(false);
    }

    public void DealDamage(IDamageable target, SDamageInfo damageInfo)
    {
        throw new System.NotImplementedException();
    }
}