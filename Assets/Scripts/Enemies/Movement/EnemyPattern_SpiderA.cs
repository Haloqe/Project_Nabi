using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyPattern_SpiderA : EnemyPattern
{
    // Ground detection
    [SerializeField] Vector2 _groundColliderSize;
    public LayerMask _groundLayer;
    public Collider2D _groundInFrontCollider;
    public Collider2D _ceilingInFrontCollider;
    
    // others
    private GameObject _webObject;
    private PlayerDamageReceiver _playerDamageReceiver;
    private bool _isInAttackState = false;
    private bool _isInChaseState = false;
    private float _jumpForce = 10f;
    private float _teethAttackRange = 4f;
    private float _webAttackRange = 5f;
    private List<StatusEffectInfo> _poisonStatusEffect;
    
    // sfx
    [SerializeField] private AudioClip _jumpAudio;
    [SerializeField] private AudioClip _walkAudio;
    [SerializeField] private AudioClip _poisonAudio;
    [SerializeField] private AudioClip _webAudio;
    
    // animation stuff
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsTeethAttacking = Animator.StringToHash("IsTeethAttacking");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int RunMultiplier = Animator.StringToHash("RunMultiplier");

    private void Awake()
    {
        MoveType = EEnemyMoveType.SpiderA;
        _webObject = Resources.Load<GameObject>("Prefabs/Enemies/Spawns/Spider_web");
    }
    
    public override void Init()
    {
        base.Init();
        _playerDamageReceiver = PlayerController.Instance.playerDamageReceiver;
        _poisonStatusEffect = new List<StatusEffectInfo> { new(EStatusEffect.Poison, 1f, 5f) };
    }

    private void WalkForward()
    {
        if (IsRooted) return;
        if (!IsGrounded()) return;
        if (IsAtEdge())
        {
            _animator.SetBool(IsWalking, false);
            return;
        }

        _rigidBody.velocity = transform.localScale.x > Mathf.Epsilon
            ? new Vector2(MoveSpeed, 0f)
            : new Vector2(-MoveSpeed, 0f);
    }

    private void RunTowardsPlayer()
    {
        _animator.SetBool(IsAttacking, false);
        _animator.SetBool(IsWalking, true);
        _animator.SetFloat(RunMultiplier, 2f);
        
        FlipEnemyTowardsTarget();
        WalkForward();
    }

    private void GenerateRandomState()
    {
        if (Random.Range(0.0f, 1.0f) <= _enemyBase.EnemyData.IdleProbability)
        {
            IsMoving = false;
            _enemyBase.ActionTimeCounter =
                Random.Range(_enemyBase.EnemyData.IdleAverageDuration * 0.5f,
                    _enemyBase.EnemyData.IdleAverageDuration * 1.5f);
        } else
        {
            IsMoving = true;
            _enemyBase.ActionTimeCounter =
                Random.Range(_enemyBase.EnemyData.WalkAverageDuration * 0.5f,
                    _enemyBase.EnemyData.WalkAverageDuration * 1.5f);

            if (Random.Range(0.0f, 1.0f) <= 0.3f) FlipEnemy();
        }
    }
    
    public override void Patrol()
    {
        // if (IsAtEdge() && IsChasingPlayer)
        // {
        //     Jump(2f);
        //     return;
        // }
        //
        _animator.SetBool(IsAttacking, false);
        _animator.SetFloat(RunMultiplier, 1f);
        IsChasingPlayer = false;
        if (_enemyBase.ActionTimeCounter <= 0) GenerateRandomState();
        if (IsMoving)
        {
            if (IsAtEdge()) FlipEnemy();
            WalkForward();
        }
        else _rigidBody.velocity = Vector2.zero;
        _animator.SetBool(IsWalking, IsMoving);
    }

    public override void Chase()
    {
        if (_isInChaseState) return;
        if (_enemyBase.ActionTimeCounter <= 0)
        {
            IsChasingPlayer = false;
            return;
        }
        StartCoroutine(JumpTowardsPlayer());
    }

    private IEnumerator JumpTowardsPlayer()
    {
        _isInChaseState = true;
        if (IsAtEdge()) yield break;
        
        int toLeft = -1;
        if (_player.transform.position.x > transform.position.x) toLeft = 1;

        _audioSource.pitch = Random.Range(0.5f, 1.5f);
        _audioSource.PlayOneShot(_jumpAudio);
        
        Jump(5f * toLeft);
        FlipEnemyTowardsTarget();
        yield return new WaitForSeconds(1f);

        _isInChaseState = false;
    }

    public override void Attack()
    {
        if (_isInAttackState) return;
        
        if (_playerDamageReceiver.GetEffectRemainingTimes()[(int)EStatusEffect.Poison] > 0 &&
            _playerDamageReceiver.GetEffectRemainingTimes()[(int)EStatusEffect.Root] <= 0)
        {
            if (!PlayerIsInWebAttackRange())
            {
                RunTowardsPlayer();
                return;
            }
            
            StartCoroutine(WebAttack());
            return;
        }
        
        if (!PlayerIsInTeethAttackRange())
        {
            RunTowardsPlayer();
            return;
        }
        
        StartCoroutine(TeethAttack());
    }

    private void Jump(float distance)
    {
        if (_animator.GetBool(IsJumping)) return;
        _animator.SetBool(IsWalking, false);
        _animator.SetBool(IsJumping, true);
        _rigidBody.velocity = new Vector3(_rigidBody.velocity.x + distance, _jumpForce, 0);
    }
    
    private void DisableJump()
    {
        _animator.SetBool(IsJumping, false);
    }

    private IEnumerator WebAttack()
    {
        _isInAttackState = true;

        _audioSource.pitch = Random.Range(0.5f, 1.5f);
        _audioSource.PlayOneShot(_webAudio);
        
        Jump(0f);
        FlipEnemyTowardsTarget();
        _animator.SetBool(IsWalking, false);
        yield return new WaitForSeconds(0.3f);
        Instantiate(_webObject, transform.position, Quaternion.identity);
        
        while (_animator.GetBool(IsJumping)) yield return null;
        yield return new WaitForSeconds(1.5f);
        
        _isInAttackState = false;
    }

    private IEnumerator TeethAttack()
    {
        _isInAttackState = true;
        
        FlipEnemyTowardsTarget();
        _animator.SetBool(IsWalking, false);
        _animator.SetBool(IsTeethAttacking, true);
        _enemyBase.DamageInfo.StatusEffects = _poisonStatusEffect;

        _audioSource.pitch = Random.Range(0.5f, 1.5f);
        _audioSource.PlayOneShot(_poisonAudio);
        
        yield return new WaitForSeconds(1f);
        
        _animator.SetBool(IsTeethAttacking, false);
        _enemyBase.DamageInfo.StatusEffects = new List<StatusEffectInfo> { };
        // _enemyBase.DamageInfo.Damage = new DamageInfo((EDamageType)Enum.Parse(typeof(EDamageType),
        //     _enemyBase.EnemyData.DamageType), _enemyBase.EnemyData.DefaultDamage);
        
        _isInAttackState = false;
    }

    public override bool PlayerIsInAttackRange() // the 30 pixels mentioned in flowchart
    {
        if (_isInAttackState) return true;
        Vector3 targetPosition = _enemyBase.Target.transform.position;
        return Mathf.Abs(transform.position.x - targetPosition.x) <= _enemyBase.EnemyData.AttackRangeX 
            && Mathf.Abs(targetPosition.y - transform.position.y) <= _enemyBase.EnemyData.AttackRangeY;
    }

    public override bool PlayerIsInDetectRange() // range where it jumps towards player
    {
        Vector3 targetPosition = _enemyBase.Target.transform.position;
        return Mathf.Abs(transform.position.x - targetPosition.x) <= _enemyBase.EnemyData.DetectRangeX 
            && Mathf.Abs(targetPosition.y - transform.position.y) <= _enemyBase.EnemyData.DetectRangeY;
        
        // is it over 30 pixels?
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground")) _animator.SetBool(IsJumping, false);
    }

    private bool PlayerIsInWebAttackRange()
    {
        return Mathf.Abs(transform.position.x - _player.transform.position.x) <= _webAttackRange
            && Mathf.Abs(_player.transform.position.y - transform.position.y) <= _enemyBase.EnemyData.AttackRangeY;
    }

    private bool PlayerIsInTeethAttackRange()
    {
        return Mathf.Abs(transform.position.x - _player.transform.position.x) <= _teethAttackRange 
            && Mathf.Abs(_player.transform.position.y - transform.position.y) <= _enemyBase.EnemyData.AttackRangeY;
    }
    
    private bool IsGrounded()
    {
        return Physics2D.OverlapBox(transform.position, _groundColliderSize, 0, _groundLayer);
    }

    private bool IsAtEdge()
    {
        return _ceilingInFrontCollider.IsTouchingLayers(_groundLayer) ||
               !_groundInFrontCollider.IsTouchingLayers(_groundLayer);
    }
    
    private void OnDrawGizmos()
    {
        // Gizmos.DrawWireCube(transform.position, _groundColliderSize);
        Gizmos.DrawWireCube(transform.position, new Vector3(_webAttackRange, _webAttackRange, 0));
        Gizmos.DrawWireCube(transform.position, new Vector3(_teethAttackRange, _teethAttackRange, 0));
    }
    
}
