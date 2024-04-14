using System.Collections;
using UnityEngine;

public class EnemyMovement_Stationary : EnemyMovement
{
    SpriteRenderer _spriteRenderer;
    bool _isHidden = true;

    private void Awake()
    {
        MoveType = EEnemyMoveType.Stationary;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Color color = _spriteRenderer.material.color;
        color.a = 0f;
        _spriteRenderer.material.color = color;
    }
    
    public override void Patrol()
    {
        _animator.SetBool("IsAttacking", false);
        if (_enemyBase.ActionTimeCounter <= 0)
        {
            if (Random.Range(0.0f, 1.0f) <= 0.5f)
            {
                FlipEnemy();
                _enemyBase.ActionTimeCounter = Random.Range(_enemyBase.EnemyData.IdleAverageDuration * 0.5f, _enemyBase.EnemyData.IdleAverageDuration * 1.5f);
            }
        }
    }

    public override void Chase()
    {
        if (IsFlippable)
        {
            FlipEnemyTowardsTarget();
            _animator.SetBool("IsAttacking", false);
        }
    }

    public override void Attack()
    {
        if (_isHidden)
        {
            StartCoroutine("FadeIn");
            _isHidden = false;
        }

        if (IsFlippable) FlipEnemyTowardsTarget();
        _animator.SetBool("IsAttacking", true);
    }

    private IEnumerator FadeIn()
    {
        for (float i = 0.05f; i <= 1; i += 0.05f)
        {
            Color color = _spriteRenderer.material.color;
            color.a = i;
            _spriteRenderer.material.color = color;
            yield return new WaitForSeconds(0.05f);
        }
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
