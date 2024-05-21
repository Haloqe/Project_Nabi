using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Random = UnityEngine.Random;

public class EnemyMovement_Flight : EnemyMovement
{
    private float _nextWaypointDistance = 3f;
    private Path _path;
    private int _currentWaypoint = 0;
    private Seeker _seeker;
    public LayerMask _playerLayer;
    public LayerMask _platformLayer;
    public Collider2D _playerDetectCollider;
    // private Vector2 _attackDirection;
    private Vector3 _targetPosition;
    private Vector2 _patrolDirection;
    private bool _directionIsChosen = false;
    private bool _directionIsFlipping = false;
    private bool _isSpawnedByQueen = false;
    private bool _isInAttackState = false;
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsInAttackSequence = Animator.StringToHash("IsInAttackSequence");
    private static readonly int AttackSpeed = Animator.StringToHash("AttackSpeed");

    private void Awake()
    {
        MoveType = EEnemyMoveType.Flight;
        _seeker = GetComponent<Seeker>();
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
    }

    private void OnPathComplete(Path p)
    {
        if (p.error) return;
        _path = p;
        _currentWaypoint = 0;
    }

    private void UpdatePath()
    {
        if (!isActiveAndEnabled) return;
        if (_seeker.IsDone())
            _seeker.StartPath(_rigidBody.position, _enemyBase.Target.transform.position + new Vector3(0f, 4f, 0f), OnPathComplete);
    }

    public override void Patrol()
    {
        // Vector3 direction = new Vector3(-0.3f * (Mathf.PingPong(Time.time, 2) - 1f), 0.3f * (Mathf.PingPong(Time.time, 2) - 1f), 0);
        // Vector3 force = direction * 100f * Time.deltaTime;
        // _rigidBody.AddForce(force);

        if (!_directionIsFlipping && Physics2D.OverlapCircle(transform.position, 0.8f, _platformLayer))
            StartCoroutine(nameof(FlipDirection));

        Vector2 force = _patrolDirection * 100f * Time.deltaTime;
        _rigidBody.AddForce(force);
        FlipEnemyTowardsMovement();

        if (_directionIsChosen) return;
        StartCoroutine(nameof(ChooseRandomDirection));
    }

    private IEnumerator ChooseRandomDirection()
    {
        _directionIsChosen = true;
        _patrolDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        yield return new WaitForSeconds(Random.Range(3f, 5f));
        _directionIsChosen = false;
    }

    private IEnumerator FlipDirection()
    {
        _directionIsFlipping = true;
        _patrolDirection = -_patrolDirection;
        yield return new WaitForSeconds(1f);
        _directionIsFlipping = false;
    }
    
    private IEnumerator MoveToPosition(Vector3 destination, float speed, bool facingTarget)
    {
        Vector3 moveDirection = (destination - transform.position).normalized;
        Debug.Log("going through here..." + destination);
        while (!IsCloseEnough(gameObject, destination))
        {
            _rigidBody.velocity = moveDirection * speed;
            if (facingTarget) FlipEnemyTowardsTarget();
            else FlipEnemyTowardsMovement();
            yield return null;
        }
        Debug.Log("successfully reached destination!");
        Debug.Log("target: " + destination + ", rn: " + transform.position);
    }
    
    private bool IsCloseEnough(GameObject obj, Vector3 pos)
    {
        if (!(Vector3.Distance(obj.transform.position, pos) < 0.3f)) return false;
        obj.transform.position = pos;
        return true;
    }

    public override void Chase()
    {
        if (_path == null) return;
        if (_path.GetTotalLength() >= _enemyBase.EnemyData.DetectRangeY) return;

        FlipEnemyTowardsMovement();

        if (_currentWaypoint >= _path.vectorPath.Count) return;

        Vector2 direction = ((Vector2)_path.vectorPath[_currentWaypoint] - _rigidBody.position).normalized;
        Vector2 force = direction * _moveSpeed * Time.deltaTime * 100f;
        _rigidBody.AddForce(force);

        float distance = Vector2.Distance(_rigidBody.position, _path.vectorPath[_currentWaypoint]);
        if (distance < _nextWaypointDistance) _currentWaypoint++;
    }

    // public override void Attack()
    // {
    //     FlipEnemyTowardsMovement();
    //     if (!_animator.GetBool(IsAttacking))
    //     {
    //         _animator.SetBool(IsAttacking, true);
    //         _attackDirection = ((Vector2)_enemyBase.Target.transform.position - _rigidBody.position).normalized;
    //         return;
    //     }
    //     
    //     
    //     string currentAnimation = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
    //
    //     // switch ()
    //     if (currentAnimation == "Telegraph")
    //     {
    //         // rotate 65 degrees opposite direction to the way it's facing
    //         // facing left -> anti-clockwise, vice versa
    //         _rigidBody.AddTorque(1f);
    //     }
    //     else if (currentAnimation == "Attack Sequence")
    //     {
    //         // rotate back 65 degrees
    //         _rigidBody.AddTorque(-1f);
    //         _rigidBody.AddForce(_attackDirection * Time.deltaTime * 800f / _animator.GetFloat(AttackSpeed));
    //         // instead of force, change it to velocity
    //     }
    //     else if (currentAnimation == "Attack End")
    //     {
    //         _rigidBody.AddForce(-_attackDirection * Time.deltaTime * 150f / _animator.GetFloat(AttackSpeed));
    //     }
    //     else
    //     {
    //         _animator.SetBool(IsAttacking, false);
    //     }
    //
    // }
    
