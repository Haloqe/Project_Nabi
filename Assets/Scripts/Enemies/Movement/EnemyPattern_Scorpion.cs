using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object; 
using Random = UnityEngine.Random;

public class EnemyPattern_Scorpion : EnemyPattern
{
    // colliders & rigid bodies
    public List<Collider2D> AttackableColliders;
    
    // attack loop stuff
    private bool _isInAttackSequence;
    private int _cycleCount = 0;
    private int _tailCycleCount = 0;
    private BossHealthBar _bossHealthBar;
    
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
    private GameObject _laserVFXObject;
    private Object[] _VFXobjects;
    private Vector3 _shooterDirection;
    private Vector3 _playerDirection;
    private bool _isShootingBullets = false;
    private int _raycastLayerMask;
    private LineRenderer _lineRenderer;
    
    // claw stuff
    [SerializeField] private GameObject[] _armObjects = new GameObject[6];
    [SerializeField] private GameObject[] _dustParticles = new GameObject[2];
    private Vector3[] _defaultClawPositions = new Vector3[2];
    private Vector3[] _electricClawPositions = new Vector3[2];
    private Vector3[] _raisedClawPositions = new Vector3[2];
    private Vector3[] _clawAttackPositions = new Vector3[2];
    private float _defaultClawGap = 16f;
    private float _electricClawGap = 16f;
    private float _raisedClawGap = 12f;
    private float _clawAttackGap = 3f;
    
    // sfx
    [SerializeField] private AudioSource[] _audioSources;
    [SerializeField] private AudioClip _laserAudio;
    [SerializeField] private AudioClip _bulletAudio;
    [SerializeField] private AudioClip _clawSnapAudio;
    [SerializeField] private AudioClip _electricFieldAudio;
    [SerializeField] private AudioClip[] _groundPoundAudio;
    [SerializeField] private AudioClip _clawAttackAudio;
    [SerializeField] private AudioClip _clawDragAudio;
    [SerializeField] private AudioClip _clawBlinkAudio;
    
    private PlayableDirector _encounterTimeline;
    private bool _isInCutscene = true;
    
