using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyMovement : MonoBehaviour
{
    protected EnemyBase _enemyBase;
    protected GameObject _target;
    protected IDamageable _targetDamageable;

    [SerializeField] protected float DefaultMoveSpeed = 1f;
    protected float _moveSpeed;
    protected bool _isRooted = false;
    protected Rigidbody2D _rigidBody;
    protected Animator _animator;
    protected bool _isFlippable = true;

    //the speed at which the enemy will be pulled
    public float smoothing = 1f;
    // private Transform _target = null;
    private GameObject _attacker;

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
        // _rigidBody.velocity = Vector2.zero;
        _moveSpeed = 0;
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
}