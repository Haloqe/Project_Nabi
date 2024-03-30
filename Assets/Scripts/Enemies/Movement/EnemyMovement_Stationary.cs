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

}
