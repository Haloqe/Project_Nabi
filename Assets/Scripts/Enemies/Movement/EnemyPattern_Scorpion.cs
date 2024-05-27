using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EnemyPattern_Scorpion : EnemyPattern
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
    private GameObject _laserObject;
    private Vector3 _shooterDirection;
    private Vector3 _playerDirection;
    private bool _isShootingBullets;
    private int _raycastLayerMask;
    private LineRenderer _lineRenderer;
    
    // claw stuff
    [SerializeField] private GameObject[] _armObjects = new GameObject[6];
    private GameObject _electricityObject;
    private Vector3[] _defaultClawPositions = new Vector3[2];
    private Vector3[] _electricClawPositions = new Vector3[2];
    private Vector3[] _raisedClawPositions = new Vector3[2];
    private Vector3[] _clawAttackPositions = new Vector3[2];
    private float _defaultClawGap = 16f;
    private float _electricClawGap = 16f;
    private float _raisedClawGap = 12f;
    private float _clawAttackGap = 3f;
    // private static readonly int IsClawAttacking = Animator.StringToHash("IsClawAttacking");
    
    private void Awake()
    {
        MoveType = EEnemyMoveType.Scorpion;
        _bulletObject = Resources.Load<GameObject>("Prefabs/Enemies/Spawns/Scorpion_bullet");
        _laserObject = transform.Find("laser").transform.gameObject;
        _electricityObject = transform.Find("electricity").transform.gameObject;
        
        Transform bouncingObjectsParent = transform.Find("bouncingObjects");
        _bouncingPartRigidBodies = bouncingObjectsParent.GetComponentsInChildren<Rigidbody2D>();
        for (int i = 0; i < _bouncingPartRigidBodies.Length; i++)
        {
            StartCoroutine(Bounce(_bouncingPartRigidBodies[i], _bouncingSpeeds[i]));
        }
        
        Vector3 midpoint = gameObject.transform.position;
        _defaultClawPositions = new []
        {
            midpoint + new Vector3(-_defaultClawGap, 3f, 0),
            midpoint + new Vector3(_defaultClawGap, 3f, 0)
        };
        _electricClawPositions = new[]
        {
            midpoint + new Vector3(-_electricClawGap, 3f, 0),
            midpoint + new Vector3(_electricClawGap, 3f, 0)
        };
        _raisedClawPositions = new[]
        {
            midpoint + new Vector3(-_raisedClawGap, 15f, 0),
            midpoint + new Vector3(_raisedClawGap, 15f, 0)
        };
        _clawAttackPositions = new[]
        {
            midpoint + new Vector3(-_clawAttackGap, 3f, 0),
            midpoint + new Vector3(_clawAttackGap, 3f, 0)
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
        float rotationDegrees = 20f;
        int direction = 1;
        if (Random.Range(0f, 1f) > 0.5f) direction *= -1;
        _lineRenderer.enabled = true;
        _laserObject.SetActive(true);
        
        _shooterDirection = _shooterPositionObject.transform.position - _basePositionObject.transform.position;
        _playerDirection = _player.transform.position - _basePositionObject.transform.position;
        _shooterObject.transform.rotation *=
            Quaternion.FromToRotation(_shooterDirection, _playerDirection).normalized;

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
        _laserObject.SetActive(false);
        StartCoroutine(ShootBullet());
        _isInAttackSequence = false;
    }

    private void Draw2DRay(Vector2 startPos, Vector2 endPos)
    {
        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.SetPosition(1, endPos);

        float laserWidth = 1f;

        _laserObject.transform.localScale = new Vector3(laserWidth, Vector2.Distance(startPos, endPos), 0f);
        float angle = Vector3.Angle(endPos - startPos, new Vector3(0f, 1f, 0f));
        _laserObject.transform.eulerAngles = new Vector3(0f, 0f, -angle * Math.Sign((endPos - startPos).x));
        _laserObject.transform.position = Vector2.Lerp(startPos, endPos, 0.5f);
    }

    private IEnumerator ClawAttack()
    {
        _isInAttackSequence = true;
        
        yield return MoveClaws(_defaultClawPositions, 2f);
        yield return RotateByAmount(10f, 1, 50f);
        
        // snap snap thing
        yield return new WaitForSeconds(2f);
        yield return MoveClaws(_clawAttackPositions, 5f);
        
        yield return new WaitForSeconds(2f);
        yield return RotateByAmount(10f, -1, 50f);
        yield return MoveClaws(_defaultClawPositions, 2f);
        
        _isInAttackSequence = false;
    }

    private IEnumerator ElectricityAttack()
    {
        _isInAttackSequence = true;
        
        yield return MoveClaws(_electricClawPositions, 5f);
        _animator.SetTrigger("ElectricityAttack");
        yield return new WaitForSeconds(1f);
        // animator: sets colliders, sets claw colors, sets electricity active
        // resets colliders, resets claw colors, resets electricity active
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        yield return MoveClaws(_defaultClawPositions, 5f);
        
        _isInAttackSequence = false;
    }

    private IEnumerator GroundPound()
    {
        _isInAttackSequence = true;
        
        yield return MoveClaws(_raisedClawPositions, 2f);
        // rotate claws
        yield return new WaitForSeconds(2f);

        float playerXPosition = _player.transform.position.x;
        float xLevel = transform.position.x;
        float yLevel = transform.position.y + 3f;
        Vector3[] attackPosition = new Vector3[2];
        
        yield return RotateByAmount(45f, -1, 70f);
        _animator.SetTrigger("GroundPound");
        yield return new WaitForSeconds(1f);
        
        if (playerXPosition - xLevel is >= -3f and <= 3f) // if player is at middle
        {
            attackPosition = new [] {
                new Vector3(xLevel - 0.5f, yLevel, 0),
                new Vector3(xLevel + 0.5f, yLevel, 0)
            };
        }
        else if (playerXPosition < xLevel) // if player is at left
        {
            attackPosition = new [] {
                new Vector3(playerXPosition, yLevel, 0),
                new Vector3(2 * xLevel - playerXPosition, yLevel, 0)
            };
        }
        else // if player is at right
        {
            attackPosition = new [] {
                new Vector3(2 * xLevel - playerXPosition, yLevel, 0),
                new Vector3(playerXPosition, yLevel, 0)
            };
        }
        
        yield return MoveClaws(attackPosition, 3f);
        yield return new WaitForSeconds(0.5f);
        yield return RotateByAmount(45f, 1, 50f);
        yield return MoveClaws(_defaultClawPositions, 2f);

        _isInAttackSequence = false;
    }

    private IEnumerator MoveClaws(Vector3[] finalPositions, float speed)
    {
        Vector3[] initialPositions =
        {
            _armObjects[0].transform.position, // left claw
            _armObjects[3].transform.position // right claw
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
        
        for (int j = 0; j <= 5; j++) // iterations
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
                _armObjects[0 + rightClaw].transform.Rotate(0f, 0f, -direction * rotationAngle);
            }
        }
        
        Vector3 c = _armObjects[0 + rightClaw].transform.position - _armObjects[1 + rightClaw].transform.position;
        Vector3 d = _armObjects[1 + rightClaw].transform.position - _armObjects[2 + rightClaw].transform.position;
        Vector3 crossP = Vector3.Cross(c, d).normalized;
        
        int isWrongDirection = (int)Mathf.Round(crossP.z) * (int)Mathf.Pow(-1, rightClaw);
        if (isWrongDirection != -1) return;
        if (_armObjects[1 + rightClaw].transform.position.y < transform.position.y) return;

        _armObjects[1 + rightClaw].transform.Rotate(0f, 0f,
            -_armObjects[1 + rightClaw].transform.rotation.z);
        float flipAngle = Vector3.Angle(c, d);
        _armObjects[2 + rightClaw].transform.Rotate(0f, 0f, 
            Mathf.Pow(-1, rightClaw + 1) * (360f - 2 * flipAngle));
    }
    
    private IEnumerator RotateByAmount(float angle, int direction, float speed)
    {
        float initialAngle = _armObjects[0].transform.eulerAngles.z;
        while (true)
        {
            _armObjects[0].transform.eulerAngles =
                new Vector3(0, 0, _armObjects[0].transform.eulerAngles.z + direction * speed * Time.deltaTime);
            _armObjects[3].transform.eulerAngles =
                new Vector3(0, 0, _armObjects[3].transform.eulerAngles.z - direction * speed * Time.deltaTime);
            yield return null;

            float now = _armObjects[0].transform.eulerAngles.z;
            if (Mathf.Abs(now - initialAngle) >= 180f)
                now -= Mathf.Sign(now) * 360f;
            Debug.Log("initial angle: " + initialAngle + " and now: " + now);
            if (Mathf.Abs(direction * (now - initialAngle)) > angle)
                break;
        }
    }
    
    public override void Attack()
    {
        if (_isShootingBullets) TrackPlayer();
        if (_isInAttackSequence) return;
        
        switch (_cycleCount)
        {
            case 0:
            StartCoroutine(GroundPound());
            _cycleCount++;
            break;
            
            case >= 2:
            StartCoroutine(ElectricityAttack());
            _cycleCount = 0;
            break;
            
            default:
            GenerateRandomAttack();
            _cycleCount++;
            break;
        }
    }
    
    private void GenerateRandomAttack()
    {
        switch (Random.Range(1, 3))
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