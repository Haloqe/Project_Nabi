using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyMovement_Flight : EnemyMovement
{
    private float _nextWaypointDistance = 3f;
    private Path _path;
    private int _currentWaypoint = 0;
    private bool _reachedEndOfPath = false;
    private Seeker _seeker;
    public LayerMask _playerLayer;
    public Collider2D _playerDetectCollider;
    private Vector2 _attackDirection;
    
    private void Awake()
    {
        moveType = EEnemyMoveType.Flight;
        _seeker = GetComponent<Seeker>();
        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            _path = p;
            _currentWaypoint = 0;
        }
    }

    private void UpdatePath()
    {
        if (!isActiveAndEnabled) return;
        if (_seeker.IsDone())
            _seeker.StartPath(_rigidBody.position, _enemyBase.Target.transform.position + new Vector3(0f, 4f, 0f), OnPathComplete);
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

    public override void Patrol()
    {
        Vector3 direction = new Vector3(-0.3f * (Mathf.PingPong(Time.time, 2) - 1f), 0.3f * (Mathf.PingPong(Time.time, 2) - 1f), 0);
        Vector3 force = direction * 100f * Time.deltaTime;
        _rigidBody.AddForce(force);
    }

    public override void Chase()
    {
        if (_path == null) return;
        if (_path.GetTotalLength() >= _enemyBase.EnemyData.DetectRangeY) return;

        FlipEnemyTowardsMovement();

        if (_currentWaypoint >= _path.vectorPath.Count)
        {
            _reachedEndOfPath = true;
            return;
        }
        else
        {
            _reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)_path.vectorPath[_currentWaypoint] - _rigidBody.position).normalized;
        Vector2 force = direction * _moveSpeed * Time.deltaTime * 100f;
        _rigidBody.AddForce(force);

        float distance = Vector2.Distance(_rigidBody.position, _path.vectorPath[_currentWaypoint]);
        if (distance < _nextWaypointDistance) _currentWaypoint++;
    }

    public override void Attack()
    {
        FlipEnemyTowardsMovement();
        if (!_animator.GetBool("IsAttacking"))
        {
            _animator.SetBool("IsAttacking", true);
            _attackDirection = ((Vector2)_enemyBase.Target.transform.position - _rigidBody.position).normalized;
            return;
        }
        else
        {
            string currentAnimation = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            if (currentAnimation == "Telegraph")
            {
                // rotate 65 degrees opposite direction to the way it's facing
                // facing left -> anti-clockwise, vice versa
                _rigidBody.AddTorque(1f);
            }
            else if (currentAnimation == "Attack Sequence")
            {
                // rotate back 65 degrees
                _rigidBody.AddTorque(-1f);
                _rigidBody.AddForce(_attackDirection * Time.deltaTime * 800f);
            }
            else if (currentAnimation == "Attack End")
            {
                _rigidBody.AddForce(-_attackDirection * Time.deltaTime * 150f);
            }
            else
            {
                _animator.SetBool("IsAttacking", false);
            }
        }
    }

    public override bool PlayerIsInAttackRange()
    {
        Vector2 playerAttackColliderSize = new Vector2(_enemyBase.EnemyData.AttackRangeX, _enemyBase.EnemyData.AttackRangeY);

        if (Physics2D.OverlapBox(transform.position, playerAttackColliderSize, 0, _playerLayer)) return true;
        else return false;
    }

    public override bool PlayerIsInDetectRange()
    {
        if (Physics2D.OverlapCircle(transform.position, _enemyBase.EnemyData.DetectRangeX, _playerLayer)) return true;
        else return false;
    }

    // void OnDrawGizmos()
    // {
    //     Gizmos.DrawWireSphere(transform.position, _enemyBase.EnemyData.DetectRangeX);
    //     Gizmos.DrawWireCube(transform.position - transform.up, new Vector2 (_enemyBase.EnemyData.AttackRangeX, _enemyBase.EnemyData.AttackRangeY));
    // }
}
