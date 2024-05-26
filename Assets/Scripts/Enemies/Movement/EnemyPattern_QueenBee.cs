using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


public class EnemyPattern_QueenBee : EnemyPattern
{
    private bool _isBouncing = true;
    private bool _beesAreCommanded;
    private bool _isInAttackSequence;
    private bool _justFinishedAttack = true;
    private UnityEngine.Object _spawnVFXPrefab;
    private GameObject _bombObject;
    private EnemyManager _enemyManager;
    private Vector3[] _bombPositions = new Vector3[13];
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
        _enemyManager = EnemyManager.Instance;

        float gap = 3f;
        for (int i = 0; i < _bombPositions.Length; i++)
        {
            _bombPositions[i] = new Vector3(-19.5f + gap * i, -7f, 0);
        }
    }

    public override void Attack()
    {
        if (_isInAttackSequence) return;
        
        if (_justFinishedAttack)
        {
            StartCoroutine(Idle());
            return;
        }
        
        EnemyPattern_Bee[] allBees = FindObjectsOfType<EnemyPattern_Bee>();
        bool lessThanTwoBees = allBees.Length <= 1;
        if (lessThanTwoBees)
        {
            StartCoroutine(SpawnMinions(Mathf.FloorToInt(Random.Range(3, 6))));
            return;
        }
        
        if (!_beesAreCommanded)
        {
            StartCoroutine(BattleCry());
            return;
        }
        
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
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -19.69f, 14.99f),
            Mathf.Clamp(transform.position.y, -6.79f, 7.99f), 0f);
        Vector3 clampedDestination = new Vector3(Mathf.Clamp(destination.x, -19.7f, 15f), Mathf.Clamp(destination.y, -6.8f, 8f), 0);
        Vector3 moveDirection = (clampedDestination - transform.position).normalized;
        while (!IsCloseEnough(gameObject, clampedDestination)
               && transform.position.x is >= -19.7f and <= 15f
               && transform.position.y is >= -6.8f and <= 8f)
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

    private IEnumerator SpawnMinions(int spawnAmount)
    {
        _isInAttackSequence = true;
        
        yield return MoveToAttackPosition();
        yield return new WaitForSeconds(1f);
        _animator.SetBool(IsAttacking, true);
        _animator.SetInteger(AttackIndex, 1);
        yield return new WaitForSeconds(1f);
        _animator.SetBool(IsAttacking, false);
        
        Vector3 spawnLocation = _player.transform.position;
        for (int i = 0; i < spawnAmount; i++)
        {
            spawnLocation += new Vector3(Random.Range(0, 3f), Random.Range(0, 3f), 0);
            _enemyManager.SpawnEnemy(2, spawnLocation, true).GetComponent<EnemyPattern_Bee>().SendQueenSpawnedInfo();
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

        foreach (Vector3 position in _bombPositions)
        {
            Instantiate(_bombObject, position, Quaternion.identity);
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
        yield return new WaitForSeconds(0.8f);
        
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
        
        EnemyPattern_Bee[] allBeesScript = FindObjectsOfType<EnemyPattern_Bee>();
        GameObject[] allBees = new GameObject[allBeesScript.Length];
        for (int i = 0; i < allBeesScript.Length; i++)
        {
            allBees[i] = allBeesScript[i].gameObject;
        }

        int attackOrDefense = (int)Math.Floor(Random.Range(0.0f, 2f));
        _beesAreCommanded = true;

        switch (attackOrDefense)
        {
            case 0:
            foreach (GameObject b in allBees)
            {
                EnemyBase beeBase = b.GetComponent<EnemyBase>();
                EnemyPattern beeMovement = b.GetComponent<EnemyPattern>();
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
                EnemyPattern beeMovement = b.GetComponent<EnemyPattern>();
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
}
