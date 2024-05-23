using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EnemyMovement_Scorpion : EnemyMovement
{
    // attack loop stuff
    private bool _isInAttackSequence;
    private int _cycleCount = 0;
    
    // bouncing stuff
    private Rigidbody2D[] _bouncingPartRigidBodies;
    private float[] _bouncingSpeeds = {2f, 2.3f, 2.6f, 2.6f};
    
    // shooting stuff
    [SerializeField] private GameObject _shooterPositionObject;
    [SerializeField] private GameObject _basePositionObject;
    [SerializeField] private GameObject _shooterObject;
    [SerializeField] private GameObject[] _tailObjects;
    private GameObject _bulletObject;
    private Vector3 _shooterDirection;
    private Vector3 _playerDirection;
    private bool _isShootingBullets;
    private int _raycastLayerMask;
    private LineRenderer _lineRenderer;
    
    // claw stuff
    [SerializeField] private GameObject[] _armObjects = new GameObject[6];
    private Vector3[] _defaultClawPositions = new Vector3[2];
    private Vector3[] _electricClawPositions = new Vector3[2];
    private float _defaultClawGap = 16f;
    private float _electricClawGap = 5f;
    // private static readonly int IsClawAttacking = Animator.StringToHash("IsClawAttacking");
    

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
        
        Vector3 midpoint = gameObject.transform.position;
        _defaultClawPositions = new []
        {
            midpoint + new Vector3(-_defaultClawGap, 0f, 0),
            midpoint + new Vector3(_defaultClawGap, 0f, 0)
        };
        _electricClawPositions = new[]
        {
            midpoint + new Vector3(-_electricClawGap, 0, 0),
            midpoint + -new Vector3(_electricClawGap, 0, 0)
        };
        
        _raycastLayerMask = LayerMask.GetMask("Platform");
        _lineRenderer = GetComponent<LineRenderer>();
    }
    
    public override void EnableMovement()
    {
        IsRooted = false;
        EnableFlip();
        // ResetMoveSpeed();
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
        float[] flipMoveAmount = {1f, 1.7f, -0.3f};
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

        _shooterDirection = _shooterPositionObject.transform.position - _basePositionObject.transform.position;
        _playerDirection = _player.transform.position - _basePositionObject.transform.position;
        _shooterObject.transform.rotation *= Quaternion.FromToRotation(_shooterDirection, _playerDirection).normalized;
    }

    private IEnumerator ShootBullet()
    {
        while (_isShootingBullets)
        {
            yield return new WaitForSeconds(5f);
            if (!_isShootingBullets) yield break;
            var bullet = Instantiate(_bulletObject,
                _shooterPositionObject.transform.position,
                Quaternion.identity).GetComponent<Scorpion_Bullet>();
            bullet.Shoot(_shooterDirection);
        }
    }

    private IEnumerator ShootLaser()
    {
        _isInAttackSequence = true;
        _isShootingBullets = false;
        float rotationSpeed = 5f;
        float rotationDegrees = 10f;
        int direction = 1;
        if (Random.Range(0f, 1f) > 0.5f) direction *= -1;
        _lineRenderer.enabled = true;
        
        _shooterDirection = _shooterPositionObject.transform.position - _basePositionObject.transform.position;
        _playerDirection = _player.transform.position - _basePositionObject.transform.position;
        _shooterObject.transform.rotation *= Quaternion.FromToRotation(_shooterDirection, _playerDirection).normalized;

        _shooterObject.transform.Rotate(0, 0, -direction * rotationDegrees);
        float initialAngle = _shooterObject.transform.eulerAngles.z;
        
        while (Mathf.Abs(_shooterObject.transform.eulerAngles.z - initialAngle) <= rotationDegrees * 2)
        {
            Vector3 position = _shooterPositionObject.transform.position;
            _shooterDirection = position - _basePositionObject.transform.position;
            if (Physics2D.Raycast(position, _shooterDirection, 100f, _raycastLayerMask))
            {
                RaycastHit2D hit = Physics2D.Raycast(position, _shooterDirection, 100f, _raycastLayerMask);
                Draw2DRay(position, hit.point);
            }
            else
            {
                Draw2DRay(_shooterPositionObject.transform.position, _shooterDirection * 100f);
            }
            _shooterObject.transform.Rotate(0, 0, direction * rotationSpeed * Time.deltaTime);
            yield return null;
        }

        _lineRenderer.enabled = false;
        _isShootingBullets = true;
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
        StartCoroutine(MoveClaws(_defaultClawPositions, 2f));
        GripClaws();
        // StartCoroutine(RotateByAmount(_armObjects[0], 90f, 1, 50f));
        // _animator.SetBool(IsClawAttacking, true);
        
        yield return new WaitForSeconds(3f);
        Vector3 gap = new Vector3(5f, 0, 0);
        Vector3 midpoint = gameObject.transform.position;
        StartCoroutine(MoveClaws(new[] {midpoint - gap, midpoint + gap}, 5f));
        
        yield return new WaitForSeconds(5f);
        // _animator.SetBool(IsClawAttacking, false);
        // StartCoroutine(RotateByAmount(_armObjects[0], 90f, -1, 50f));
        StartCoroutine(MoveClaws(_defaultClawPositions, 2f));
        _isInAttackSequence = false;
    }

    private IEnumerator ElectricityAttack()
    {
        // _isInAttackSequence = true;
        // StartCoroutine(MoveClaws(_electricClawPositions, 5f));
        // for (int i = 0; i <= 1; i++)
        // {
        //     _clawObjects[i].GetComponent<SpriteRenderer>().color = Color.cyan;
        //     _clawObjects[i].GetComponent<PolygonCollider2D>().enabled = false;
        // }
        // yield return new WaitForSeconds(1f);
        //
        // _electricityObject.SetActive(true);
        //
        // yield return new WaitForSeconds(5f);
        //
        // _electricityObject.SetActive(false);
        // for (int i = 0; i <= 1; i++)
        // {
        //     _clawObjects[i].GetComponent<SpriteRenderer>().color = Color.white;
        //     _clawObjects[i].GetComponent<PolygonCollider2D>().enabled = true;
        // }
        // StartCoroutine(MoveClaws(_defaultClawPositions, 5f));
        // yield return new WaitForSeconds(2f);
        // _isInAttackSequence = false;
        yield return null;
    }

    private void GroundPound()
    {
        _isInAttackSequence = true;

        _isInAttackSequence = false;
    }

    private IEnumerator MoveClaws(Vector3[] finalPositions, float speed)
    {
        _isInAttackSequence = true;
        Vector3[] initialPositions =
        {
            _armObjects[0].transform.position,
            _armObjects[3].transform.position
        };
        Vector3[] directions =
        {
            finalPositions[0] - initialPositions[0], // left claw
            finalPositions[1] - initialPositions[1] // right claw
        };
  
        for (int i = 1; i <= 100 / speed; i++)
        {
            float step = speed * i / 100f;
            MoveClawIK(initialPositions[0] + directions[0] * step, true);
            MoveClawIK(initialPositions[1] + directions[1] * step, false);
            yield return null;
        }
    }
    
    private void MoveClawIK(Vector3 finalPosition, bool isLeftClaw)
    {
        int rightClaw = 3;
        if (isLeftClaw) rightClaw = 0;
        
        for (int j = 0; j <= 5; j++)
        {
            for (int i = 1; i <= 2; i++)
            {
                Vector3 a = _armObjects[0 + rightClaw].transform.position - _armObjects[i + rightClaw].transform.position;
                Vector3 b = finalPosition - _armObjects[i + rightClaw].transform.position;
                Vector3 cross = Vector3.Cross(b, a).normalized;
                float rotationAngle = Vector3.Angle(b, a);
                int direction = -1;
                if ((int)Mathf.Round(cross.z) == -1) direction *= -1;
                _armObjects[i + rightClaw].transform.Rotate(0f,0f, direction * rotationAngle);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_defaultClawPositions[0], 1);
        Gizmos.DrawSphere(_defaultClawPositions[1], 1);
    }

    private IEnumerator RotateByAmount(GameObject obj, float angle, int direction, float speed)
    {
        float initialAngle = obj.transform.eulerAngles.z;
        while (Mathf.Abs(obj.transform.eulerAngles.z - initialAngle) <= angle)
        {
            obj.transform.Rotate(0, 0, direction * speed * Time.deltaTime);
            yield return null;
        }
    }

    private void GripClaws()
    {
        
    }
    
    public override void Attack()
    {
        if (_isShootingBullets) TrackPlayer();
        if (_isInAttackSequence) return;
        
        switch (_cycleCount)
        {
            case 0:
            GroundPound();
            _cycleCount++;
            break;
            
            case >= 2:
            StartCoroutine(ElectricityAttack());
            _cycleCount = 0;
            break;
            
            default:
            GenerateRandomAttack();
            break;
        }
    }
    
    private void GenerateRandomAttack()
    {
        switch (Math.Floor(Random.Range(0f, 2f)))
        {
            case 0:
            StartCoroutine(ShootLaser());
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