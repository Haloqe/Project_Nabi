using Cinemachine;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // movement
    [SerializeField] public float DefaultMoveSpeed = 10f;
    private float _moveSpeed; 
    private Vector2 _moveDirection;
    private bool _isMoving;
    private bool _isRooted = false;

    // jumping
    [SerializeField] public float DefaultJumpForce = 10f;
    private float _jumpForce;
    private bool _isJumping;

    // attack
    private bool _isAttacking;
    public bool IsDashing;

    // others
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _moveSpeed = DefaultMoveSpeed;
        _jumpForce = DefaultJumpForce;
        _isJumping = true;
        _isMoving = false;
    }

    private void Update()
    {
        _animator.SetBool("IsMoving", _isMoving && !_isRooted);
    }

    private void FixedUpdate()
    {
        // disable extra movement if rooted or dashing
        if (_isRooted || IsDashing) return;

        // Only allow up, down movement during attack
        if (_isAttacking)
        {
            _rigidbody2D.velocity = new Vector2(0.0f, _rigidbody2D.velocity.y);
        }
        else
        {
            _rigidbody2D.velocity = new Vector2(_moveDirection.x * _moveSpeed, _rigidbody2D.velocity.y);
        }
    }

    ///<summary>Set move speed to default speed * percentage. If isSlowerThanNow is true, update speed only if 
    /// the current speed is slower than the new speed. If onlySpeed is set to false, update both jump force and move speed.
    ///</summary>
    // 디버프 슬로우는 중첩 X, 이속버프 + 슬로우는 중첩 O.
    public void ChangeSpeedByPercentage(float percentage, bool isSlowerThanNow = false, bool onlySpeed = false)
    {
        float newSpeed = DefaultMoveSpeed * percentage;

        // if has buff
        if (_moveSpeed > DefaultMoveSpeed)
        {
            newSpeed = _moveSpeed * percentage;
        }
            
        if (!isSlowerThanNow || _moveSpeed > newSpeed)
        {
            _moveSpeed = newSpeed;
            if (!onlySpeed) _jumpForce = DefaultJumpForce * percentage;
        }
    }

    public void SetMoveSpeedForDuration(float percentage, float duration)
    {
        ChangeSpeedByPercentage(percentage, false, true);
        StartCoroutine(ResetSpeedAfterDuration(duration));
    }

    private IEnumerator ResetSpeedAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        ResetMoveSpeed();
        FindObjectOfType<PlayerCombat>().SetActiveSlow();
    }

    public void ResetMoveSpeed()
    {
        _moveSpeed = DefaultMoveSpeed;
        _jumpForce = DefaultJumpForce;
    }

    public void SetMoveDirection(Vector2 value)
    {
        _moveDirection = value;
        if (value.x == 0 || _isAttacking || IsDashing)
        {
            _isMoving = false;
        }
        else
        {
            // Note: Player sprite default direction is left
            _isMoving = true;
            transform.localScale = new Vector2(-Mathf.Sign(value.x), transform.localScale.y);
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
                _isMoving = true;
                transform.localScale = new Vector2(-Mathf.Sign(_moveDirection.x), transform.localScale.y);
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
            _isMoving = false;
        }
    }

    public void SetJump(bool value)
    {
        if (_isJumping || _isRooted || _isAttacking) return;
        if (!value) return;

        _rigidbody2D.velocity += (new Vector2(0, _jumpForce));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            _isJumping = false;
            _animator.SetBool("IsJumping", _isJumping);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            _isJumping = true;
            _animator.SetBool("IsJumping", _isJumping);
        }
    }

    public void RemoveDebuffs()
    {
        if (_moveSpeed <= DefaultMoveSpeed) _moveSpeed = DefaultMoveSpeed;
        if (_jumpForce <= DefaultJumpForce) _jumpForce = DefaultJumpForce;
        _isRooted = false;
    }
}