    // public override void Attack()
    // {
    //     FlipEnemyTowardsMovement();
    //     if (!_animator.GetBool(IsAttacking))
    //     {
    //         _animator.SetBool(IsAttacking, true);
    //         _attackDirection = ((Vector2)_enemyBase.Target.transform.position - _rigidBody.position).normalized;
    //         return;
    //     }
    //     
    //     string currentAnimation = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
    //     
    //     switch (currentAnimation)
    //     {
    //         case "Telegraph":
    //             _rigidBody.AddTorque(1f);
    //             break;
    //         
    //         case "Attack Sequence":
    //             _rigidBody.velocity = new Vector2(0f, 0f);
    //             _rigidBody.AddTorque(-1f);
    //             StartCoroutine(MoveToPosition(_player.transform.position, _moveSpeed, false));
    //             break;
    //         
    //         case "Attack End":
    //             // _rigidBody.AddForce(-_attackDirection * Time.deltaTime * 150f / _animator.GetFloat(AttackSpeed));
    //             break;
    //         
    //         default:
    //             _animator.SetBool(IsAttacking, false);
    //             break;
    //     }
    // }

    public override void Attack()
    {
        if (_isInAttackState) return;
        
        if (!_animator.GetBool(IsAttacking))
        {
            _animator.SetBool(IsAttacking, true);
            IsAttackingPlayer = true;
            _targetPosition = _enemyBase.Target.transform.position;
            StartCoroutine(Telegraph());
            return;
        }

        if (_animator.GetBool(IsInAttackSequence))
        {
            StartCoroutine(AttackSequence());
            return;
        }

        StartCoroutine(AttackEnd());
    }

    private IEnumerator Telegraph()
    {
        _isInAttackState = true;
        
        int directionFacing = 1;
        if (_targetPosition.x > transform.position.x) directionFacing *= -1;
        Vector3 position = _targetPosition + new Vector3(directionFacing * 3f, 3f, 0);
        yield return MoveToPosition(position, _moveSpeed, true);
        _rigidBody.velocity = new Vector3(0f, 0f, 0f);
        yield return new WaitForSeconds(1f);

        _animator.SetBool(IsInAttackSequence, true);
        IsAttackingPlayer = true;
        _isInAttackState = false;
    }

    private IEnumerator AttackSequence()
    {
        _isInAttackState = true;
        
        _rigidBody.velocity = new Vector3(0f, 0f, 0f);
        Vector3 position = _targetPosition + new Vector3(0f, 0.5f, 0);
        yield return MoveToPosition(position, _moveSpeed * 3f, false);
        
        _animator.SetBool(IsInAttackSequence, false);
        _isInAttackState = false;
    }

    private IEnumerator AttackEnd()
    {
        _isInAttackState = true;
        
        int directionFacing = -1;
        if (_targetPosition.x > transform.position.x) directionFacing *= -1;
        Vector3 position = _targetPosition + new Vector3(directionFacing * 2f, 2f, 0);
        yield return MoveToPosition(position, _moveSpeed, true);

        yield return new WaitForSeconds(3f);
        
        _animator.SetBool(IsAttacking, false);
        IsAttackingPlayer = false;
        _isInAttackState = false;
    }

    public void SendQueenSpawnedInfo()
    {
        _isSpawnedByQueen = true;
    }

    public override bool PlayerIsInAttackRange()
    {
        Vector2 playerAttackColliderSize = new Vector2(_enemyBase.EnemyData.AttackRangeX, _enemyBase.EnemyData.AttackRangeY);

        return Physics2D.OverlapBox(transform.position, playerAttackColliderSize, 0, _playerLayer);
    }

    public override bool PlayerIsInDetectRange()
    {
        if (_isSpawnedByQueen) return true;
        return Physics2D.OverlapCircle(transform.position, _enemyBase.EnemyData.DetectRangeX, _playerLayer);
    }

    // void OnDrawGizmos()
    // {
    //     Gizmos.DrawWireSphere(transform.position, _enemyBase.EnemyData.DetectRangeX);
    //     Gizmos.DrawWireCube(transform.position - transform.up, new Vector2 (_enemyBase.EnemyData.AttackRangeX, _enemyBase.EnemyData.AttackRangeY));
    // }
}
