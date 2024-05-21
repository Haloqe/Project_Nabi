using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


public class EnemyMovement_QueenBee : EnemyMovement
{
    // public LayerMask _platformLayer;
    // private Vector2 _attackDirection;
    // private Vector2 _patrolDirection;
    // private bool _directionIsChosen;
    // private bool _directionIsFlipping;
    private bool _isBouncing = true;
    private bool _lessThanTwoBees;
    private bool _beesAreCommanded;
    private bool _isInAttackSequence;
    private bool _justFinishedAttack = true;
    private UnityEngine.Object _spawnVFXPrefab;
    private GameObject _bombObject;
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int AttackIndex = Animator.StringToHash("AttackIndex");

    private void Awake()
    {
        MoveType = EEnemyMoveType.QueenBee;
        _spawnVFXPrefab = Resources.Load("Prefabs/Effects/SpawnPoofVFX");
        _bombObject = Resources.Load<GameObject>("Prefabs/Enemies/Spawns/QueenBee_bomb");
    }

    public override void Init()
    {
        base.Init();
        StartCoroutine(Bounce());
    }

    public override void Attack()
    {
        if (_isInAttackSequence) return;
        
        if (_justFinishedAttack)
        {
            StartCoroutine(Idle());
            return;
        }
        
        GameObject[] allBees = GameObject.FindGameObjectsWithTag("Enemy");
        _lessThanTwoBees = allBees.Length <= 2;
        if (_lessThanTwoBees)
        {
            StartCoroutine(SpawnMinions(3));
            return;
        }
        
        if (!_beesAreCommanded)
        {
            StartCoroutine(BattleCry());
            return;
        }
        
        // BodySlam or PoisonBomb
        StartCoroutine(Generate3RandomAttacks());
    }
    
    private IEnumerator Generate3RandomAttacks()
    {
        _isInAttackSequence = true;
        
        for (int i = 0; i <= 2; i++)
        {
            switch (Math.Floor(Random.Range(0f, 2f)))
            {
                case 0:
                yield return BodySlam();
                break;

                case 1:
                yield return PoisonBomb();
                break;
            }
        }

        _justFinishedAttack = true;
        _isInAttackSequence = false;
    }

    private IEnumerator MoveToPosition(Vector3 destination, float speed, bool facingTarget = true)
    {
        Vector3 moveDirection = (destination - transform.position).normalized;
        while (!IsCloseEnough(gameObject, destination))
        {
            _rigidBody.velocity = moveDirection * speed;
            if (facingTarget) FlipEnemyTowardsTarget();
            else FlipEnemyTowardsMovement();
            yield return null;
        }
    }

    private IEnumerator MoveToAttackPosition()
    {
        int directionFacing = 1;
        if (_player.transform.position.x > transform.position.x) directionFacing *= -1;
        Vector3 position = _player.transform.position + new Vector3(directionFacing * 10f, 2f, 0);
        yield return MoveToPosition(position, _moveSpeed);
    }

    private bool IsCloseEnough(GameObject obj, Vector3 pos)
    {
        if (!(Vector3.Distance(obj.transform.position, pos) < 0.3f)) return false;
        obj.transform.position = pos;
        return true;
    }
    
    private IEnumerator Idle()
    {
        _isInAttackSequence = true;
        _animator.SetBool(IsAttacking, false);
        
        int directionFacing = 1;
        if (_player.transform.position.x > transform.position.x) directionFacing *= -1;
        Vector3 idlePosition = _player.transform.position + new Vector3(directionFacing * 7f, 2f, 0);
        yield return MoveToPosition(idlePosition, _moveSpeed);
        yield return new WaitForSeconds(3f);
        
        // float timeCounter = 0f;
        // while (timeCounter <= 3f)
        // {
        //     Vector3 moveDirection = (_player.transform.position
        //         + new Vector3(directionFacing * 6f, 6f, 0)
        //         - transform.position).normalized;
        //     _rigidBody.velocity = moveDirection * _moveSpeed;
        //     FlipEnemyTowardsTarget();
        //     timeCounter += Time.deltaTime;
        //     yield return null;
        // }
        
        _justFinishedAttack = false;
        _isInAttackSequence = false;
    }

    private IEnumerator Bounce()
    {
        float speed = 3f;
        int direction = 1;
        while (_isBouncing)
        {
            var force = new Vector2(0f, direction * speed * 0.5f);
            _rigidBody.AddForce(force, ForceMode2D.Impulse);
            yield return new WaitForSeconds(3f / speed);
            direction *= -1;
        }
    }

