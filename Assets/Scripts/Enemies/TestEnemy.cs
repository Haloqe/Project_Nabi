using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : EnemyBase
{
    //----------------------------------------------------
    // TEMP CODE TO TEST DAMAGE SYSTEM
    // Attack every 5 seconds
    private Transform _target = null;
    private IDamageable _targetDamageable = null;
    private float _skillLifeTime = 1f;
    [SerializeField] float AttackInterval = 5;
    [SerializeField] bool ShouldAttack = false;

    [Header("Damage")]
    [SerializeField] private EDamageType DamageType;
    [SerializeField] float DamageAmount = 3;
    [SerializeField] float DamageDuration = 0;
    [SerializeField] float DamageTick = 0;

    [Header("Status Effect")]
    [SerializeField] public EStatusEffect StatusEffect;
    [SerializeField] public float EffectStrength;
    [SerializeField] public float EffectDuration;

    private GameObject _attacker;
    private int _attackCount = 0;
    private AttackInfo _attackInfo;
    private bool _isAttackDOTInArea = false;
    private Coroutine _activeDOTRoutine = null;

    protected override void Initialise()
    {
        _target = PlayerController.Instance.transform;
        _targetDamageable = _target.gameObject.GetComponent<IDamageable>();
        _attacker = transform.Find("AttackRange").gameObject;
        _attackInfo = new AttackInfo
        {
            Damages = new List<DamageInfo>() { new DamageInfo(DamageType, DamageAmount, DamageDuration, DamageTick) },
            StatusEffects = new List<StatusEffectInfo> { new StatusEffectInfo(StatusEffect, EffectStrength, EffectDuration) }
        };
        // TEMP
        foreach (var damage in _attackInfo.Damages)
        {
            if (damage.Duration == 0 && damage.Tick != 0)
            {
                _isAttackDOTInArea = true;
                break;
            }
        }

        //StartCoroutine(ChaseRoutine());
        //if (ShouldAttack) StartCoroutine(AttackRoutine());
       _attacker.SetActive(ShouldAttack); 
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
            while (Vector3.Distance(transform.position, _target.transform.position) > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, _target.position, 0.65f * Time.deltaTime);
                yield return null;
            }
            yield return chaseWait;
        }
    }

    // TEMP
    IEnumerator DOTAttackRoutine()
    {
        while (true)
        {
            // Wait for a tick time and take damage repeatedly
            DealDamage(_targetDamageable, _attackInfo);
            yield return new WaitForSeconds(_attackInfo.Damages[0].Tick);
        }
    }

    //// TEMP Attack On/Off
    //IEnumerator AttackRoutine()
    //{
    //    WaitForSeconds attackWait = new WaitForSeconds(AttackInterval);
    //    while (true)
    //    {
    //        StartCoroutine(Attack());
    //        yield return attackWait;
    //    }
    //}

    //// TEMP
    //IEnumerator Attack()
    //{
    //    _attackCount = 0;
    //    _attacker.SetActive(true);
    //    yield return new WaitForSeconds(_skillLifeTime);
    //    _attacker.SetActive(false);
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // TEMP primitive check
        //if (_attackCount++ != 0) return;
        if (collision.gameObject.CompareTag("Player") == false) return;
        if (_isAttackDOTInArea && _activeDOTRoutine == null)
        {
            Debug.Log("START DOT");
            _activeDOTRoutine = StartCoroutine(DOTAttackRoutine());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") == false) return;
        Debug.Log("STOP DOT");
        if (_isAttackDOTInArea && _activeDOTRoutine != null)
        {
            StopCoroutine(_activeDOTRoutine);
            _activeDOTRoutine = null;
        }
    }



    //----------------------------------------------------
}
