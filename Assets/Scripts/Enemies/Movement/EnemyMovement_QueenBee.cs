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
        SpawnVFXPrefab = Resources.Load("Prefabs/Effects/SpawnPoof");
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
            SpawnMinions(3);
            break;

            case 1:
            _attackTimeCounter += 5f;
            BattleCry();
            break;

            case 2:
            _attackTimeCounter += 5f;
            BodySlam();
            break;

            case 3:
            _attackTimeCounter += 5f;
            PoisonSpray();
            break;
        }
    }

    private void SpawnMinions(int spawnAmount)
    {
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

    private void BattleCry()
    {
        List<GameObject> allBees = (GameObject.FindGameObjectsWithTag("Enemy")).ToList();
        allBees.Remove(gameObject);

        int attackOrDefense = (int)Math.Floor(Random.Range(0.0f, 2f));

        switch (attackOrDefense)
        {
            case 0:
            foreach (GameObject b in allBees)
            {
                ChangeSpeedByPercentage(1.3f);
                _enemyBase.ChangeAttackSpeedByPercentage(1.3f);
                Debug.Log("giving speed");
            }
            break;

            case 1:
            foreach(GameObject b in allBees)
            {
                _enemyBase.ChangeArmourByPercentage(1.3f);
            }
            break;
        }

        StartCoroutine(BuffReset(5f, attackOrDefense, allBees));
    }

    private IEnumerator BuffReset(float buffSeconds, int attackOrDefense, List<GameObject> allBees)
    {
        yield return new WaitForSeconds(buffSeconds);

        switch (attackOrDefense)
        {
            case 0:
            foreach (GameObject b in allBees)
            {
                if (b == null) continue;
                ResetMoveSpeed();
                _enemyBase.ResetAttackSpeed();
                Debug.Log("resetting speed");
            }
            break;

            case 1:
            foreach(GameObject b in allBees)
            {
                if (b == null) continue;
                _enemyBase.ResetArmour();
                Debug.Log("resetting armour");
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
