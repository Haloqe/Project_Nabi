using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

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
    private readonly float _defaultMoveSpeed = 7f;
    public float MoveSpeed { get; private set; } 
    public float moveSpeedMultiplier = 1f;
    private Vector2 _moveDirection;
    public bool IsMoving { get; private set; }
    private bool _isRooted = false;
    public bool IsOnMovingPlatform = false;

    // jumping
    private readonly float _defaultJumpForce = 13f;
    public float jumpForce;
    private bool _isJumping;
    private bool _isRunningUpwardVelocityCoroutine;
    private int _jumpCounter;
    private readonly float _coyoteTime = 0.2f;
    private float _coyoteTimeCounter;
    private readonly float _jumpBufferTime = 0.2f;
    private float _jumpBufferTimeCounter;
    private float _upwardsGravityScale = 2f;
    private float _downwardsGravityScale = 5f;

    // attack
    private bool _isAttacking;
    public bool isDashing;
    public bool isAreaAttacking; //temp
    
    // Others
    private Vector2 _additionalVelocity;
    private readonly static int Moving = Animator.StringToHash("IsMoving");
    private readonly static int IsDoubleJumping = Animator.StringToHash("IsDoubleJumping");
    private readonly static int IsJumping = Animator.StringToHash("IsJumping");
    

    private void Start()
    {
        GameEvents.Restarted += OnRestarted;
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

        _mainCollider = GetComponents<BoxCollider2D>()[0];
        _defaultBounciness = _mainCollider.sharedMaterial.bounciness;
        _defaultFriction = _mainCollider.sharedMaterial.friction;
    }

    private void OnDestroy()
    {
        GameEvents.Restarted -= OnRestarted;
        InGameEvents.TimeSlowDown -= OnTimeSlowDown;
        InGameEvents.TimeRevertNormal -= OnTimeRevertNormal;
    }

    private void OnRestarted()
    {
        RemoveDebuffs();
        ResetFriction();
        ResetBounciness();
        _isAttacking = false;
        isDashing = false;
        isAreaAttacking = false;
        _jumpCounter = 0;
        _isJumping = false;
        moveSpeedMultiplier = 1f;
        OnTimeRevertNormal();
    }

    private void Update()
    {
        _animator.SetBool(Moving, IsMoving && !_isRooted);
        
        _coyoteTimeCounter -= Time.unscaledDeltaTime;
        _jumpBufferTimeCounter -= Time.unscaledDeltaTime;
    }

    private void FixedUpdate()
    {
        // disable extra movement if rooted or dashing or attacking
        if (_isRooted || isDashing || isAreaAttacking/*|| _isAttacking*/) return;
        
        _rigidbody2D.velocity = _additionalVelocity.magnitude != 0 ? 
            new Vector2(_additionalVelocity.x / Time.timeScale, _additionalVelocity.y + _rigidbody2D.velocity.y) : 
            new Vector2((_moveDirection.x * MoveSpeed * moveSpeedMultiplier) / Time.timeScale, _rigidbody2D.velocity.y);
        
        if (!_isJumping)
        {
            _rigidbody2D.gravityScale = _playerController.DefaultGravityScale;
            return;
        }
        
        if (_isRunningUpwardVelocityCoroutine && _jumpCounter <= 1)
        {
            _rigidbody2D.gravityScale = _upwardsGravityScale;
        }
        else if (!_isRunningUpwardVelocityCoroutine || _rigidbody2D.velocity.y < 0)
        {
            _rigidbody2D.gravityScale = _downwardsGravityScale;
        }
    }

    private void OnTimeRevertNormal()
    {
        _rigidbody2D.gravityScale = _playerController.DefaultGravityScale;
        FixVelocity();
    }
    
    private void OnTimeSlowDown()
    {
        _rigidbody2D.gravityScale = _playerController.DefaultGravityScale / (Time.timeScale * Time.timeScale);
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

    public void SetMoveDirection(Vector2 value)
    {
        _moveDirection = value;
        if (value.x == 0 || _isAttacking || isDashing)
        {
            IsMoving = false;
        }
        else
        {
            // Note: Player sprite default direction is left
            IsMoving = true;
            transform.localScale = new Vector2(-Mathf.Sign(value.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y);
        }
    }

    public void EnableMovement(bool strict)
    {
        if (strict)
        {
            _isRooted = false;
        }
        else
        {
            _isAttacking = false;
            if (_moveDirection.x != 0)
            {
                // Note: Player sprite default direction is left
                IsMoving = true;
                transform.localScale = new Vector2(-Mathf.Sign(_moveDirection.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y);
            }
        }
    }

    public void DisableMovement(bool strict)
    {
        if (strict)
        {
            _isRooted = true;
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

    // public void SetJump(bool value)
    // {
    //     // Can the player jump?
    //     if (!value) return;
    //     if (isDashing) return;
    //     if (_isRooted || _isAttacking) return;
    //     if (_coyoteTimeCounter < 0f && _jumpCounter >= 2) return;
    //     
    //     // Is this a second jump?
    //     if (_jumpCounter > 1) {
    //         _jumpBufferTimeCounter = _jumpBufferTime;
    //         _animator.SetBool(IsDoubleJumping, true);
    //     }
    //
    //     _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, jumpForce / Time.timeScale);
    //     _jumpCounter += 1;
    //     _jumpBufferTimeCounter = 0;
    // }

    public void StartJump()
    {
        // can the player jump?
        if (isDashing) return;
        if (_isRooted || _isAttacking) return;
        if (_coyoteTimeCounter < 0f && _jumpCounter >= 2) return;
        
        // is this a second jump?
        if (_jumpCounter >= 1)
        {
            _jumpBufferTimeCounter = _jumpBufferTime;
            _animator.SetBool(IsDoubleJumping, true);
        }
        
        StartCoroutine(nameof(UpwardVelocityCoroutine));
        _jumpCounter += 1;
        _jumpBufferTimeCounter = 0;
    }

    public void StopJump()
    {
        StopCoroutine(nameof(UpwardVelocityCoroutine));
        _isRunningUpwardVelocityCoroutine = false;
    }

    private IEnumerator UpwardVelocityCoroutine()
    {
        _isRunningUpwardVelocityCoroutine = true;
        
        float jumpTimeCounter = 0f;
        while (jumpTimeCounter <= 0.25f)
        {
            yield return null;
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, jumpForce / Time.timeScale);
            jumpTimeCounter += Time.unscaledDeltaTime;
        }
        
        _isRunningUpwardVelocityCoroutine = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ground")) return;
        
        // Movement component only handles collision with the ground
        _isJumping = false;
        _jumpCounter = 0;
        _coyoteTimeCounter = _coyoteTime;
        _animator.SetBool(IsJumping, _isJumping);
        _animator.SetBool(IsDoubleJumping, _isJumping);
        // if (_jumpBufferTimeCounter > 0f) SetJump(true);
        if (_jumpBufferTimeCounter > 0f) StartJump();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            _isJumping = true;
            _animator.SetBool(IsJumping, _isJumping);
        }
    }

    public void RemoveDebuffs()
    {
        if (MoveSpeed <= _defaultMoveSpeed) MoveSpeed = _defaultMoveSpeed;
        if (jumpForce <= _defaultJumpForce) jumpForce = _defaultJumpForce;
        _isRooted = false;
    }

    #region Animation Event Handlers
    // Move horizontally to the direction the character is facing
    public void AnimEvent_StartMoveHorizontal(float xVelocity)
    {
        if (_isRooted) return;

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
