using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 10f;
    [SerializeField] public float jumpForce = 10f;
    private Rigidbody2D _rigidbody2D;
    private CapsuleCollider2D _capsuleCollider2D;
    private Vector2 _moveDirection;
    private bool _isJumping;
    private bool _isJumpPressed = false;
    

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _capsuleCollider2D = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate()
    {
        _rigidbody2D.velocity = new Vector2(_moveDirection.x * moveSpeed, _rigidbody2D.velocity.y);
        if (_isJumpPressed && !_isJumping)
        {
            //_rigidbody2D.AddForce(new Vector2(0, jumpForce));
            _rigidbody2D.velocity += (new Vector2(0, jumpForce));
        }
    }

    public void SetMoveDirection(Vector2 value)
    {
        _moveDirection = value;
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
}
