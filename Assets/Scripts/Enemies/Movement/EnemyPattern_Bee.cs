using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Random = UnityEngine.Random;

public class EnemyPattern_Bee : EnemyPattern
{
    private float _nextWaypointDistance = 3f;
    private Path _path;
    private int _currentWaypoint = 0;
    private Seeker _seeker;
    public LayerMask _playerLayer;
    public LayerMask _platformLayer;
    private Vector3 _targetPosition;
    private Vector2 _patrolDirection;
    private bool _directionIsChosen = false;
    private bool _directionIsFlipping = false;
    private bool _isSpawnedByQueen = false;
    private bool _isInAttackState = false;
    private bool _movingToPosition = false;
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsInAttackSequence = Animator.StringToHash("IsInAttackSequence");

    // Attack info
    private AttackInfo _contactAttackInfo;
    private AttackInfo _baseAttackInfo;
    
    [SerializeField] private AudioSource[] _audioSources;
    [SerializeField] private AudioClip _attackAudio;
    [SerializeField] private AudioClip _detectAudio;
    [SerializeField] private AudioClip _chaseAudio;
    [SerializeField] private AudioClip _hitWallAudio;

    private void Awake()
    {
        Init();
        MoveType = EEnemyMoveType.Flight;
        _seeker = GetComponent<Seeker>();
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
    }

    private void Start()
    {
        _contactAttackInfo = _enemyBase.DamageInfo;
        _baseAttackInfo = new AttackInfo()
        {
            Damage = new DamageInfo(EDamageType.Base, 15, 0),
            StatusEffects = new List<StatusEffectInfo>(),
        };
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
            _seeker.StartPath(_rigidBody.position, 
                _enemyBase.Target.transform.position + new Vector3(0f, 4f, 0f), OnPathComplete);
    }

    public override void Patrol()
    {
        _audioSources[0].loop = false;
        if (!_directionIsFlipping && Physics2D.OverlapCircle(transform.position, 0.8f, _platformLayer))
        {
            StartCoroutine(FlipDirection());
        }

        Vector2 force = _patrolDirection * (100f * Time.deltaTime);
        _rigidBody.AddForce(force);
        FlipEnemyTowardsMovement();

        if (_directionIsChosen) return;
        StartCoroutine(ChooseRandomDirection());
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
        _movingToPosition = true;
        
        Vector3 moveDirection = (destination - transform.position).normalized;
        float timer = 0f;
        while (!IsCloseEnough(gameObject, destination) && _movingToPosition && timer <= 1.5f)
        {
            _rigidBody.velocity = moveDirection * speed;
            if (facingTarget) FlipEnemyTowardsTarget();
            else FlipEnemyTowardsMovement();
            yield return null;
            timer += Time.deltaTime;
        }
        
        _rigidBody.velocity = Vector2.zero;
        _movingToPosition = false;
    }
    
    private bool IsCloseEnough(GameObject obj, Vector3 pos)
    {
        if (!(Vector3.Distance(obj.transform.position, pos) < 0.3f)) return false;
        obj.transform.position = pos;
        return true;
    }

    public override void Chase()
    {
        if (_path == null || _path.GetTotalLength() >= _enemyBase.EnemyData.DetectRangeY)
        {
            Patrol();
            return;
        }

        FlipEnemyTowardsMovement();

        if (_currentWaypoint >= _path.vectorPath.Count)
        {
            return;
        }

        Vector2 direction = ((Vector2)_path.vectorPath[_currentWaypoint] - _rigidBody.position).normalized;
        direction += new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
        Vector2 force = direction * (MoveSpeed * Time.deltaTime * 160f);
        _rigidBody.AddForce(force);

        float distance = Vector2.Distance(_rigidBody.position, _path.vectorPath[_currentWaypoint]);
        if (distance < _nextWaypointDistance) _currentWaypoint++;
    }

    public override void Attack()
    {
        if (_isInAttackState) return;
        
        _animator.SetBool(IsAttacking, true);
        IsAttackingPlayer = true;
        _targetPosition = _enemyBase.Target.transform.position;
        StartCoroutine(Telegraph());
        
        
        // if (!_animator.GetBool(IsAttacking))
        // {
        //     _animator.SetBool(IsAttacking, true);
        //     IsAttackingPlayer = true;
        //     _targetPosition = _enemyBase.Target.transform.position;
        //     StartCoroutine(Telegraph());
        //     return;
        // }

        // if (_animator.GetBool(IsInAttackSequence))
        // {
        //     StartCoroutine(AttackSequence());
        //     return;
        // }
        //
        // StartCoroutine(AttackEnd());
    }

