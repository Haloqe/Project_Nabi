using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyMovement : MonoBehaviour
{
    protected EnemyBase _enemyBase;
    private GameObject _attacker;
    public EEnemyMoveType MoveType;
    protected float _moveSpeed;
    public bool IsRooted = false;
    public bool IsMoving = true;
    public bool IsChasingPlayer = false;
    public bool IsAttackingPlayer = false;
    public bool IsFlippable = true;
    protected Rigidbody2D _rigidBody;
    protected Animator _animator;

    public void Init()
    {
        _animator = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _enemyBase = GetComponent<EnemyBase>();
        _moveSpeed = _enemyBase.EnemyData.DefaultMoveSpeed;
        EnableMovement();
    }

    public virtual void ResetMoveSpeed()
    {
        _moveSpeed = _enemyBase.EnemyData.DefaultMoveSpeed;
    }

    public void ChangeSpeedByPercentage(float percentage)
    {
        _moveSpeed = _enemyBase.EnemyData.DefaultMoveSpeed * percentage;
    }

    public void FlipEnemy()
    {
        transform.localScale = new Vector2(
            -1 * transform.localScale.x, transform.localScale.y);
    }

    public void FlipEnemyTowardsMovement()
    {
        if (_rigidBody.velocity.x >= 0.01f) transform.localScale = new Vector3(1f, 1f, 1f);
        else transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    public void FlipEnemyTowardsTarget()
    {
        if (transform.position.x - _enemyBase.Target.transform.position.x >= 0)
        {
            if (transform.localScale.x > Mathf.Epsilon) FlipEnemy();
        } else {
            if (transform.localScale.x < Mathf.Epsilon) FlipEnemy();
        }
    }

    public virtual void EnableMovement()
    {
        IsRooted = false;
        // ResetMoveSpeed();
    }

    public virtual void DisableMovement()
    {
        IsRooted = true;
        // _animator.SetBool("IsAttacking", false);
        // _animator.SetBool("IsWalking", false);
    }

    // for animation events
    protected virtual void DisableFlip()
    {
        IsFlippable = false;
    }

    protected virtual void EnableFlip()
    {
        IsFlippable = true;
    }

    private void EnterAttackSequence()
    {
        IsAttackingPlayer = true;
    }

    private void ExitAttackSequence()
    {
        IsAttackingPlayer = false;
        _animator.SetBool("IsAttacking", false);
    }

    protected Vector2 pullOverallVelocity = Vector2.zero;
    public void StartPullX(int direction, float strength, float duration)
    {
        DisableFlip();
        IsRooted = true;
        IsMoving = false;
        pullOverallVelocity += new Vector2(direction * strength, 0);
        StartCoroutine(PullXCoroutine(direction, strength, duration));
    }

    // Currently only considers the x axis; need to change if needed in future
    protected virtual IEnumerator PullXCoroutine(int direction, float strength, float duration)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            _rigidBody.velocity = pullOverallVelocity;
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        pullOverallVelocity -= new Vector2(direction * strength, 0);
        _rigidBody.velocity = pullOverallVelocity;
        if (pullOverallVelocity == Vector2.zero) EnableFlip();
        
        // while (elapsedTime < duration)
        // {
        //     _rigidBody.AddForce(new Vector2(direction * strength, 0), ForceMode2D.Force);
        //     elapsedTime += Time.fixedDeltaTime;
        //     yield return new WaitForFixedUpdate();
        // }
    }

    public virtual void Patrol() {}
    public virtual void Chase() {}
    public virtual void Attack() {}
    public virtual bool PlayerIsInAttackRange() { return false; }
    public virtual bool PlayerIsInDetectRange() { return false; }
    
}