    // private void RandomMovement()
    // {
    //     if (!_directionIsFlipping && Physics2D.OverlapCircle(transform.position, 0.8f, _platformLayer))
    //         StartCoroutine(nameof(FlipDirection));
    //
    //     Vector2 force = _patrolDirection * 100f * Time.deltaTime;
    //     _rigidBody.AddForce(force);
    //     FlipEnemyTowardsMovement();
    //
    //     if (_directionIsChosen) return;
    //     StartCoroutine(nameof(ChooseRandomDirection));
    // }
    //
    // private void DefaultMovement()
    // {
    //
    // }
    //
    // private IEnumerator ChooseRandomDirection()
    // {
    //     _directionIsChosen = true;
    //     _patrolDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    //     yield return new WaitForSeconds(Random.Range(3f, 5f));
    //     _directionIsChosen = false;
    // }
    //
    // private IEnumerator FlipDirection()
    // {
    //     _directionIsFlipping = true;
    //     _patrolDirection = -_patrolDirection;
    //     yield return new WaitForSeconds(1f);
    //     _directionIsFlipping = false;
    // }

    private IEnumerator SpawnMinions(int spawnAmount)
    {
        _isInAttackSequence = true;
        
        yield return MoveToAttackPosition();
        yield return new WaitForSeconds(1f);
        _animator.SetBool(IsAttacking, true);
        _animator.SetInteger(AttackIndex, 1);
        yield return new WaitForSeconds(1f);
        _animator.SetBool(IsAttacking, false);
        
        Vector3 spawnLocation = transform.position;
        for (int i = 0; i < spawnAmount; i++)
        {
            spawnLocation += new Vector3(Random.Range(0, 3f), Random.Range(0, 3f), 0);
            EnemyManager.Instance.SpawnEnemy(2, spawnLocation, true).GetComponent<EnemyMovement_Flight>().SendQueenSpawnedInfo();
            Instantiate(_spawnVFXPrefab, spawnLocation, Quaternion.identity);
        }

        _justFinishedAttack = true;
        _isInAttackSequence = false;
    }

    private IEnumerator PoisonBomb()
    {
        yield return MoveToAttackPosition();
        yield return new WaitForSeconds(1f);
        _animator.SetBool(id: IsAttacking, true);
        _animator.SetInteger(AttackIndex, 3);
        yield return new WaitForSeconds(1f);
        _animator.SetBool(IsAttacking, false);

        float gap = 4f;
        float playerPositionX = _player.transform.position.x;
        Vector3[] bombPositions = {
            new(playerPositionX - gap, -7f, 0),
            new(playerPositionX, -7f, 0),
            new(playerPositionX + gap, -7f, 0)
        };

        for (int i = 0; i <= 2; i++)
        {
            Instantiate(_bombObject, bombPositions[i], Quaternion.identity);
        }
    }

    private IEnumerator BodySlam()
    {
        _isBouncing = false;
        
        int directionFacing = 1;
        if (_player.transform.position.x > transform.position.x) directionFacing *= -1;
        Vector3 startPosition = _player.transform.position + new Vector3(directionFacing * 7f, 0, 0);
        Vector3 finalPosition = startPosition - new Vector3(directionFacing * 5f, 0, 0);
        
        yield return MoveToPosition(startPosition, _moveSpeed * 1.3f);
        _rigidBody.velocity = new Vector2(0f, 0f);
        _animator.SetBool(IsAttacking, true);
        _animator.SetInteger(AttackIndex, 2);
        yield return new WaitForSeconds(2f);
        
        float dashSpeed = _moveSpeed * 3.5f;
        yield return MoveToPosition(finalPosition, dashSpeed, false);

        _animator.SetBool(IsAttacking, false);
        yield return new WaitForSeconds(2f);

        _isBouncing = true;
        StartCoroutine(Bounce());
    }

    private IEnumerator BattleCry()
    {
        _isInAttackSequence = true;
        
        yield return MoveToAttackPosition();
        yield return new WaitForSeconds(1f);
        _animator.SetBool(IsAttacking, true);
        _animator.SetInteger(AttackIndex, 1);
        yield return new WaitForSeconds(1f);
        _animator.SetBool(IsAttacking, false);
        
        List<GameObject> allBees = GameObject.FindGameObjectsWithTag("Enemy").ToList();
        allBees.Remove(gameObject);

        int attackOrDefense = (int)Math.Floor(Random.Range(0.0f, 2f));
        _beesAreCommanded = true;

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

        _justFinishedAttack = true;
        _isInAttackSequence = false;
        
        yield return new WaitForSeconds(15f);
        _beesAreCommanded = false;

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