    private void Awake()
    {
        MoveType = EEnemyMoveType.Scorpion;
        _bulletObject = Resources.Load<GameObject>("Prefabs/Enemies/Spawns/Scorpion_bullet");
        _laserObject = transform.Find("laser collider").transform.gameObject;
        _laserVFXObject = transform.Find("laser VFX").transform.gameObject;
        _VFXobjects = Resources.LoadAll("Prefabs/Effects/ScorpionVFX");
        _bossHealthBar = Instantiate(Resources.Load<GameObject>("Prefabs/UI/InGame/BossHealthUI"),
            Vector3.zero, Quaternion.identity).GetComponentInChildren<BossHealthBar>();
        
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

    public override void Init()
    {
        base.Init();
        _encounterTimeline = GameObject.Find("Encounter Timeline").GetComponent<PlayableDirector>();
        StartCoroutine(StartEncounterTimeline());
    }

    private IEnumerator StartEncounterTimeline()
    {
        yield return new WaitForSeconds(2.5f);
        CameraManager.Instance.SwapCamera(
            CameraManager.Instance.AllVirtualCameras[7],
            CameraManager.Instance.AllVirtualCameras[6]);
        yield return new WaitForSeconds(2f);

        _encounterTimeline.Play();
        yield return new WaitForSeconds(2.6f);
        PlayerController.Instance.EnablePlayerInput();
        _isInCutscene = false;
        _isShootingBullets = true;
        StartCoroutine(ShootBullet());
    }

    private void Update()
    {
        if (_isShootingBullets) TrackPlayer();
    }
    
    public override void EnableMovement()
    {
        IsRooted = false;
        EnableFlip();
        // ResetMoveSpeed();
    }

    private void PlayAudio(int audioSourceIdx, AudioClip audioClip, float pitchRange = 0f, float volume = 1f)
    {
        _audioSources[audioSourceIdx].pitch = Random.Range(1 - pitchRange, 1 + pitchRange);
        _audioSources[audioSourceIdx].volume = volume;
        _audioSources[audioSourceIdx].PlayOneShot(audioClip);
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
    
    #region tail stuff
    
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
        _shooterPositionObject.transform.localEulerAngles = negativeOrPositive == -1
            ? new Vector3(0, 0, 10f)
            : new Vector3(0, 0, 70f);
    }

    private void TrackPlayer()
    {
        if (transform.position.x - _player.transform.position.x >= 0)
        {
            if (_shooterObject.transform.localScale.x > Mathf.Epsilon) 
                FlipTail(1);
        } else {
            if (_shooterObject.transform.localScale.x < Mathf.Epsilon) 
                FlipTail(-1);
        }

        _shooterDirection = _shooterPositionObject.transform.position - 
                            _basePositionObject.transform.position;
        _playerDirection = _player.transform.position - 
                           _basePositionObject.transform.position;
        _shooterObject.transform.rotation *= 
            Quaternion.FromToRotation(_shooterDirection, _playerDirection).normalized;
    }

    private IEnumerator ShootBullet()
    {
        while (_isShootingBullets)
        {
            yield return new WaitForSeconds(3f);
            _shooterPositionObject.SetActive(true);
            yield return new WaitForSeconds(0.4f);
            var bullet = Instantiate(_bulletObject, 
                _shooterPositionObject.transform.position, 
                Quaternion.identity).GetComponent<Scorpion_Bullet>();
            bullet.Shoot(_shooterDirection);
            PlayAudio(0, _bulletAudio, 0.3f, 0.7f);
            _shooterPositionObject.SetActive(false);
            GenerateRandomTailState();
        }
    }

    private IEnumerator ShootLaser()
    {
        _isShootingBullets = false;
        
        float rotationSpeed = 10.8f;
        float rotationDegrees = 15f;
        float rotatedAmountCounter = 0f;
        int direction = (int)Mathf.Sign(_player.transform.localScale.x);
        
        _shooterDirection = _shooterPositionObject.transform.position -
                            _basePositionObject.transform.position;
        _playerDirection = _player.transform.position -
                           _basePositionObject.transform.position;
        _shooterObject.transform.rotation *=
            Quaternion.FromToRotation(_shooterDirection, _playerDirection).normalized;
        _shooterObject.transform.Rotate(0, 0, -direction * rotationDegrees);

        float z = _shooterObject.transform.localEulerAngles.z;
        if (_shooterObject.transform.localScale.x > 0)
        {
            if (z is > 11f and < 325f)
                z = z < 179f ? 11f : 325f;
        }
        else
        {
            if (z is > 30f and < 350f)
                z = z < 220f ? 30f : 350f;
        }
        _shooterObject.transform.eulerAngles = new Vector3(0, 0, z);
        
        yield return new WaitForSeconds(0.5f);
        _lineRenderer.enabled = true;
        _laserObject.SetActive(true);
        _laserVFXObject.SetActive(true);
        
        _audioSources[0].loop = true;
        _audioSources[0].volume = 1f;
        _audioSources[0].clip = _laserAudio;
        _audioSources[0].Play();
        
        while (rotatedAmountCounter <= rotationDegrees * 2)
        {
            Vector3 position = _shooterPositionObject.transform.position;
            _shooterDirection = position - _basePositionObject.transform.position;
            if (Physics2D.Raycast(position, _shooterDirection, 100f, _raycastLayerMask))
            {
                RaycastHit2D hit = Physics2D.Raycast(position, 
                    _shooterDirection, 100f, _raycastLayerMask);
                Draw2DRay(position, hit.point);
                _laserVFXObject.transform.eulerAngles = hit.point.y > -6.5f
                    ? new Vector3(0, 0, Mathf.Sign(hit.point.x - transform.position.x) * 90f)
                    : new Vector3(0, 0, 0);
                _laserVFXObject.transform.position = new Vector3(hit.point.x, hit.point.y, 0f);
            }
            else
            {
                Draw2DRay(_shooterPositionObject.transform.position, _shooterDirection * 100f);
            }
            _shooterObject.transform.Rotate(0, 0, direction * rotationSpeed * Time.deltaTime);
            rotatedAmountCounter += rotationSpeed * Time.deltaTime;
            
            if ((_shooterObject.transform.localScale.x > 0
                 && _shooterObject.transform.localEulerAngles.z is > 11f and < 325f)
                || (_shooterObject.transform.localScale.x < 0
                    && _shooterObject.transform.localEulerAngles.z is > 30f and < 350f))
                direction *= -1;
            
            yield return null;
        }

        _audioSources[0].loop = false;
        _lineRenderer.enabled = false;
        _laserObject.SetActive(false);
        _laserVFXObject.SetActive(false);
        _isShootingBullets = true;
        
        StartCoroutine(ShootBullet());
    }

    private void Draw2DRay(Vector2 startPos, Vector2 endPos)
    {
        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.SetPosition(1, endPos);

        float laserWidth = 0.4f;

        _laserObject.transform.localScale = 
            new Vector3(laserWidth, Vector2.Distance(startPos, endPos) + 3f, 0f);
        float angle = Vector3.Angle(endPos - startPos, new Vector3(0f, 1f, 0f));
        _laserObject.transform.eulerAngles = 
            new Vector3(0f, 0f, -angle * Math.Sign((endPos - startPos).x));
        _laserObject.transform.position = Vector2.Lerp(startPos, endPos, 0.5f);
    }

    private void GenerateRandomTailState()
    {
        switch (_tailCycleCount)
        {
            case >= 6:
                // StartCoroutine(Random.Range(0, 2) >= 1 ? ShootLaser() : ShootBullet());
                if (Random.Range(1, 2) >= 1)
                {
                    StopCoroutine(ShootBullet());
                    StartCoroutine(ShootLaser());
                    _tailCycleCount = 0;
                }
                break;
            default:
                _tailCycleCount++;
                break;
        }
    }
    
    #endregion tail stuff
    
    #region claw movements
    
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
  
        float step = 0;
        while (step <= 1f)
        {
            step += speed * Time.deltaTime;
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
                Vector3 a = _armObjects[0 + rightClaw].transform.position -
                            _armObjects[i + rightClaw].transform.position;
                Vector3 b = finalPosition - _armObjects[i + rightClaw].transform.position;
                Vector3 cross = Vector3.Cross(b, a).normalized;
                float rotationAngle = Vector3.Angle(b, a);
                int direction = -1;
                if ((int)Mathf.Round(cross.z) == -1) direction *= -1;
                _armObjects[i + rightClaw].transform.Rotate(0f,0f, direction * rotationAngle);
                _armObjects[0 + rightClaw].transform.Rotate(0f, 0f, -direction * rotationAngle);
            }
        }
        
