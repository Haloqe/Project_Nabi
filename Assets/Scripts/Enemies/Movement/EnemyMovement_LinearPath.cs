using System.Collections;
using UnityEngine;

public class EnemyMovement_LinearPath : EnemyMovement
{
    private bool _isOnAir = true;
    private bool _isMoving = true;
    private bool _isChasingPlayer = false;
    private bool _isInAttackSequence = false;
    private GameObject _player;

    [SerializeField] private float _idleProbability = 0.4f; //walkProbability will be 1 - idleProbability
    [SerializeField] private float _idleAverageDuration = 1f;
    [SerializeField] private float _walkAverageDuration = 0.8f;
    [SerializeField] private float _detectRange = 3f;
    [SerializeField] private float _attackRange = 1f;
    [SerializeField] private float _chasePlayerDuration = 5f;
    private float _actionTimeCounter = 0f;

    private void Awake()
    {
        _player = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        if (_isOnAir) return;

        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
            _isInAttackSequence = true;
        } else {
            _isInAttackSequence = false;
        }

        bool playerIsInAttackRange = Mathf.Abs(transform.position.x - _player.transform.position.x) <= _attackRange 
            && _player.transform.position.y - transform.position.y <= 1f;

        bool playerIsInDetectRange = Mathf.Abs(transform.position.x - _player.transform.position.x) <= _detectRange 
            && _player.transform.position.y - transform.position.y <= 1f;
        
        if (playerIsInAttackRange)
        {
            Attack();
            return;
        }
        else if (playerIsInDetectRange)
        {
            _actionTimeCounter = _chasePlayerDuration;
            Chase();
        }
        else
        {
            if (_isChasingPlayer && _actionTimeCounter > 0) Chase();
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
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //if (_isChasingPlayer) return; needs edits.
        if (_isChasingPlayer) _actionTimeCounter = 0;
        FlipEnemy();
    }

    private void FlipEnemy()
    {
        transform.localScale = new Vector2(
            -1 * transform.localScale.x, transform.localScale.y);
    }

    private void FlipEnemyTowardsTarget()
    {
        if (transform.position.x - _player.transform.position.x >= 0)
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
        _animator.SetBool("IsAttacking", false);
        _animator.SetBool("IsWalking", true);
        _isChasingPlayer = true;
        FlipEnemyTowardsTarget();
        WalkForward();
    }

    private void Attack()
    {
        if (_isInAttackSequence) FlipEnemyTowardsTarget();
        _animator.SetBool("IsAttacking", true);
    }

}

