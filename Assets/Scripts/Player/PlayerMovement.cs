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
    private bool _isRooted = false;

    // jumping
    [SerializeField] public float DefaultJumpForce = 10f;
    private float _jumpForce;
    private bool _isJumping;
    private bool _isJumpPressed = false;

    // others
    private Rigidbody2D _rigidbody2D;
    
    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _moveSpeed = DefaultMoveSpeed;
        _jumpForce = DefaultJumpForce;
    }

    private void FixedUpdate()
    {
        // disable movement if rooted
        if (_isRooted) return;
            
        _rigidbody2D.velocity = new Vector2(_moveDirection.x * _moveSpeed, _rigidbody2D.velocity.y);
        if (_isJumpPressed && !_isJumping)
        {
            //_rigidbody2D.AddForce(new Vector2(0, jumpForce));
            _rigidbody2D.velocity += (new Vector2(0, _jumpForce));
        }
    }

    ///<summary>Set move speed to default speed * percentage. If isSlowerThanNow is true, update speed only if 
    /// the current speed is slower than the new speed. If onlySpeed is set to false, update both jump force and move speed.
    ///</summary>
    // 디버프 슬로우는 중첩 X, 이속버프 + 슬로우는 중첩 O.
    public void ChangeSpeedByPercentage(float percentage, bool isSlowerThanNow = false, bool onlySpeed = false)
    {
        float newSpeed = DefaultMoveSpeed * percentage;
        if (_moveSpeed > DefaultMoveSpeed) // if has buff
            newSpeed = _moveSpeed * percentage;

        if (_moveSpeed > newSpeed || !isSlowerThanNow)
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
    }

    public void SetMoveDirection(Vector2 value)
    {
        _moveDirection = value;
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
        _isJumpPressed = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            _isJumping = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            _isJumping = true;
        }
    }

    public void RemoveDebuffs()
    {
        if (_moveSpeed <= DefaultMoveSpeed) _moveSpeed = DefaultMoveSpeed;
        if (_jumpForce <= DefaultJumpForce) _jumpForce = DefaultJumpForce;
        _isRooted = false;
    }
}