        Vector3 c = _armObjects[0 + rightClaw].transform.position -
                    _armObjects[1 + rightClaw].transform.position;
        Vector3 d = _armObjects[1 + rightClaw].transform.position -
                    _armObjects[2 + rightClaw].transform.position;
        Vector3 crossP = Vector3.Cross(c, d).normalized;

        Vector3 clawAngle = _armObjects[0 + rightClaw].transform.eulerAngles;
        
        int isWrongDirection = (int)Mathf.Round(crossP.z) * (int)Mathf.Pow(-1, rightClaw);
        if (isWrongDirection != -1) return;
        if (_armObjects[1 + rightClaw].transform.position.y < transform.position.y) return;

        _armObjects[1 + rightClaw].transform.Rotate(0f, 0f,
            -_armObjects[1 + rightClaw].transform.rotation.z);
        float flipAngle = Vector3.Angle(c, d);
        _armObjects[2 + rightClaw].transform.Rotate(0f, 0f, 
            Mathf.Pow(-1, rightClaw + 1) * (360f - 2 * flipAngle));
        
        _armObjects[0 + rightClaw].transform.eulerAngles = clawAngle;
    }
    
    private IEnumerator RotateByAmount(float angle, int direction, float speed)
    {
        float initialAngle = _armObjects[0].transform.eulerAngles.z;
        while (true)
        {
            _armObjects[0].transform.eulerAngles =
                new Vector3(0, 0, _armObjects[0].transform.eulerAngles.z +
                                  direction * speed * Time.deltaTime);
            _armObjects[3].transform.eulerAngles =
                new Vector3(0, 0, _armObjects[3].transform.eulerAngles.z -
                                  direction * speed * Time.deltaTime);
            yield return null;

            float now = _armObjects[0].transform.eulerAngles.z;
            if (Mathf.Abs(now - initialAngle) >= 180f)
                now -= Mathf.Sign(now) * 360f;
            if (Mathf.Abs(now - initialAngle) > angle)
                break;
        }
    }
    
    #endregion claw movements
    
    private IEnumerator ClawAttack()
    {
        _isInAttackSequence = true;
        
        yield return MoveClaws(_defaultClawPositions, 2f);
        yield return RotateByAmount(10f, 1, 50f);
        
        _animator.SetTrigger("GroundPound");
        yield return new WaitForSeconds(1.8f);
        PlayAudio(1, _clawAttackAudio, 0.1f);
        yield return new WaitForSeconds(0.2f);
        for (int i = -1; i <= 1; i += 2)
        {
            _dustParticles[Math.Sign(i + 1)].transform.localScale = 
                new Vector3(-i, 1, 1);
            _dustParticles[Math.Sign(i + 1)].transform.position =
                new Vector3(_armObjects[Math.Sign(i + 1) * 3].transform.position.x + i * 3f, -7.5f, 0);
            _dustParticles[Math.Sign(i + 1)].SetActive(true);
        }
        yield return MoveClaws(_clawAttackPositions, 5f);
        
        yield return RotateByAmount(10f, -1, 50f);
        StartCoroutine(VibrateClaws());
        yield return new WaitForSeconds(1.8f);
        PlayAudio(1, _clawDragAudio, 0.25f);
        yield return new WaitForSeconds(0.2f);
        
        for (int i = -1; i <= 1; i += 2)
        {
            _dustParticles[Math.Sign(i + 1)].transform.localScale = 
                new Vector3(i, 1, 1);
            _dustParticles[Math.Sign(i + 1)].transform.position =
                new Vector3(_armObjects[Math.Sign(i + 1) * 3].transform.position.x - i * 4f, -7f, 0);
            _dustParticles[Math.Sign(i + 1)].SetActive(true);
        }
        yield return MoveClaws(_defaultClawPositions, 2f);
        
        _isInAttackSequence = false;
    }

    private IEnumerator VibrateClaws()
    {
        float timer = 0f;
        Vector3[] initialPositions = 
            { _armObjects[0].transform.position, _armObjects[3].transform.position };
        
        while (timer <= 2f)
        {
            for (int i = 0; i <= 3; i += 3)
            {
                _armObjects[i].transform.position += 0.1f * timer * new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    0);
            }
            yield return null;
            timer += Time.deltaTime;
        }

        _armObjects[0].transform.position = initialPositions[0];
        _armObjects[3].transform.position = initialPositions[1];
    }

    private IEnumerator ElectricityAttack()
    {
        _isInAttackSequence = true;
        
        yield return MoveClaws(_electricClawPositions, 5f);
        yield return new WaitForSeconds(1f);
        _animator.SetTrigger("ElectricityAttack");
        // animator: sets colliders, sets claw colors, sets electricity active
        // resets colliders, resets claw colors, resets electricity active
        yield return new WaitForSeconds(4.83f * 1.7f);
        yield return new WaitForSeconds(0.5f);
        
        yield return MoveClaws(_defaultClawPositions, 5f);
        
        _isInAttackSequence = false;
    }

    private void PlayElectricFieldAudio()
    {
        _audioSources[1].volume = 0.7f;
        _audioSources[1].PlayOneShot(_electricFieldAudio);
    }

    private void StopElectricFieldAudio()
    {
        _audioSources[1].Stop();
        _audioSources[1].volume = 1f;
    }

    private void PlayClawBlinkAudio()
    {
        _audioSources[2].PlayOneShot(_clawBlinkAudio);
    }

    private IEnumerator PlayClawSnapAudio()
    {
        _audioSources[1].pitch = Random.Range(0.4f, 0.7f);
        _audioSources[1].PlayOneShot(_clawSnapAudio);
        yield return new WaitForSeconds(0.1f);
        _audioSources[1].pitch = Random.Range(0.4f, 0.7f);
        _audioSources[1].PlayOneShot(_clawSnapAudio);
    }

    private IEnumerator GroundPound()
    {
        _isInAttackSequence = true;
        
        yield return MoveClaws(_raisedClawPositions, 2f);
        // rotate claws
        yield return new WaitForSeconds(2f);
        
        yield return RotateByAmount(30f, -1, 90f);
        _animator.SetTrigger("GroundPound");
        yield return new WaitForSeconds(0.5f);
        
        float playerXPosition = _player.transform.position.x;
        float xLevel = transform.position.x;
        float yLevel = transform.position.y + 3f;
        
        yield return new WaitForSeconds(0.5f);
        Vector3[] attackPosition = new Vector3[2];
        
        attackPosition = new [] {
            new Vector3(playerXPosition - 0.5f, yLevel, 0),
            new Vector3(playerXPosition + 0.5f, yLevel, 0)
        };
        
        yield return MoveClaws(attackPosition, 3f);
        PlayAudio(1, _groundPoundAudio[Random.Range(0, 3)], 0.2f);
        PlayAudio(1, _groundPoundAudio[3], 0.2f);
        yield return new WaitForSeconds(0.5f);
        
        yield return VibrateClaws();
        for (int i = -1; i <= 1; i += 2)
        {
            _dustParticles[Math.Sign(i + 1)].transform.localScale = 
                new Vector3(i, 1, 1);
            _dustParticles[Math.Sign(i + 1)].transform.position =
                new Vector3(_armObjects[Math.Sign(i + 1) * 3].transform.position.x - i * 4f, -7f, 0);
            _dustParticles[Math.Sign(i + 1)].SetActive(true);
        }
        
        PlayAudio(1, _clawDragAudio, 0.25f);
        yield return MoveClaws(_defaultClawPositions, 2f);
        yield return RotateByAmount(30f, 1, 90f);

        _isInAttackSequence = false;
    }
    
    public override void Attack()
    {
        if (_isInAttackSequence || _isInCutscene) return;
        
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
            StartCoroutine(ClawAttack());
            _cycleCount++;
            break;
        }
    }

    public override void OnTakeDamage(float damage, float maxHealth)
    {
        _bossHealthBar.OnBossHPChanged(damage / maxHealth);
        if (_enemyBase.Health <= 0) OnDeath();
    }

    public override void OnDeath()
    {
        if (_isInCutscene) return;
        _isInCutscene = true;
        StopAllCoroutines();
        _isShootingBullets = false;
        StartCoroutine(OnDeathCoroutine());
    }

    private IEnumerator OnDeathCoroutine()
    {
        foreach (Rigidbody2D rb in _bouncingPartRigidBodies)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        
        float timer = 0f;
        while (timer <= 2f)
        {
            transform.position += 0.1f * timer * new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f), 0);
            yield return null;
            timer += Time.deltaTime;
        }
        
        _rigidBody.bodyType = RigidbodyType2D.Dynamic;
        _rigidBody.gravityScale = 1f;
        
        yield return new WaitForSeconds(5f);
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