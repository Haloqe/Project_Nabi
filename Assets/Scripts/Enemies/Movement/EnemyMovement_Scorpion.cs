using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EnemyMovement_Scorpion : EnemyMovement
{
    private bool _isInAttackSequence;
    
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
    private Vector3[] _defaultClawPositions = { new(-15.69f, -2f, 0f), new(10.9f, -2f, 0f) };
    private Vector3[] _electricClawPositions = { new(-26f, -3f, 0), new(16f, -3f, 0) };
    private Vector3 _midpoint = new(-3.5f, -5f, 0f);
    
    private bool _isShootingBullets;
    private int _raycastLayerMask;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private GameObject _electricityObject;

    private int _cycleCount = 0;
    private static readonly int IsClawAttacking = Animator.StringToHash("IsClawAttacking");

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
        
        _raycastLayerMask = LayerMask.GetMask("Platform");

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
        while (_isShootingBullets)
        {
            yield return new WaitForSeconds(5f);
            var bullet = Instantiate(_bulletObject,
                _shooterPositionObject.transform.position,
                Quaternion.identity).GetComponent<Scorpion_Bullet>();
            bullet.Shoot(_stingerDirection);
        }
    }

    private IEnumerator ShootLaser()
    {
        _isInAttackSequence = true;
        float rotationSpeed = 5f;
        float rotationDegrees = 6f;
        int direction = 1;
        if (Random.Range(0f, 1f) > 0.5f) direction *= -1;
        _isShootingBullets = false;
        _lineRenderer.enabled = true;
        
        _shooterObject.transform.rotation *= Quaternion.FromToRotation(_stingerDirection, _playerDirection).normalized;
        _shooterObject.transform.Rotate(0, 0, direction * rotationDegrees);
        float initialAngle = _shooterObject.transform.eulerAngles.z;
        while (Mathf.Abs(_shooterObject.transform.eulerAngles.z - initialAngle) <= rotationDegrees * 2)
        {
            Vector3 position = _shooterPositionObject.transform.position;
            _stingerDirection = position - _basePositionObject.transform.position;
            if (Physics2D.Raycast(position, _stingerDirection, 100f, _raycastLayerMask))
            {
                RaycastHit2D hit = Physics2D.Raycast(position, _stingerDirection, 100f, _raycastLayerMask);
                Draw2DRay(position, hit.point);
            }
            else
            {
                Draw2DRay(_shooterPositionObject.transform.position, _stingerDirection * 100f);
            }
            yield return null;
            _shooterObject.transform.Rotate(0, 0, direction * rotationSpeed * Time.deltaTime);
        }

        _isShootingBullets = true;
        _lineRenderer.enabled = false;
        StartCoroutine(ShootBullet());
        _isInAttackSequence = false;
    }

    private void Draw2DRay(Vector2 startPos, Vector2 endPos)
    {
        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.SetPosition(1, endPos);
    }

    private IEnumerator ClawAttack()
    {
        _isInAttackSequence = true;
        StartCoroutine(MoveClaws(_defaultClawPositions, 5f));
        GripClaws();
        _animator.SetBool(IsClawAttacking, true);
        
        yield return new WaitForSeconds(5f);
        Vector3 gap = new Vector3(2f, 0, 0);
        StartCoroutine(MoveClaws(new[] {_midpoint - gap, _midpoint + gap}, 50f));
        
        yield return new WaitForSeconds(5f);
        _animator.SetBool(IsClawAttacking, false);
        StartCoroutine(MoveClaws(_defaultClawPositions, 5f));
        _isInAttackSequence = false;
    }

    private IEnumerator ElectricityAttack()
    {
        _isInAttackSequence = true;
        StartCoroutine(MoveClaws(_electricClawPositions, 5f));
        for (int i = 0; i <= 1; i++)
        {
            _clawObjects[i].GetComponent<SpriteRenderer>().color = Color.cyan;
            _clawObjects[i].GetComponent<PolygonCollider2D>().enabled = false;
        }
        yield return new WaitForSeconds(1f);
        
        _electricityObject.SetActive(true);
        
        yield return new WaitForSeconds(5f);
        
        _electricityObject.SetActive(false);
        for (int i = 0; i <= 1; i++)
        {
            _clawObjects[i].GetComponent<SpriteRenderer>().color = Color.white;
            _clawObjects[i].GetComponent<PolygonCollider2D>().enabled = true;
        }
        StartCoroutine(MoveClaws(_defaultClawPositions, 5f));
        yield return new WaitForSeconds(2f);
        _isInAttackSequence = false;
    }

    private void GroundPound()
    {
        _isInAttackSequence = true;

        _isInAttackSequence = false;
    }

    private IEnumerator MoveClaws(Vector3[] finalPositions, float speed)
    {
        while (!IsCloseEnough(_clawObjects, finalPositions))
        {
            float step = speed * Time.deltaTime;
            for (int i = 0; i <= 1; i++)
            {
                _clawObjects[i].transform.position =
                    Vector3.MoveTowards(_clawObjects[i].transform.position, finalPositions[i], step);
            }
            yield return null;
        }
    }

    private void GripClaws()
    {
        
    }

    private bool IsCloseEnough(GameObject[] a, Vector3[] b)
    {
        for (int i = 0; i <= 1; i++)
        {
            if (Vector3.Distance(a[i].transform.position, b[i]) < 0.001f)
            {
                a[i].transform.position = b[i];
            } else {
                return false;
            }
        }
        return true;
    }
    
    public override void Attack()
    {
        if (_isShootingBullets) TrackPlayer();
        if (_isInAttackSequence) return;

        if (_cycleCount == 0)
        {
            GroundPound();
            _cycleCount++;
            return;
        }

        if (_cycleCount >= 2)
        {
            StartCoroutine(ElectricityAttack());
            _cycleCount = 0;
            return;
        }
        
        GenerateRandomAttack();
    }
    
    private void GenerateRandomAttack()
    {
        switch (Math.Floor(Random.Range(0f, 2f)))
        {
            case 0:
            StartCoroutine(ShootLaser());
            // StartCoroutine(ElectricityAttack());
            break;

            case 1:
            StartCoroutine(ClawAttack());
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