using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private PlayerMovement _playerMovement;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playerMovement = GetComponent<PlayerMovement>();
        }

        void OnMove(InputValue value)
        {
            _playerMovement.SetMoveDirection(value.Get<Vector2>());
        }

        void OnJump(InputValue value)
        {
            _playerMovement.SetJump(value.isPressed);
        }

        void OnCast_1(InputAction value)
        {
        
        }
    }
}
