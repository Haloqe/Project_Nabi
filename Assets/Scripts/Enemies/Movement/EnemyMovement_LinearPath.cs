using System.Collections;
using UnityEngine;

public class EnemyMovement_LinearPath : EnemyMovement
{
    private bool _isOnAir = true;
    private bool _isMoving = true;
    private bool _isChasingPlayer = false;
    private bool _isInAttackSequence = false;
    private bool _isFlippable = true;

    [SerializeField] private float _idleProbability = 0.4f; //walkProbability will be 1 - idleProbability
    [SerializeField] private float _idleAverageDuration = 1f;
    [SerializeField] private float _walkAverageDuration = 0.8f;
    [SerializeField] private float _detectRange = 3f;
    [SerializeField] private float _attackRange = 1f;
    [SerializeField] private float _chasePlayerDuration = 5f;
    private float _actionTimeCounter = 0f;


    private void Update()
    {
        if (_isOnAir) return;

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
            _isChasingPlayer = true;
            _actionTimeCounter = _chasePlayerDuration;
            Chase();
        }
        else
        {
            if (_isChasingPlayer) Chase();
            else Patrol();
        }

        _actionTimeCounter -= Time.deltaTime;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            _isOnAir = false;
        }

        // dealing damage to target
        // may have to edit to only take the child collider into account
        if (other.CompareTag(_target.tag))
        {
            _enemyBase.DealDamage(_targetDamageable, _damageBaseTEMP);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //if (_isChasingPlayer) return; needs edits.
        if (_isChasingPlayer) _actionTimeCounter = 0;
        if (other.CompareTag("Ground")) FlipEnemy();
    }


    private void FlipEnemy()
    {
        transform.localScale = new Vector2(
            -1 * transform.localScale.x, transform.localScale.y);
    }

    private void FlipEnemyTowardsTarget()
    {
        if (transform.position.x - _target.transform.position.x >= 0)
        {
            if (transform.localScale.x > Mathf.Epsilon) FlipEnemy();
        } else {
            if (transform.localScale.x < Mathf.Epsilon) FlipEnemy();
        }
    }

    private void WalkForward()
    {
        if (transform.localScale.x > Mathf.Epsilon) //if it's facing right
        {
            _rigidBody.velocity = new Vector2(_moveSpeed, 0f);
        } else {
            _rigidBody.velocity = new Vector2(-_moveSpeed, 0f);
        }
    }

    private void GenerateRandomState()
    {
        if (Random.Range(0.0f, 1.0f) <= _idleProbability)
        {
            _isMoving = false;
            _actionTimeCounter = Random.Range(_idleAverageDuration * 0.5f, _idleAverageDuration * 1.5f);
        } else
        {
            _isMoving = true;
            _actionTimeCounter = Random.Range(_walkAverageDuration * 0.5f, _walkAverageDuration * 1.5f);

            if (Random.Range(0.0f, 1.0f) <= 0.5f) FlipEnemy();
        }
    }

    private void Patrol()
    {
        _animator.SetBool("IsAttacking", false);
        _isChasingPlayer = false;
        if (_actionTimeCounter <= 0) GenerateRandomState();
        if (_isMoving) WalkForward();
        _animator.SetBool("IsWalking", _isMoving);
    }

    private void Chase()
    {
        if (_actionTimeCounter < 0)
        {
            _isChasingPlayer = false;
            return;
        }
        _animator.SetBool("IsAttacking", false);
        _animator.SetBool("IsWalking", true);
        
        FlipEnemyTowardsTarget();
        WalkForward();
    }

    private void Attack()
    {
        if (_isFlippable) FlipEnemyTowardsTarget();
        _animator.SetBool("IsAttacking", true);
    }

    // for animation events
    private void DisableFlip()
    {
        _isFlippable = false;
    }

    private void EnableFlip()
    {
        _isFlippable = true;
    }

}
