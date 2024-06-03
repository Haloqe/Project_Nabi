using System.Collections;
using UnityEngine;

public class EnemyPattern_Insectivore : EnemyPattern
{
    SpriteRenderer _spriteRenderer;
    private bool _isHidden = true;
    private GameObject _attackHitbox;
    private GameObject _bulletObject;
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

    private void Awake()
    {
        MoveType = EEnemyMoveType.Stationary;
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Color color = _spriteRenderer.material.color;
        color.a = 0f;
        _spriteRenderer.material.color = color;
        
        _attackHitbox = transform.Find("AttackHitbox").transform.gameObject;
        _bulletObject = Resources.Load<GameObject>("Prefabs/Enemies/Spawns/Insectivore_bullet");
    }
    
    public override void Patrol()
    {
        _animator.SetBool(IsAttacking, false);
        if (_enemyBase.ActionTimeCounter > 0) return;
        if (Random.Range(0.0f, 1.0f) > 0.5f) return;
        FlipEnemy();
        _enemyBase.ActionTimeCounter = Random.Range(_enemyBase.EnemyData.IdleAverageDuration * 0.5f,
            _enemyBase.EnemyData.IdleAverageDuration * 1.5f);
    }

    public override void Chase()
    {
        if (_isHidden || !IsFlippable) return;
        // if (!_isShooting) StartCoroutine(ShootBullet());
        _animator.SetTrigger("Shoot");
        FlipEnemyTowardsTarget();
        _animator.SetBool(IsAttacking, false);
    }

    public override void Attack()
    {
        if (_isHidden)
        {
            StartCoroutine(nameof(FadeIn));
            _isHidden = false;
        }

        if (IsFlippable) FlipEnemyTowardsTarget();
        _animator.SetBool(IsAttacking, true);
    }

    private IEnumerator FadeIn()
    {
        Color color = _spriteRenderer.material.color;
        while (color.a < 0.8f)
        {
            color.a += 0.6f * Time.deltaTime;
            _spriteRenderer.material.color = color;
            yield return null;
        }

        _attackHitbox.SetActive(true);
        
        while (color.a < 1)
        {
            color.a += 0.6f * Time.deltaTime;
            _spriteRenderer.material.color = color;
            yield return null;
        }
    }

    private void ShootBullet()
    {
        var bullet = Instantiate(_bulletObject,
            transform.position + new Vector3(Mathf.Sign(transform.localScale.x), 3f, 0),
            Quaternion.identity).GetComponent<Insectivore_Bullet>();
        bullet.Shoot(new Vector3(transform.localScale.x, 0, 0));
    }

    public override bool PlayerIsInAttackRange()
    {
        return Mathf.Abs(transform.position.x - _enemyBase.Target.transform.position.x) <= _enemyBase.EnemyData.AttackRangeX 
            && _enemyBase.Target.transform.position.y - transform.position.y <= _enemyBase.EnemyData.AttackRangeY;
    }

    public override bool PlayerIsInDetectRange()
    {
        return Mathf.Abs(transform.position.x - _enemyBase.Target.transform.position.x) <= _enemyBase.EnemyData.DetectRangeX 
            && _enemyBase.Target.transform.position.y - transform.position.y <= _enemyBase.EnemyData.DetectRangeY;
    }
}
