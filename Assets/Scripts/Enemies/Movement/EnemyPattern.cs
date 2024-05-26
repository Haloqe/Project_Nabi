using System.Collections;
using UnityEngine;

public abstract class EnemyPattern : MonoBehaviour
{
    protected EnemyBase _enemyBase;
    protected GameObject _player;
    public EEnemyMoveType MoveType;
    public float _moveSpeed;
    public bool IsRooted = false;
    public bool IsMoving = true;
    public bool IsChasingPlayer = false;
    public bool IsAttackingPlayer = false;
    public bool IsFlippable = true;
    protected Rigidbody2D _rigidBody;
    protected Animator _animator;
    private readonly static int Rooted = Animator.StringToHash("IsRooted");

    //used for pull
    private Vector2 _pullOverallVelocity = Vector2.zero;
    Vector2 pullForce;
    public float influenceRange;
    public float distanceToGravField;
    private readonly static int IsAttacking = Animator.StringToHash("IsAttacking");

    public virtual void Init()
    {
        _animator = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _enemyBase = GetComponent<EnemyBase>();
        _player = GameObject.FindWithTag("Player");
        _moveSpeed = _enemyBase.EnemyData.DefaultMoveSpeed;
        EnableMovement();
    }

    public void StopMovementCoroutines()
    {
        StopAllCoroutines();
    }

    public void ResetMoveSpeed()
    {
        _moveSpeed = _enemyBase.EnemyData.DefaultMoveSpeed;
    }

    public void ChangeSpeedByPercentage(float percentage)
    {
        _moveSpeed = _enemyBase.EnemyData.DefaultMoveSpeed * percentage;
    }

    protected void FlipEnemy()
    {
        transform.localScale = new Vector2(
            -1 * transform.localScale.x, transform.localScale.y);
    }

    protected void FlipEnemyTowardsMovement()
    {
        if (_rigidBody.velocity.x <= 0)
        {
            if (transform.localScale.x > 0) FlipEnemy();
        } else {
            if (transform.localScale.x < 0) FlipEnemy();
        }

    }

    protected void FlipEnemyTowardsTarget()
    {
        if (transform.position.x - _enemyBase.Target.transform.position.x >= 0)
        {
            if (transform.localScale.x > 0) FlipEnemy();
        } else {
            if (transform.localScale.x < 0) FlipEnemy();
        }
    }

    public virtual void EnableMovement()
    {
        IsRooted = false;
        _animator.SetBool(Rooted, false);
        EnableFlip();
        // ResetMoveSpeed();
    }

    public void DisableMovement()
    {
        IsRooted = true;
        _animator.SetBool(Rooted, true);
        DisableFlip();
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
        if (_animator != null) _animator.SetBool(IsAttacking, false);
    }

    public void StartPullX(int direction, float strength, float duration)
    {
        DisableFlip();
        IsRooted = true;
        IsMoving = false;
        _pullOverallVelocity += new Vector2(direction * strength, 0);
        StartCoroutine(PullXCoroutine(direction, strength, duration));
    }

    // Currently only considers the x axis; need to change if needed in future
    protected virtual IEnumerator PullXCoroutine(int direction, float strength, float duration)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            _rigidBody.velocity = _pullOverallVelocity;
            elapsedTime += Time.fixedUnscaledDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        _pullOverallVelocity -= new Vector2(direction * strength, 0);
        _rigidBody.velocity = _pullOverallVelocity;
        if (_pullOverallVelocity == Vector2.zero) EnableFlip();
    }

    public void StartGravityPull(Vector3 gravCorePosition, float strength, float duration)
    {
        DisableFlip();
        StartCoroutine(GravityPullCoroutine(gravCorePosition, strength, duration));
    }

    // gravCorePosition = fetch the direction of the gravity Field 
    // strength, duration = constant
    protected virtual IEnumerator GravityPullCoroutine(Vector3 gravCorePosition, float strength, float duration)
    {
        distanceToGravField = Vector2.Distance(gravCorePosition, _rigidBody.position);

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            pullForce = ((Vector2)(gravCorePosition) - _rigidBody.position).normalized / distanceToGravField * strength;
            if(MoveType == EEnemyMoveType.Flight)
            {
                _rigidBody.AddForce(pullForce, ForceMode2D.Force);
            }
            else
            {
                pullForce.y = 0;
                _rigidBody.AddForce(pullForce, ForceMode2D.Force);
            }
            elapsedTime += Time.fixedUnscaledDeltaTime;
            yield return new WaitForFixedUpdate();

        }
    }

    public virtual void Patrol() {}
    public virtual void Chase() {}
    public virtual void Attack() {}
    public virtual bool PlayerIsInAttackRange() { return false; }
    public virtual bool PlayerIsInDetectRange() { return false; }
}
