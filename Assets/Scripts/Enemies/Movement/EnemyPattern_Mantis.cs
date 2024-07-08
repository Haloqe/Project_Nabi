using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyPattern_Mantis : EnemyPattern
{
    // Ground detection
    [SerializeField] Vector2 _groundColliderSize;
    public LayerMask _groundLayer;
    public Collider2D _groundInFrontCollider;
    public Collider2D _ceilingInFrontCollider;

    private float _chaseSpeed = 5f;
    private float _dashSpeed = 16f;
    private float _dashTime = 0.3f;
    [SerializeField] private ParticleSystemRenderer _dashVFX;
    
    // sfx
    [SerializeField] private AudioClip _dashAudio;
    
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

    private void Awake()
    {
        MoveType = EEnemyMoveType.LinearPath;
        Init();
    }

    private void WalkForward(bool isChasing = false)
    {
        if (IsRooted) return;
        if (!IsGrounded()) return;
        if (IsAtEdge()) FlipEnemy();
        float moveSpeed = isChasing ? _chaseSpeed : MoveSpeed;

        _rigidBody.velocity = transform.localScale.x > Mathf.Epsilon ?
            new Vector2(moveSpeed, 0f) :
            new Vector2(-moveSpeed, 0f); //if it's facing right
    }

    // private void GenerateRandomState()
    // {
    //     if (Random.Range(0.0f, 1.0f) <= _enemyBase.EnemyData.IdleProbability)
    //     {
    //         IsMoving = false;
    //         _enemyBase.ActionTimeCounter = Random.Range(_enemyBase.EnemyData.IdleAverageDuration * 0.5f,
    //             _enemyBase.EnemyData.IdleAverageDuration * 1.5f);
    //     } else
    //     {
    //         IsMoving = true;
    //         _enemyBase.ActionTimeCounter = Random.Range(_enemyBase.EnemyData.WalkAverageDuration * 0.5f,
    //             _enemyBase.EnemyData.WalkAverageDuration * 1.5f);
    //
    //         if (Random.Range(0.0f, 1.0f) <= 0.3f) FlipEnemy();
    //     }
    // }

    public override void Patrol()
    {
        if (IsAtEdge() && IsChasingPlayer)
        {
            _rigidBody.velocity = Vector2.zero;
            
            return;
        }

        _animator.SetBool(IsAttacking, false);
        IsChasingPlayer = false;
        // if (_enemyBase.ActionTimeCounter <= 0) GenerateRandomState();
        // if (IsMoving) WalkForward();
        // else _rigidBody.velocity = Vector2.zero;
        // _animator.SetBool(IsWalking, IsMoving);
        
        WalkForward();
    }

    public override void Chase()
    {
        if (IsAtEdge())
        {
            Patrol();
            FlipEnemyTowardsTarget();
            return;
        }
        
        _animator.SetBool(IsAttacking, false);
        // _animator.SetBool(IsWalking, true);
        
        FlipEnemyTowardsTarget();
        WalkForward(true);
    }

    public override void Attack()
    {
        // if (IsFlippable) FlipEnemyTowardsTarget();
        // _animator.SetBool(IsAttacking, true);
        // ChangeSpeedByPercentage(0);
        if (IsAttackingPlayer) return;

        StartCoroutine(Telegraph());
    }

    private IEnumerator Telegraph()
    {
        IsAttackingPlayer = true;
        _animator.SetTrigger("Telegraph");
        _rigidBody.velocity = Vector2.zero;
        yield return new WaitForSeconds(1f);
        StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        _animator.SetTrigger("FirstAttack");
        yield return DashAttack();
        yield return new WaitForSeconds(0.5f);
        _animator.SetTrigger("SecondAttack");
        yield return DashAttack();
        yield return new WaitForSeconds(0.3f);

        _animator.SetTrigger("EndAttack");
        IsAttackingPlayer = false;
    }

    private IEnumerator DashAttack()
    {
        _audioSource.pitch = Random.Range(0.85f, 1.15f);
        _audioSource.PlayOneShot(_dashAudio, 0.4f);
        float dashTimeCounter = 0;
        float direction = Math.Sign(transform.localScale.x);
        _dashVFX.flip = new Vector3(-direction, 0, 0);
        while (dashTimeCounter <= _dashTime)
        {
            _rigidBody.velocity = IsAtEdge() ? Vector2.zero : new Vector2(_dashSpeed * direction, 0f);
            dashTimeCounter += Time.deltaTime;
            yield return null;
        }
    }

    public override bool PlayerIsInAttackRange()
    {
        return Mathf.Abs(transform.position.x - _enemyBase.Target.transform.position.x)
               <= _enemyBase.EnemyData.AttackRangeX 
            && Mathf.Abs(_enemyBase.Target.transform.position.y - transform.position.y)
               <= _enemyBase.EnemyData.AttackRangeY;
    }

    public override bool PlayerIsInDetectRange()
    {
        return Mathf.Abs(transform.position.x - _enemyBase.Target.transform.position.x)
               <= _enemyBase.EnemyData.DetectRangeX 
            && Mathf.Abs(_enemyBase.Target.transform.position.y - transform.position.y)
               <= _enemyBase.EnemyData.DetectRangeY;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapBox(transform.position, _groundColliderSize, 0, _groundLayer);
    }

    private bool IsAtEdge()
    {
        return _ceilingInFrontCollider.IsTouchingLayers(_groundLayer)
               || !_groundInFrontCollider.IsTouchingLayers(_groundLayer);
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, _groundColliderSize);
    }
}
