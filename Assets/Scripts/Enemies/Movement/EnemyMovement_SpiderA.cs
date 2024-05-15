using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement_SpiderA : EnemyMovement
{
    // Ground detection
    [SerializeField] Vector2 _groundColliderSize;
    public LayerMask _groundLayer;
    public Collider2D _groundInFrontCollider;
    public Collider2D _ceilingInFrontCollider;
    
    private void Awake()
    {
        MoveType = EEnemyMoveType.SpiderA;
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

    private void RunTowardsPlayer()
    {
        _animator.SetBool("IsAttacking", false);
        _animator.SetBool("IsWalking", true);
        _animator.SetFloat("RunMultiplier", 2f);
        
        FlipEnemyTowardsTarget();
        WalkForward();
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
            // _rigidBody.velocity = Vector2.zero;
            // _animator.SetBool("IsWalking", false);
            // return;
            
            // jump down the platform!!
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
        // jump towards the player 30 pixels
    }

    public override void Attack()
    {
        if (_enemyBase.ActionTimeCounter >= 0) return;

        PlayerDamageReceiver playerDamager = _enemyBase.Target.gameObject.GetComponent<PlayerDamageReceiver>();

        // TODO
        // if (playerDamager.StatusEffectVFXs[(int)EStatusEffect.Poison] != null &&
        //     playerDamager.StatusEffectVFXs[(int)EStatusEffect.Stun] != null)
        // {
        //     if (!PlayerIsInWebAttackRange())
        //     {
        //         RunTowardsPlayer();
        //         return;
        //     }
        //
        //     Jump();
        //     WebAttack();
        //     _enemyBase.ActionTimeCounter = 1.5f;
        //     return;
        // }

        if (!PlayerIsInTeethAttackRange())
        {
            RunTowardsPlayer();
            return;
        }

        TeethAttack();
        _enemyBase.ActionTimeCounter = 1.5f;
        return;
    }

    private void Jump()
    {

    }

    private void WebAttack()
    {

    }

    private void TeethAttack()
    {

    }

    public override bool PlayerIsInAttackRange() // the 30 pixels mentioned in flowchart
    {
        return Mathf.Abs(transform.position.x - _enemyBase.Target.transform.position.x) <= _enemyBase.EnemyData.AttackRangeX 
            && _enemyBase.Target.transform.position.y - transform.position.y <= _enemyBase.EnemyData.AttackRangeY;
    }

    public override bool PlayerIsInDetectRange() // range where it jumps towards player
    {
        return Mathf.Abs(transform.position.x - _enemyBase.Target.transform.position.x) <= _enemyBase.EnemyData.DetectRangeX 
            && _enemyBase.Target.transform.position.y - transform.position.y <= _enemyBase.EnemyData.DetectRangeY;
        
        // is it over 30 pixels?
    }

    private bool PlayerIsInWebAttackRange()
    {
        return false;
    }

    private bool PlayerIsInTeethAttackRange()
    {
        return false;
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
