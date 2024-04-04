using System.Collections;
using UnityEngine;

public class EnemyMovement_Stationary : EnemyMovement
{
    private void Awake()
    {
        moveType = EEnemyMoveType.Stationary;
    }
    
    public override void Patrol()
    {
        _animator.SetBool("IsAttacking", false);
        if (_enemyBase.ActionTimeCounter <= 0)
        {
            if (Random.Range(0.0f, 1.0f) <= 0.5f)
            {
                FlipEnemy();
                _enemyBase.ActionTimeCounter = Random.Range(_enemyBase.EnemyData.IdleAverageDuration * 0.5f, _enemyBase.EnemyData.IdleAverageDuration * 1.5f);
            }
        }
    }

    public override void Chase()
    {
        if (IsFlippable)
        {
            FlipEnemyTowardsTarget();
            _animator.SetBool("IsAttacking", false);
        }
    }

    public override void Attack()
    {
        if (IsFlippable) FlipEnemyTowardsTarget();
        _animator.SetBool("IsAttacking", true);
    }

    public override bool PlayerIsInAttackRange()
    {
        return Mathf.Abs(transform.position.x - _enemyBase.Target.transform.position.x) <= _enemyBase.EnemyData.AttackRangeX 
            && _enemyBase.Target.transform.position.y - transform.position.y <= _enemyBase.EnemyData.AttackRangeY;
    }

    public override bool PlayerIsInDetectRange()
    {
        return Mathf.Abs(transform.position.x - _enemyBase.Target.transform.position.x) <= _enemyBase.EnemyData.DetectRangeX 
            && _enemyBase.Target.transform.position.y - transform.position.y <= _enemyBase.EnemyData.DetectRangeY;
    }

}
