using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class EnemyMovement_QueenBee : EnemyMovement
{
    public LayerMask _playerLayer;
    public LayerMask _platformLayer;
    private Vector2 _attackDirection;
    private Vector2 _patrolDirection;
    private bool _directionIsChosen = false;
    private bool _directionIsFlipping = false;
    private float _attackTimeCounter = 0f;
    private UnityEngine.Object SpawnVFXPrefab;
    
    private void Awake()
    {
        MoveType = EEnemyMoveType.QueenBee;
        SpawnVFXPrefab = Resources.Load("Prefabs/Effects/SpawnPoofVFX");
    }


    public override void Patrol()
    {

    }

    private void Idle()
    {
        Vector3 direction = new Vector3(-0.3f * (Mathf.PingPong(Time.time, 2) - 1f), 0.3f * (Mathf.PingPong(Time.time, 2) - 1f), 0);
        Vector3 force = direction * 100f * Time.deltaTime;
        _rigidBody.AddForce(force);
    }

    private void RandomMovement()
    {
        if (!_directionIsFlipping && Physics2D.OverlapCircle(transform.position, 0.8f, _platformLayer))
            StartCoroutine("FlipDirection");

        Vector2 force = _patrolDirection * 100f * Time.deltaTime;
        _rigidBody.AddForce(force);
        FlipEnemyTowardsMovement();

        if (_directionIsChosen) return;
        StartCoroutine("ChooseRandomDirection");
    }

    private void DefaultMovement()
    {

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

    public override void Attack()
    {
        if (_attackTimeCounter <= 0)
        {
            GenerateRandomAttack();
        }
        _attackTimeCounter -= Time.deltaTime;
    }

    private void GenerateRandomAttack()
    {
        switch (Math.Floor(Random.Range(0.0f, 2f)))
        {
            case 0:
            _attackTimeCounter += 5f;
            StartCoroutine(SpawnMinions(3));
            break;

            case 1:
            _attackTimeCounter += 5f;
            StartCoroutine(BattleCry());
            break;

            case 2:
            _attackTimeCounter += 5f;
            _animator.SetBool("IsAttacking", true);
            _animator.SetInteger("AttackIndex", 2);
            BodySlam();
            break;

            case 3:
            _attackTimeCounter += 5f;
            _animator.SetBool("IsAttacking", true);
            _animator.SetInteger("AttackIndex", 3);
            PoisonSpray();
            break;
        }
    }

    private IEnumerator SpawnMinions(int spawnAmount)
    {
        _animator.SetBool("IsAttacking", true);
        _animator.SetInteger("AttackIndex", 0);
        yield return new WaitForSeconds(1f);
        _animator.SetBool("IsAttacking", false);
        
        Vector3 spawnLocation = transform.position;
        for (int i = 0; i < spawnAmount; i++)
        {
            spawnLocation += new Vector3(Random.Range(0, 3f), Random.Range(0, 3f), 0);
            EnemyManager.Instance.SpawnEnemy(2, spawnLocation, true);
            Instantiate(SpawnVFXPrefab, spawnLocation, Quaternion.identity);
        }
    }

    private void PoisonSpray()
    {
        StartCoroutine(PoisonSprayCoroutine());
    }

    private IEnumerator PoisonSprayCoroutine()
    {
        Vector3 poisonLocation = GameObject.Find("Player").transform.position;
        yield return new WaitForSeconds(2f);
        // change animation state
        Vector3 shootDirection = (poisonLocation - transform.position).normalized;
        Vector3 force = shootDirection * 100f * Time.deltaTime;
        // _rigidBody.AddForce(force);
    }

    private void BodySlam()
    {

    }

    private IEnumerator BattleCry()
    {
        _animator.SetBool("IsAttacking", true);
        _animator.SetInteger("AttackIndex", 1);
        yield return new WaitForSeconds(1f);
        _animator.SetBool("IsAttacking", false);
        
        List<GameObject> allBees = (GameObject.FindGameObjectsWithTag("Enemy")).ToList();
        allBees.Remove(gameObject);

        int attackOrDefense = (int)Math.Floor(Random.Range(0.0f, 2f));

        switch (attackOrDefense)
        {
            case 0:
            foreach (GameObject b in allBees)
            {
                EnemyBase beeBase = b.GetComponent<EnemyBase>();
                EnemyMovement beeMovement = b.GetComponent<EnemyMovement>();
                GameObject attackCommandVFX = b.transform.Find("AttackCommandVFX").gameObject;
                beeMovement.ChangeSpeedByPercentage(1.3f);
                beeBase.ChangeAttackSpeedByPercentage(1.3f);
                attackCommandVFX.SetActive(true);
            }
            break;

            case 1:
            foreach(GameObject b in allBees)
            {
                EnemyBase beeBase = b.GetComponent<EnemyBase>();
                GameObject defenseCommandVFX = b.transform.Find("DefenseCommandVFX").gameObject;
                beeBase.ChangeArmourByPercentage(1.3f);
                defenseCommandVFX.SetActive(true);
            }
            break;
        }

        yield return new WaitForSeconds(5f);

        switch (attackOrDefense)
        {
            case 0:
            foreach (GameObject b in allBees)
            {
                if (b == null) continue;
                EnemyBase beeBase = b.GetComponent<EnemyBase>();
                EnemyMovement beeMovement = b.GetComponent<EnemyMovement>();
                GameObject attackCommandVFX = b.transform.Find("AttackCommandVFX").gameObject;
                
                beeMovement.ResetMoveSpeed();
                beeBase.ResetAttackSpeed();
                attackCommandVFX.SetActive(false);
            }
            break;

            case 1:
            foreach(GameObject b in allBees)
            {
                if (b == null) continue;
                EnemyBase beeBase = b.GetComponent<EnemyBase>();
                GameObject defenseCommandVFX = b.transform.Find("DefenseCommandVFX").gameObject;
                beeBase.ResetArmour();
                defenseCommandVFX.SetActive(false);
            }
            break;
        }
    }


    public override bool PlayerIsInAttackRange()
    {
        return true;
    }

    public override bool PlayerIsInDetectRange()
    {
        return false;
    }

    // void OnDrawGizmos()
    // {
    //     Gizmos.DrawWireSphere(transform.position, _enemyBase.EnemyData.DetectRangeX);
    //     Gizmos.DrawWireCube(transform.position - transform.up, new Vector2 (_enemyBase.EnemyData.AttackRangeX, _enemyBase.EnemyData.AttackRangeY));
    // }
}
