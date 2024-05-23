using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FullscreenEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Butterfly : MonoBehaviour
{
    // Variables
    private readonly float _lifeTime = 30f;
    private readonly float _detectInterval = 1f;    // Time interval to detect if enemy is in range
    private readonly float _attackSpeed = 1f;       // Speed of the butterfly while attacking
    private readonly float _flySpeed = 0.022f;      // Speed of the butterfly while traveling towards target
    private readonly float _cloverSize = 1.7f;      // Size of the clover leaves
    private float _attackSpeedMultiplier = 1f;
    private int _buffStack = 0;
    private float _theta = 0f;                      // Angle for the parametric equation
    public float extraRelativeDamage = 0.0f;
    private bool _isDying;
    
    // Targets
    private Transform _enemy;                       // The enemy the butterfly is targeting
    private IDamageable _enemyDamageable;
    private Transform _player;
    private Vector3 _targetOffset;                  // Position offset from the target's pivot
    
    // Active Coroutines
    private bool _isAttacking;
    private Coroutine _detectCoroutine;
    private Coroutine _flyCoroutine;
    
    // References
    public float relativeDamage;
    private EnemyVisibilityChecker _visibilityChecker;
    private AttackInfo _attackInfo;
    private PlayerDamageDealer _playerDamageDealer;
    private PlayerController _playerController;
    private Renderer _renderer;
    
    // VFX
    public ParticleSystem buffVFX;
    public ParticleSystem attackVFX;
    public ParticleSystem critVFX;

    private void Awake()
    {
        // Get references
        _renderer = GetComponent<Renderer>();
        _visibilityChecker = Camera.main.GetComponent<EnemyVisibilityChecker>();
        _playerController = PlayerController.Instance;
        _playerDamageDealer = _playerController.playerDamageDealer;
        
        // Initial position
        _player = PlayerController.Instance.transform;
        transform.position = _player.position + GetRandomOffsetNearPlayer();
        
        // Damage
        _attackInfo = new AttackInfo();
        _attackInfo.Damage.TotalAmount = PlayerController.Instance.Strength * (relativeDamage + extraRelativeDamage);
    }

    private void Start()
    {
        StartCoroutine(LifeTimeCoroutine());
        _detectCoroutine = StartCoroutine(DetectCoroutine());
    }

    private void Update()
    {
        // If is in combat, attack the alive enemy or return to player if enemy is dead 
        if (_isAttacking)
        {
            if (_enemy)
            {
                AttackTarget();
            }
            else
            {
                _isAttacking = false;
                _flyCoroutine = StartCoroutine(FlyCoroutine(_player));
            }
        }
        // If attached to the player and not flying towards a target, fix position
        else if (_flyCoroutine == null)
        {
            transform.position = _player.position + _targetOffset;

            if (_detectCoroutine == null) _detectCoroutine = StartCoroutine(DetectCoroutine());
        }
    }

    private IEnumerator DetectCoroutine()
    {
        while (true)
        {
            Transform enemy = GetRandomEnemyInRange();
            if (enemy)
            {
                // If it was on its way to the player, stop flying to the player and fly to the new target
                _detectCoroutine = null;
                if (_flyCoroutine != null) StopCoroutine(_flyCoroutine);
                _flyCoroutine = StartCoroutine(FlyCoroutine(enemy));
                yield break;
            }
            yield return new WaitForSeconds(_detectInterval);   
        }
    }

    private Transform GetRandomEnemyInRange(Transform exceptEnemy = null)
    {
        var visibleEnemies = _visibilityChecker.visibleEnemies;
        if (exceptEnemy) visibleEnemies.Remove(exceptEnemy.gameObject);
        return visibleEnemies.Count == 0 ? null : visibleEnemies[Random.Range(0, visibleEnemies.Count)].transform;
    }

    // Fly towards the target from its current position
    private IEnumerator FlyCoroutine(Transform target)
    { 
        // Compute a position to move to
        if (target == _player)
        {
            // If the target is player, move to a random spawn point
            _targetOffset = GetRandomOffsetNearPlayer();
        }
        else
        {
            var colliders = target.gameObject.GetComponents<Collider2D>().ToList();
            var hitbox = target.transform.Find("AttackHitbox");
            if (hitbox) colliders.AddRange(hitbox.GetComponents<Collider2D>().ToList());
            Bounds bounds = colliders[Random.Range(0, colliders.Count)].bounds;
            float x = Random.Range(bounds.min.x + _cloverSize / 2.0f, bounds.max.x - _cloverSize / 2.0f);
            float y = Random.Range(bounds.min.y + _cloverSize / 2.0f, bounds.max.y - _cloverSize / 2.0f);
            
            // Calculate the offset from the object's position
            _targetOffset = new Vector3(x, y, 0) - target.transform.position;
        }
        
        // Fly towards the target
        while (target && Vector3.Distance(transform.position, target.position + _targetOffset) > 0.001f)
        {
            // If enemy goes out of the screen, stop travelling
            if (target != _player && !_visibilityChecker.visibleEnemies.Contains(target.gameObject))
            {
                target = null;
                break;
            }
            transform.position = Vector3.MoveTowards(transform.position, target.position + _targetOffset, _flySpeed);
            yield return null;
        }
        
        // If enemy goes out of the screen or dies, return to player
        if (target == null)
        {
            _flyCoroutine = StartCoroutine(FlyCoroutine(_player));
        }
        // Target is reached; stop flying
        else
        {
            transform.position = target.position + _targetOffset;
            _flyCoroutine = null;
            
            // Enemy reached
            if (target != _player)
            {
                _enemy = target;
                _isAttacking = true;
                _enemyDamageable = _enemy.GetComponent<IDamageable>();
                Attack();
            }
            // Player reached
            else
            {
                _detectCoroutine = StartCoroutine(DetectCoroutine());
            }
        }
    }
    
    // Fly in a four-leaf clover shape and attack the target
    private float _timer = 0.0f;
    private void AttackTarget()
    {
        // Follow enemy position
        transform.position = _enemy.position + _targetOffset; // _enemy.transform.TransformPoint(_targetOffset);
        
        // Follow trajectory
        _theta += Time.unscaledDeltaTime * _attackSpeed * _attackSpeedMultiplier;
        float r = _cloverSize * Mathf.Sin(2 * _theta);
        float x = r * Mathf.Cos(_theta);
        float y = r * Mathf.Sin(_theta);
        transform.position += new Vector3(x, y, 0);
        
        // Update timer
        _timer += Time.unscaledDeltaTime / _attackSpeedMultiplier;
        
        // Attack enemy when at the centre
        if (_timer >= Mathf.PI / (2 * _attackSpeed * _attackSpeedMultiplier)) // use the calculated time interval
        {
            Attack();
            _timer = 0;
        }
    }

    private bool _isInvisble;
    // Fly back to player when not seen by the camera
    private void OnBecameInvisible()
    {
        if (_isInvisble) return;
        if (!isActiveAndEnabled) return;
        _isInvisble = true;
        
        // Return threshold
        float timer = 0;
        while (timer <= 2.0f)
        {
            timer += Time.unscaledDeltaTime;
            if (_renderer.isVisible)
            {
                _isInvisble = false;
                return;
            }
        }
        
        // Check if it can fly to another enemy
        Transform enemy = GetRandomEnemyInRange();
        _enemy = null;
        if (enemy)
        {
            _isAttacking = false;
            if (_flyCoroutine != null) StopCoroutine(_flyCoroutine);
            if (isActiveAndEnabled) _flyCoroutine = StartCoroutine(FlyCoroutine(enemy));
        }
        // If no other enemy to fly to, fly to the player
        else
        {
            if (isActiveAndEnabled) _flyCoroutine = StartCoroutine(FlyCoroutine(_player));
            _isAttacking = false;
        }
        _isInvisble = false;
    }

    private Vector3 GetRandomOffsetNearPlayer()
    {
        float radius = 1.7f;                     
        float angle = Random.Range(0, 190 * Mathf.Deg2Rad); // random angle in radians between 0 and 180 degrees

        // calculate x and y coordinates
        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle) + 1.0f; // add the player's height to the y coordinate
        _targetOffset = new Vector3(x, y, 0);
        return _targetOffset;
    }

    private IEnumerator LifeTimeCoroutine()
    {
        yield return new WaitForSeconds(_lifeTime);
        _playerDamageDealer.TurbelaKillButterfly(this);
    }

    public IEnumerator DieCoroutine()
    {
        _isDying = true;
        var spRenderer = GetComponent<SpriteRenderer>();
        var changeAmount = new Color(0, 0, 0, 0.0035f);
        while (spRenderer.color.a >= 0.0f)
        {
            spRenderer.color -= changeAmount;
            yield return null;
        }
        Destroy(gameObject);
    }

    private void Attack()
    {
        // Adjust for a critical damage
        var damageToSend = _attackInfo.Clone();
        var critChanceExtra = Define.TurbelaButterflyCritStats[(int)_playerController.TurbelaButterflyCritPreserv];
        if (Random.value <= _playerController.CriticalRate + critChanceExtra)
        {
            damageToSend.Damage.TotalAmount *= 2.0f;
            critVFX.Play();
        }
        else attackVFX.Play();
        
        _playerDamageDealer.DealDamage(_enemyDamageable, damageToSend);
    }

    public IEnumerator BuffCoroutine(float duration, float speedMultiplier)
    {
        // Do not apply buff if it is about to die
        if (_isDying) yield break;
        
        // Start buff
        buffVFX.Play();
        _attackSpeedMultiplier = speedMultiplier;
        _buffStack++;
        
        // End buff
        yield return new WaitForSeconds(duration);
        if (--_buffStack == 0) _attackSpeedMultiplier = 1.0f;
    }
}
