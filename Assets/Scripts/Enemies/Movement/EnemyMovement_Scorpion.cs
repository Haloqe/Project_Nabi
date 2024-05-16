using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class EnemyMovement_Scorpion : EnemyMovement
{
    private Vector2 _attackDirection;
    private Vector2 _patrolDirection;
    private bool _directionIsChosen = false;
    private float _attackTimeCounter = 0f;
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private GameObject _shooterPositionObject;
    [SerializeField] private GameObject _basePositionObject;
    [SerializeField] private GameObject _shooterObject;
    // [SerializeField] private GameObject _headObject;
    // [SerializeField] private GameObject _bottomStingerObject;
    // [SerializeField] private GameObject _topStingerObject;
    // [SerializeField] GameObject[] _partObjects;
    private Rigidbody2D[] _partRigidbodies;
    private float[] _bouncingSpeeds = new float[] {2f, 2.3f, 2.6f, 2.7f};
    private Vector3 _stingerDirection;
    private Vector3 _playerDirection;
    
    private void Awake()
    {
        MoveType = EEnemyMoveType.Scorpion;

        Transform bouncingObjectsParent = transform.Find("bouncingObjects");
        _partRigidbodies = bouncingObjectsParent.GetComponentsInChildren<Rigidbody2D>();
        for (int i = 0; i < _partRigidbodies.Length; i++)
        {
            Vector2 initialForce = new Vector2(0f, _bouncingSpeeds[i]);
            _partRigidbodies[i].AddForce(initialForce, ForceMode2D.Impulse);
            StartCoroutine(Bounce(_partRigidbodies[i], _bouncingSpeeds[i]));
        }
    }

    private IEnumerator Bounce(Rigidbody2D rb, float speed)
    {
        while (true)
        {
            Vector2 targetPosition = new Vector2(rb.position.x, rb.position.y + 1f);
            Vector2 velocity = Vector2.zero;
            rb.position = Vector2.SmoothDamp(rb.position, targetPosition, ref velocity, speed);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public override void Patrol()
    {

    }

    private IEnumerator ChooseRandomDirection()
    {
        _directionIsChosen = true;
        _patrolDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        yield return new WaitForSeconds(Random.Range(3f, 5f));
        _directionIsChosen = false;
    }

    private void DefaultMovement()
    {
        Vector3 direction = new Vector3(0, 0.3f * (Mathf.PingPong(Time.time, 2) - 1f), 0);
        Vector3 force = direction * 100f * Time.deltaTime;
        
        // _headRB.AddForce(force);
    }

    public override void Attack()
    {
        if (_attackTimeCounter <= 0)
        {
            GenerateRandomAttack();
        }
        _attackTimeCounter -= Time.deltaTime;

        // track player
        // shoot every 5 seconds
        TrackPlayer();
        DefaultMovement();
    }

    private void FlipShooter()
    {

    }

    private void TrackPlayer()
    {
        // get player location
        // if player location is to the left and is facing right, flip. vice versa
        // get the vector of the direction of the stinger and the vector to the direction of the player
        // subtract the vectors and add torque to it

        if (transform.position.x - _playerObject.transform.position.x >= 0)
        {
            if (_shooterObject.transform.localScale.x > Mathf.Epsilon) FlipShooter();
        } else {
            if (_shooterObject.transform.localScale.x < Mathf.Epsilon) FlipShooter();
        }

        _stingerDirection = _shooterPositionObject.transform.position - _basePositionObject.transform.position;
        _playerDirection = _playerObject.transform.position - _basePositionObject.transform.position;
        _shooterObject.transform.rotation *= Quaternion.FromToRotation(_stingerDirection, _playerDirection).normalized;
        
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
