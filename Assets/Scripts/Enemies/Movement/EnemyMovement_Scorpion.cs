using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyMovement_Scorpion : EnemyMovement
{
    private float _attackTimeCounter = 0f;
    [SerializeField] private GameObject _shooterPositionObject;
    [SerializeField] private GameObject _basePositionObject;
    [SerializeField] private GameObject _shooterObject;
    [SerializeField] private GameObject[] _tailObjects;
    private GameObject _bulletObject;
    private Rigidbody2D[] _bouncingPartRigidBodies;
    private Rigidbody2D[] _clawRigidBodies;
    private GameObject[] _clawObjects;
    private float[] _bouncingSpeeds = {2f, 2.3f, 2.6f, 2.6f};
    private Vector3 _stingerDirection;
    private Vector3 _playerDirection;
    
    private void Awake()
    {
        MoveType = EEnemyMoveType.Scorpion;
        _bulletObject = Resources.Load<GameObject>("Prefabs/Enemies/Spawns/Scorpion_bullet");
        
        Transform bouncingObjectsParent = transform.Find("bouncingObjects");
        _bouncingPartRigidBodies = bouncingObjectsParent.GetComponentsInChildren<Rigidbody2D>();
        for (int i = 0; i < _bouncingPartRigidBodies.Length; i++)
        {
            StartCoroutine(Bounce(_bouncingPartRigidBodies[i], _bouncingSpeeds[i]));
        }
        
        Transform clawObjectsParent = transform.Find("clawObjects");
        _clawRigidBodies = clawObjectsParent.GetComponentsInChildren<Rigidbody2D>();
        _clawObjects = new GameObject[_clawRigidBodies.Length];
        for (int i = 0; i < _clawRigidBodies.Length; i++) { _clawObjects[i] = _clawRigidBodies[i].gameObject; }
        
    }

    public override void Init()
    {
        base.Init();
        StartCoroutine(ShootBullet());
    }

    private IEnumerator Bounce(Rigidbody2D rb, float speed)
    {
        int direction = 1;
        while (true)
        {
            var force = new Vector2(0f, direction * speed * 0.5f);
            rb.AddForce(force, ForceMode2D.Impulse);
            yield return new WaitForSeconds(3f / speed);
            direction *= -1;
        }
    }

    public override void Patrol()
    {
        
    }

    public override void Attack()
    {
        if (_attackTimeCounter <= 0)
        {
            GenerateRandomAttack();
        }
        _attackTimeCounter -= Time.deltaTime;
        
        TrackPlayer();
    }

    private void FlipTail(int negativeOrPositive)
    {
        float[] flipMoveAmount = {1f, 2f, -0.5f};
        for (int i = 0; i < _tailObjects.Length; i++)
        {
            _tailObjects[i].transform.localScale = new Vector2(
                -1f * (_tailObjects[i].transform.localScale.x),
                _tailObjects[i].transform.localScale.y);
            _tailObjects[i].transform.position = new Vector2(
                (_tailObjects[i].transform.position.x + flipMoveAmount[i] * negativeOrPositive),
                _tailObjects[i].transform.position.y);
        }
    }

    private void TrackPlayer()
    {
        if (transform.position.x - _player.transform.position.x >= 0)
        {
            if (_shooterObject.transform.localScale.x > Mathf.Epsilon) FlipTail(1);
        } else {
            if (_shooterObject.transform.localScale.x < Mathf.Epsilon) FlipTail(-1);
        }

        _stingerDirection = _shooterPositionObject.transform.position - _basePositionObject.transform.position;
        _playerDirection = _player.transform.position - _basePositionObject.transform.position;
        _shooterObject.transform.rotation *= Quaternion.FromToRotation(_stingerDirection, _playerDirection).normalized;
    }

    private IEnumerator ShootBullet()
    {
        while (_player != null)
        {
            yield return new WaitForSeconds(5f);
            var bullet = Instantiate(_bulletObject,
                _shooterPositionObject.transform.position,
                Quaternion.identity).GetComponent<Scorpion_Bullet>();
            bullet.Shoot(_stingerDirection);
        }
    }

    private void ClawAttack()
    {
        // initial position
        // telegraph
        // go to the middle
        // go back to initial position
        
    }

    private void MoveClawsBack()
    {
        // Vector3[] initialPositions = {_clawObjects[0].transform.position, _clawObjects[1].transform.position};
        
    }

    private void GenerateRandomAttack()
    {
        switch (Math.Floor(Random.Range(0.0f, 3f)))
        {
            case 0:
            _attackTimeCounter += 5f;
            break;

            case 1:
            _attackTimeCounter += 5f;
            break;

            case 2:
            _attackTimeCounter += 5f;
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
