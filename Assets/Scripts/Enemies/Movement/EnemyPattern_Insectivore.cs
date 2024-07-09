using System.Collections;
using UnityEngine;

public class EnemyPattern_Insectivore : EnemyPattern
{
    SpriteRenderer _spriteRenderer;
    private bool _isHidden = true;
    private GameObject _attackHitbox;
    [SerializeField] private GameObject _bulletObject;
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

    [SerializeField] private AudioClip _shootAudio;
    [SerializeField] private AudioClip _snapAudio;
    [SerializeField] private AudioClip _revealAudio;

    private void Awake()
    {
        MoveType = EEnemyMoveType.Stationary;
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Color color = _spriteRenderer.material.color;
        color.a = 0f;
        _spriteRenderer.material.color = color;
        
        _attackHitbox = transform.Find("AttackHitbox").transform.gameObject;
        Init();
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

    private void PlaySnapSound()
    {
        _audioSource.pitch = Random.Range(1.2f, 1.6f);
        _audioSource.PlayOneShot(_snapAudio);
    }

    private IEnumerator FadeIn()
    {
        Color color = _spriteRenderer.material.color;
        _audioSource.pitch = Random.Range(0.8f, 1.2f);
        _audioSource.PlayOneShot(_revealAudio);
        
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
        _audioSource.pitch = Random.Range(1.4f, 1.8f);
        _audioSource.volume = 0.8f;
        _audioSource.PlayOneShot(_shootAudio);
    }

    private void DisableIsAttacking()
    {
        _animator.SetBool(IsAttacking, false);
    }

    public override bool PlayerIsInAttackRange()
    {
        if (_animator.GetBool(IsAttacking)) return true;
        return Mathf.Abs(transform.position.x - _enemyBase.Target.transform.position.x) <= _enemyBase.EnemyData.AttackRangeX 
            && _enemyBase.Target.transform.position.y - transform.position.y <= _enemyBase.EnemyData.AttackRangeY;
    }

    public override bool PlayerIsInDetectRange()
    {
        return Mathf.Abs(transform.position.x - _enemyBase.Target.transform.position.x) <= _enemyBase.EnemyData.DetectRangeX 
            && _enemyBase.Target.transform.position.y - transform.position.y <= _enemyBase.EnemyData.DetectRangeY;
    }
}
