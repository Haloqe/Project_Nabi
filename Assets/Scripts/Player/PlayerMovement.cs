using System.Collections;
using Cinemachine;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // References
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private PlayerController _playerController;
    
    // physics
    private BoxCollider2D _mainCollider;
    private float _defaultFriction;
    private float _defaultBounciness;

    // movement
    private bool _isMoveDisabled;
    private readonly float _defaultMoveSpeed = 9f;
    public float MoveSpeed { get; private set; } 
    public float moveSpeedMultiplier = 1f;
    private float _moveDirection;
    public bool IsMoving { get; private set; }
    public bool IsRooted { get; private set; }
    public bool isOnMovingPlatform;
    private Vector2 _additionalVelocity;

    // jumping
    private int _enteredGroundCount;
    private readonly float _defaultJumpForce = 13f;
    public float jumpForce;
    private bool _isJumping;
    private bool _isRunningFirstJump;
    private int _jumpCounter;
    private readonly float _coyoteTime = 0.2f;
    private float _coyoteTimeCounter;
    private readonly float _jumpBufferTime = 0.3f;
    private float _jumpBufferTimeCounter;
    public float UpwardsGravityScale { get; private set; }
    public float DownwardsGravityScale { get; private set; }
    
    // attack
    private bool _isAttacking;
    public bool isDashing;
    public bool isAreaAttacking; 
    public bool isRangedAttacking;
    
    // camera
    private CameraManager _cameraManager;
    private CameraFollowObject _cameraFollowObject;
    private float _fallSpeedYDampingChangeThreshold;

    // animator
    private readonly static int Moving = Animator.StringToHash("IsMoving");
    private readonly static int IsDoubleJumping = Animator.StringToHash("IsDoubleJumping");
    private readonly static int IsJumping = Animator.StringToHash("IsJumping");

    private void Start()
    {
        GameEvents.Restarted += OnRestarted;
        PlayerEvents.Defeated += OnDefeated;
        // PlayerEvents.Spawned += OnSpawned;
        InGameEvents.TimeSlowDown += OnTimeSlowDown;
        InGameEvents.TimeRevertNormal += OnTimeRevertNormal;
        
        _playerController = PlayerController.Instance;
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        MoveSpeed = _defaultMoveSpeed;
        jumpForce = _defaultJumpForce;
        _jumpCounter = 0;
        _isJumping = true;
        IsMoving = false;
        UpwardsGravityScale = 2f;
        DownwardsGravityScale = 4.5f;

        _cameraManager = CameraManager.Instance;
        _fallSpeedYDampingChangeThreshold = _cameraManager.FallSpeedYDampingChangeThreshold;
        _cameraFollowObject = GameObject.Find("CameraFollowingObject").GetComponent<CameraFollowObject>();

        _mainCollider = GetComponents<BoxCollider2D>()[0];
        _defaultBounciness = _mainCollider.sharedMaterial.bounciness;
        _defaultFriction = _mainCollider.sharedMaterial.friction;
    }

    private void OnDestroy()
    {
        GameEvents.Restarted -= OnRestarted;
        PlayerEvents.Defeated -= OnDefeated;
        InGameEvents.TimeSlowDown -= OnTimeSlowDown;
        InGameEvents.TimeRevertNormal -= OnTimeRevertNormal;
    }

    private void OnDefeated()
    {
        _moveDirection = 0;
        _additionalVelocity = Vector2.zero;
    }

    // private void OnSpawned()
    // {
    //     Debug.Log("player spawning rnnnn");
    //     GameObject followObject = new GameObject("CameraFollowingObject");
    //     _cameraFollowObject = followObject.AddComponent<CameraFollowObject>();
    //     GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>().Follow = followObject.transform;
    //     
    // }
    
    private void OnRestarted()
    {
        _isAttacking = false;
        isDashing = false; 
        isAreaAttacking = false;
        _isJumping = false;
        _jumpCounter = 0;
        _enteredGroundCount = 0;
        moveSpeedMultiplier = 1f;
        
        RemoveDebuffs();
        ResetFriction();
        ResetBounciness();
        ResetMoveSpeed();
        OnTimeRevertNormal();
    }

    private void Update()
    {
        _animator.SetBool(Moving, IsMoving && !IsRooted);
        _coyoteTimeCounter -= Time.unscaledDeltaTime;
        _jumpBufferTimeCounter -= Time.unscaledDeltaTime;

        // if player is falling past a certain speed threshold
        if (_rigidbody2D.velocity.y < _fallSpeedYDampingChangeThreshold
            && !_cameraManager.IsLerpingYDamping
            && !_cameraManager.LerpedFromPlayerFalling)
        {
            _cameraManager.LerpYDamping(true);
        }

        // if player is standing still or moving up
        if (_rigidbody2D.velocity.y >= 0f
            && !_cameraManager.IsLerpingYDamping
            && _cameraManager.LerpedFromPlayerFalling)
        {
            _cameraManager.LerpedFromPlayerFalling = false;
            _cameraManager.LerpYDamping(false);
        }
    }

    private void FixedUpdate()
    {
        // Disable extra movement if is rooted, dashing, or area-attacking
        if (IsRooted || isDashing || isAreaAttacking) return;

        // Movement-driven velocity
        _rigidbody2D.velocity =
            new Vector2((_moveDirection * MoveSpeed * moveSpeedMultiplier) / Time.timeScale, _rigidbody2D.velocity.y);

        // Animation-driven velocity
        if (_isMoveDisabled)
            _rigidbody2D.velocity = Vector2.zero;
        else if (_additionalVelocity.magnitude != 0)
            _rigidbody2D.velocity = _additionalVelocity / Time.timeScale;
 
        if (!isRangedAttacking && _isJumping && (!_isRunningFirstJump || _rigidbody2D.velocity.y < 0))
        {
            _rigidbody2D.gravityScale = DownwardsGravityScale;
            if (Time.timeScale != 1) _rigidbody2D.gravityScale /= (Time.timeScale * Time.timeScale);
        }

        if (_rigidbody2D.velocity.y < -35f)
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, -35f);
    }
    
    private void OnTimeRevertNormal()
    {
        if (!_isJumping)
        {
            _rigidbody2D.gravityScale = _playerController.DefaultGravityScale;
        }
        else 
        {
            if (_isRunningFirstJump && _jumpCounter <= 1)
            {
                _rigidbody2D.gravityScale = UpwardsGravityScale;
            }
            else if (!_isRunningFirstJump || _rigidbody2D.velocity.y < 0)
            {
                _rigidbody2D.gravityScale = DownwardsGravityScale;
            }
        }
        FixVelocity();
    }
    
    private void OnTimeSlowDown()
    {
        if (!_isJumping)
        {
            _rigidbody2D.gravityScale = _playerController.DefaultGravityScale;
        }
        else 
        {
            if (_isRunningFirstJump && _jumpCounter <= 1)
            {
                _rigidbody2D.gravityScale = UpwardsGravityScale;
            }
            else if (!_isRunningFirstJump || _rigidbody2D.velocity.y < 0)
            {
                _rigidbody2D.gravityScale = DownwardsGravityScale;
            }
        }
        _rigidbody2D.gravityScale /= Time.timeScale * Time.timeScale;
        FixVelocity();
    }

    // TODO
    private void FixVelocity()
    {
        _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _rigidbody2D.velocity.y / Time.timeScale);
    }

    ///<summary>
    /// Set move speed to default speed * percentage. If isSlowerThanNow is true, update speed only if 
    /// the current speed is slower than the new speed. If onlySpeed is set to false, update both jump force and move speed.
    ///</summary>
    // 디버프 슬로우는 중첩 X, 이속버프 + 슬로우는 중첩 O.
    public void ChangeSpeedByPercentage(float percentage, bool isSlowerThanNow = false, bool onlySpeed = false)
    {
        float newSpeed = _defaultMoveSpeed * percentage;

        // if has buff
        if (MoveSpeed > _defaultMoveSpeed)
        {
            newSpeed = MoveSpeed * percentage;
        }
            
        if (!isSlowerThanNow || MoveSpeed > newSpeed)
        {
            MoveSpeed = newSpeed;
            if (!onlySpeed) jumpForce = _defaultJumpForce * percentage;
        }
    }

    public void SetMoveSpeedForDuration(float percentage, float duration)
    {
        ChangeSpeedByPercentage(percentage, false, true);
        StartCoroutine(ResetSpeedAfterDuration(duration));
    }

    private IEnumerator ResetSpeedAfterDuration(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        ResetMoveSpeed();
        gameObject.GetComponent<PlayerDamageReceiver>().SetActiveSlow();
    }

    public void ResetMoveSpeed()
    {
        MoveSpeed = _defaultMoveSpeed;
        jumpForce = _defaultJumpForce;
    }
    
    public void SetMoveDirection(float value)
    {
        _moveDirection = value;
        if (value == 0)
        {
            IsMoving = false;
        }
        else if (_isAttacking || isDashing)
        {
            if (value != 0 && _playerController.playerDamageDealer.IsAttackBufferAvailable)
            {
                Debug.Log("Updated: " + savedLookDirection);
                savedLookDirection = value;
            }
        }
        else
        {
            // Note: Player sprite default direction is left
            IsMoving = true;
            transform.localScale =
                new Vector2(-Mathf.Sign(value) * Mathf.Abs(transform.localScale.x), transform.localScale.y);
            
            _cameraFollowObject.TurnCamera();
        }
    }

    public float savedLookDirection = 0;
    public void UpdateLookDirectionOnAttackEnd()
    {
        transform.localScale = new Vector2(-Mathf.Sign(savedLookDirection) 
            * Mathf.Abs(transform.localScale.x), transform.localScale.y);
    }

    public void EnableMovement(bool strict)
    {
        if (strict)
        {
            IsRooted = false;
        }
        else
        {
            _isAttacking = false;
            if (_moveDirection != 0)
            {
                // Note: Player sprite default direction is left
                IsMoving = true;
                transform.localScale = new Vector2(-Mathf.Sign(_moveDirection) * Mathf.Abs(transform.localScale.x), transform.localScale.y);
            }
        }
    }

    public void DisableMovement(bool strict)
    {
        if (strict)
        {
            IsRooted = true;
            _rigidbody2D.velocity = Vector2.zero;
        }
        else
        {
            _isAttacking = true;
            IsMoving = false;
        }
    }

    public void SetDash()
    {
        isDashing = true;
        IsMoving = false;
    }
    
    public void StartJump()
    {
        // can the player jump?
        if (IsRooted) return;
        
        // Counter
        if (_coyoteTimeCounter < 0f && _jumpCounter >= 2 || isDashing || _isAttacking)
        {
            _jumpBufferTimeCounter = _jumpBufferTime;
            return;
        }
        
        // is this a second jump?
        _jumpCounter += 1;
        _jumpBufferTimeCounter = 0;
        if (_jumpCounter >= 2)
        {
            _animator.SetBool(IsDoubleJumping, true);
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, jumpForce * 1.3f / Time.timeScale);
            return;
        }
        
        // Set gravity scale
        _rigidbody2D.gravityScale = UpwardsGravityScale;
        if (Time.timeScale != 1) _rigidbody2D.gravityScale /= Time.timeScale * Time.timeScale;
        
        // Start jump
        StartCoroutine(nameof(FirstJump));
    }

    public void StopJump()
    {
        StopCoroutine(nameof(FirstJump));
        _isRunningFirstJump = false;
    }

    private IEnumerator FirstJump()
    {
        _isRunningFirstJump = true;
        
        float jumpTimeCounter = 0f;
        while (jumpTimeCounter <= 0.2f)
        {
            if (isDashing) break;
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, jumpForce / Time.timeScale);
            jumpTimeCounter += Time.unscaledDeltaTime;
            yield return null;
        }
        
        _isRunningFirstJump = false;
    }
    
    public void ResetEnteredGroundCount() => _enteredGroundCount = 0;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ground")) return;
        if (++_enteredGroundCount > 1) return;
        
        // Ground
        _isJumping = false;
        _jumpCounter = 0;
        _coyoteTimeCounter = _coyoteTime;
        _rigidbody2D.gravityScale = _playerController.DefaultGravityScale;
        _animator.SetBool(IsJumping, _isJumping);
        _animator.SetBool(IsDoubleJumping, _isJumping);
        if (_jumpBufferTimeCounter > 0f) StartJump();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Ground")) return;
        if (--_enteredGroundCount > 0) return;
        
        // Not on the ground
        _isJumping = true;
        StartCoroutine(JumpTriggerDelayCoroutine());
    }

    private IEnumerator JumpTriggerDelayCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        _animator.SetBool(IsJumping, _isJumping);
    }

    public void RemoveDebuffs()
    {
        if (MoveSpeed <= _defaultMoveSpeed) MoveSpeed = _defaultMoveSpeed;
        if (jumpForce <= _defaultJumpForce) jumpForce = _defaultJumpForce;
        IsRooted = false;
    }

    #region Animation Event Handlers
    public void AnimEvent_StopMove()
    {
        _isMoveDisabled = true;
    }
    
    public void AnimEvent_StartMove()
    {
        _isMoveDisabled = false;
    }
    
    // Move horizontally to the direction the character is facing
    public void AnimEvent_StartMoveHorizontal(float xVelocity)
    {
        if (IsRooted) return;
        
        // Cast a ray to check a wall
        Vector2 playerCentre = new Vector2(transform.position.x, transform.position.y + 0.1f);
        Vector2 faceDir = new Vector2(-Mathf.Sign(gameObject.transform.localScale.x), 0f);
        RaycastHit2D hit = Physics2D.Raycast(playerCentre, faceDir, 1.0f);
        if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Platform")) return;
        
        // If no wall in front, move forward
        float dir = -Mathf.Sign(gameObject.transform.localScale.x);
        _additionalVelocity = new Vector2(_additionalVelocity.x + dir * xVelocity, _additionalVelocity.y);
    }
    public void AnimEvent_StartMoveVertical(float yVelocity)
    {
        _additionalVelocity = new Vector2(_additionalVelocity.x, yVelocity);
    }

    public void AnimEvent_StopMoveHorizontal()
    {
        _additionalVelocity = new Vector2(0, _additionalVelocity.y);
    }

    public void AnimEvent_StopMoveVertical()
    {
        _additionalVelocity = new Vector2(_additionalVelocity.x, 0);
    }

    public void MoveForSeconds(Vector2 velocity, float seconds)
    {
        StartCoroutine(MoveCoroutine(velocity, seconds));
    }

    private IEnumerator MoveCoroutine(Vector2 velocity, float seconds)
    {
        _rigidbody2D.velocity += velocity;
        yield return new WaitForSecondsRealtime(seconds);
        _rigidbody2D.velocity -= velocity;
    }
    #endregion 


    #region Physics Settings
    public void SetFriction(float friction)
    {
        _mainCollider.sharedMaterial.friction = friction;
        _mainCollider.enabled = false;
        _mainCollider.enabled = true;
    }

    public void ResetFriction()
    {
        SetFriction(_defaultFriction);
    }

    public void SetBounciness(float bounciness)
    {
        _mainCollider.sharedMaterial.bounciness = bounciness;
        _mainCollider.enabled = false;
        _mainCollider.enabled = true;
    }

    public void ResetBounciness()
    {
        SetBounciness(_defaultBounciness);
    }
    #endregion
}