    private IEnumerator Telegraph()
    {
        _isInAttackState = true;
        
        _audioSources[1].PlayOneShot(_detectAudio);
        _audioSources[0].loop = true;
        _audioSources[0].volume = 0.1f;
        _audioSources[0].pitch = Random.Range(0.9f, 1.1f);
        _audioSources[0].clip = _chaseAudio;
        _audioSources[0].Play();
        
        int directionFacing = 1;
        if (_targetPosition.x > transform.position.x) directionFacing *= -1;
        Vector3 position = _targetPosition + new Vector3(directionFacing * 3f, 3f, 0);
        position += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
        yield return MoveToPosition(position, MoveSpeed, true);
        _rigidBody.velocity = Vector3.zero;
        yield return new WaitForSeconds(0.2f);

        _animator.SetBool(IsInAttackSequence, true);
        IsAttackingPlayer = true;
        StartCoroutine(AttackSequence());
        // _isInAttackState = false;
    }

    private IEnumerator AttackSequence()
    {
        // _isInAttackState = true;
        _enemyBase.UpdateAttackInfo(_baseAttackInfo);
        _rigidBody.velocity = new Vector3(0f, 0f, 0f);
        Vector3 position = _targetPosition + new Vector3(0f, 0.5f, 0);

        _audioSources[0].loop = false;
        _audioSources[1].pitch = Random.Range(0.8f, 1.2f);
        _audioSources[1].PlayOneShot(_attackAudio);
        yield return MoveToPosition(position, MoveSpeed * 3.5f, false);
        
        _enemyBase.UpdateAttackInfo(_contactAttackInfo);
        _animator.SetBool(IsInAttackSequence, false);
        StartCoroutine(AttackEnd());
        // _isInAttackState = false;
    }

    private IEnumerator AttackEnd()
    {
        // _isInAttackState = true;
        
        int directionFacing = -1;
        if (_targetPosition.x > transform.position.x) directionFacing *= -1;
        Vector3 position = transform.position + new Vector3(directionFacing * 2f, 2f, 0);
        position += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
        yield return MoveToPosition(position, MoveSpeed, true);
        
        yield return new WaitForSeconds(1.5f);
        
        _animator.SetBool(IsAttacking, false);
        _isInAttackState = false;
        IsAttackingPlayer = false;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_animator.GetBool(IsAttacking))
        {
            _movingToPosition = false;
            _audioSources[1].pitch = Random.Range(0.8f, 1.2f);
            _audioSources[1].PlayOneShot(_hitWallAudio);
        }

        Vector2 hitPoint = other.GetContact(0).point;
        Vector3 moveDirection = transform.position - new Vector3(hitPoint.x, hitPoint.y, 0);
        Vector3 force = moveDirection * (MoveSpeed * Time.deltaTime * 160f);
        _rigidBody.AddForce(force);
    }

    public void SendQueenSpawnedInfo()
    {
        _isSpawnedByQueen = true;
    }

    public override bool PlayerIsInAttackRange()
    {
        Vector2 playerAttackColliderSize =
            new Vector2(_enemyBase.EnemyData.AttackRangeX, _enemyBase.EnemyData.AttackRangeY);

        return Physics2D.OverlapBox(transform.position, playerAttackColliderSize, 0, _playerLayer);
    }

    public override bool PlayerIsInDetectRange()
    {
        if (_isSpawnedByQueen) return true;
        if (IsAttackingPlayer) return true;
        return Physics2D.OverlapCircle(transform.position, _enemyBase.EnemyData.DetectRangeX, _playerLayer);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _enemyBase.EnemyData.DetectRangeX);
        // Gizmos.DrawWireSphere(transform.position, 0.8f);
        Gizmos.DrawWireCube(transform.position,
        new Vector2 (_enemyBase.EnemyData.AttackRangeX, _enemyBase.EnemyData.AttackRangeY));
    }
}
