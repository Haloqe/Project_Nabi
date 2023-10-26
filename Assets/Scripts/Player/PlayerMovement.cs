using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        // disable movement if rooted
        if (_isRooted) return;
            
        _rigidbody2D.velocity = new Vector2(_moveDirection.x * _moveSpeed, _rigidbody2D.velocity.y);
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
        if (value.x != 0)
        {
            _isMoving = true;
            transform.localScale = new Vector2(Mathf.Sign(value.x), transform.localScale.y);
        }
        else _isMoving = false;
    }

    public void EnableDisableMovement(bool shouldEnable)
    {
        _isRooted = !shouldEnable;
        if (!shouldEnable)
        {
            _rigidbody2D.velocity = Vector2.zero;
        }
    }

    public void SetJump(bool value)
    {
        if (_isJumping || _isRooted) return;
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
