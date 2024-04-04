using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyMovement_LinearPath : EnemyMovement
{

    // Ground detection
    [SerializeField] Vector2 _groundColliderSize;
    public LayerMask _groundLayer;
    public Collider2D _groundInFrontCollider;
    public Collider2D _ceilingInFrontCollider;
    
    private void Awake()
    {
        moveType = EEnemyMoveType.LinearPath;
    }

    private void WalkForward()
    {
        if (IsRooted) return;
        if (!IsGrounded()) return;
        if (IsAtEdge()) FlipEnemy();

        if (transform.localScale.x > Mathf.Epsilon) //if it's facing right
        {
            _rigidBody.velocity = new Vector2(_moveSpeed, 0f);
        } else {
            _rigidBody.velocity = new Vector2(-_moveSpeed, 0f);
        }
    }

    private void GenerateRandomState()
    {
        if (Random.Range(0.0f, 1.0f) <= _enemyBase.EnemyData.IdleProbability)
        {
            IsMoving = false;
            _enemyBase.ActionTimeCounter = Random.Range(_enemyBase.EnemyData.IdleAverageDuration * 0.5f, _enemyBase.EnemyData.IdleAverageDuration * 1.5f);
        } else
        {
            IsMoving = true;
            _enemyBase.ActionTimeCounter = Random.Range(_enemyBase.EnemyData.WalkAverageDuration * 0.5f, _enemyBase.EnemyData.WalkAverageDuration * 1.5f);

            if (Random.Range(0.0f, 1.0f) <= 0.3f) FlipEnemy();
        }
    }

    public override void Patrol()
    {
        if (IsAtEdge() && IsChasingPlayer)
        {
            _rigidBody.velocity = Vector2.zero;
            _animator.SetBool("IsWalking", false);
            return;
        }

        _animator.SetBool("IsAttacking", false);
        IsChasingPlayer = false;
        if (_enemyBase.ActionTimeCounter <= 0) GenerateRandomState();
        if (IsMoving) WalkForward();
        else _rigidBody.velocity = Vector2.zero;
        _animator.SetBool("IsWalking", IsMoving);
    }

    public override void Chase()
    {
        if (IsAtEdge())
        {
            Patrol();
            FlipEnemyTowardsTarget();
            return;
        }

        if (_enemyBase.ActionTimeCounter < 0)
        {
            IsChasingPlayer = false;
            return;
        }
        _animator.SetBool("IsAttacking", false);
        _animator.SetBool("IsWalking", true);
        
        FlipEnemyTowardsTarget();
        WalkForward();
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

    public bool IsGrounded()
    {
        if (Physics2D.OverlapBox(transform.position, _groundColliderSize, 0, _groundLayer)) return true;
        else return false;
    }

    public bool IsAtEdge()
    {
        if (_ceilingInFrontCollider.IsTouchingLayers(_groundLayer) || !_groundInFrontCollider.IsTouchingLayers(_groundLayer)) return true;
        else return false;
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up, _groundColliderSize);
    }
}
