using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : EnemyBase
{
    //----------------------------------------------------
    // TEMP CODE TO TEST DAMAGE SYSTEM
    // Attack every 5 seconds
    private Transform _target = null;
    private float _skillLifeTime = 1f;
    [SerializeField] float AttackInterval = 5;
    [SerializeField] bool ShouldAttack = false;
    [SerializeField] float DamageAmount = 3;
    [SerializeField] public EStatusEffect StatusEffect;
    [SerializeField] public float EffectStrength;
    [SerializeField] public float EffectDuration;
    private GameObject _attacker;
    private int _attackCount = 0;
    private SDamageInfo _damageInfo;

    protected override void Initialise()
    {
        _target = PlayerController.Instance.transform;
        _attacker = transform.Find("AttackTEMP").gameObject;
        _attacker.SetActive(false);
        _damageInfo = new SDamageInfo
        {
            DamageSource = gameObject.GetInstanceID(),
            Damages = new List<SDamage>() { new SDamage(EDamageType.Base, DamageAmount, 0) },
            StatusEffects = new List<SStatusEffect> { new SStatusEffect(StatusEffect, EffectStrength, EffectDuration) }
        };
        //StartCoroutine(ChaseRoutine());
        if (ShouldAttack) StartCoroutine(AttackRoutine());
    }

    protected override void FixedUpdate()
    {
        if (_target == null)
        {
            StopAllCoroutines();
            _attacker.SetActive(false);
        }
    }

    // TEMP
    IEnumerator ChaseRoutine()
    {
        WaitForSeconds chaseWait = new WaitForSeconds(2.5f);
        while (true)
        {
            while (Vector3.Distance(transform.position, _target.position) > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, _target.position, 0.65f * Time.deltaTime);
                yield return null;
            }
            yield return chaseWait;
        }
    }

    // TEMP
    IEnumerator AttackRoutine()
    {
        WaitForSeconds attackWait = new WaitForSeconds(AttackInterval);
        while (true)
        {
            StartCoroutine(Attack());
            yield return attackWait;
        }
    }

    // TEMP
    IEnumerator Attack()
    {
        _attackCount = 0;
        _attacker.SetActive(true);
        yield return new WaitForSeconds(_skillLifeTime);
        _attacker.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 이런식으로 하면 안 됨
        // 난 테스트용이고 충돌한 대상을 확신하니까 이런식으로 primitive check만 한거야
        if (_attackCount++ != 0) return;
        if (collision.gameObject.CompareTag("Player") == false) return;
        DealDamage(collision.gameObject.GetComponent<IDamageable>(), _damageInfo);
    }

    //----------------------------------------------------
}
