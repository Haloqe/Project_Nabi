using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyMovement : MonoBehaviour
{
    protected EnemyBase _enemyBase;
    protected SDamageInfo _damageBaseTEMP;
    protected GameObject _target;
    protected IDamageable _targetDamageable;

    [SerializeField] protected float DefaultMoveSpeed = 1f;
    protected float _moveSpeed;
    protected bool _isRooted = false;
    protected Rigidbody2D _rigidBody;
    protected Animator _animator;

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
        _damageBaseTEMP = new SDamageInfo
        {
            Damages = new List<SDamage>() { new SDamage(EDamageType.Base, 15) },
            StatusEffects = new List<SStatusEffect>(),
        };
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

    public virtual void EnableMovement()
    {
        _isRooted = false;
    }

    public virtual void DisableMovement()
    {
        _isRooted = true;
        _rigidBody.velocity = Vector2.zero;
    }

    // public void EnablePulling(float pullDuration)
    // {
    //     StartCoroutine(OnPull(pullDuration));
    // }

    // IEnumerator OnPull(float pullDuration)
    // {
    //     //everything needs to be done on pull:
    //     //could change the pulling time later - hardcoded for now
    //     float remainingTime = pullDuration;
    //     float deltaTime = Time.deltaTime;

    //     while (remainingTime > 0)
    //     {
    //         transform.position = Vector3.Lerp(transform.position, _target.position, smoothing * Time.deltaTime);
    //         remainingTime -= deltaTime;
    //         yield return null;
    //     }

    //     Debug.Log("Pulling finished.");
    // }
}