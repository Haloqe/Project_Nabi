using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyMovement : MonoBehaviour
{
    protected EnemyBase _enemyBase;
    protected GameObject _target;
    protected IDamageable _targetDamageable;
    private GameObject _attacker;
    public EEnemyMoveType moveType;

    [SerializeField] protected float DefaultMoveSpeed = 1f;
    protected float _moveSpeed;
    protected bool _isRooted = false;
    protected bool _isMoving = true;
    protected bool _isOnAir = true;
    protected Rigidbody2D _rigidBody;
    protected Animator _animator;
    protected bool _isFlippable = true;

    protected virtual void Start()
    {
        _target = GameObject.FindWithTag("Player");
        _targetDamageable = _target.gameObject.GetComponent<IDamageable>();
        _animator = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _enemyBase = GetComponent<EnemyBase>();
        _moveSpeed = DefaultMoveSpeed;
        EnableMovement();
    }

    public virtual void ResetMoveSpeed()
    {
        _moveSpeed = DefaultMoveSpeed;
    }

    public void ChangeSpeedByPercentage(float percentage)
    {
        _moveSpeed = DefaultMoveSpeed * percentage;
    }

    protected virtual void FlipEnemy()
    {
        transform.localScale = new Vector2(
            -1 * transform.localScale.x, transform.localScale.y);
    }

    protected virtual void FlipEnemyTowardsTarget()
    {
        if (transform.position.x - _target.transform.position.x >= 0)
        {
            if (transform.localScale.x > Mathf.Epsilon) FlipEnemy();
        } else {
            if (transform.localScale.x < Mathf.Epsilon) FlipEnemy();
        }
    }

    public virtual void EnableMovement()
    {
        _isRooted = false;
        ResetMoveSpeed();
    }

    public virtual void DisableMovement()
    {
        _isRooted = true;
        _animator.SetBool("IsAttacking", false);
        _animator.SetBool("IsWalking", false);
    }

    // for animation events
    protected virtual void DisableFlip()
    {
        _isFlippable = false;
    }

    protected virtual void EnableFlip()
    {
        _isFlippable = true;
    }

    protected Vector2 pullOverallVelocity = Vector2.zero;
    public void StartPullX(int direction, float strength, float duration)
    {
        DisableFlip();
        _isRooted = true;
        _isMoving = false;
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
}