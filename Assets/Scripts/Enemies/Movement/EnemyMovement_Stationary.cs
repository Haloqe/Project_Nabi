using System.Collections;
using UnityEngine;

public class EnemyMovement_Stationary : EnemyMovement
{
    [SerializeField] private float _flipProbability = 0.1f; //walkProbability will be 1 - idleProbability
    [SerializeField] private float _idleAverageDuration = 1f;
    [SerializeField] private float _detectRange = 3f;
    [SerializeField] private float _attackRange = 1f;
    private float _actionTimeCounter = 0f;


    private void Update()
    {
        if (_isRooted) return;
        if (_target == null)
        {
            Patrol();
            return;
        }

        bool playerIsInAttackRange = Mathf.Abs(transform.position.x - _target.transform.position.x) <= _attackRange 
            && _target.transform.position.y - transform.position.y <= 1f;

        bool playerIsInDetectRange = Mathf.Abs(transform.position.x - _target.transform.position.x) <= _detectRange 
            && _target.transform.position.y - transform.position.y <= 1f;
        
        if (playerIsInAttackRange)
        {
            Attack();
            return;
        }
        else if (playerIsInDetectRange)
        {
            Chase();
        }
        else
        {
            Patrol();
        }

        _actionTimeCounter -= Time.deltaTime;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // dealing damage to target
        // may have to edit to only take the child collider into account
        if (other.CompareTag(_target.tag))
        {
            _enemyBase.DealDamage(_targetDamageable, _enemyBase._damageInfoTEMP);
        }
    }


    private void Patrol()
    {
        _animator.SetBool("IsAttacking", false);
        if (_actionTimeCounter <= 0)
        {
            if (Random.Range(0.0f, 1.0f) <= _flipProbability)
            {
                FlipEnemy();
                _actionTimeCounter = Random.Range(_idleAverageDuration * 0.5f, _idleAverageDuration * 1.5f);
            }
        }
    }

    private void Chase()
    {
        if (_isFlippable)
        {
            FlipEnemyTowardsTarget();
            _animator.SetBool("IsAttacking", false);
        }
    }

    private void Attack()
    {
        if (_isFlippable) FlipEnemyTowardsTarget();
        _animator.SetBool("IsAttacking", true);
    }

}
