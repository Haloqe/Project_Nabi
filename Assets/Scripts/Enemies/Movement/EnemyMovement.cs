using UnityEngine;

public abstract class EnemyMovement : MonoBehaviour
{
    [SerializeField] protected float DefaultMoveSpeed = 1f;
    protected float _moveSpeed;
    protected bool _isRooted = false;
    protected Rigidbody2D _rigidBody;

    protected virtual void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _moveSpeed = DefaultMoveSpeed;
        EnableDisableMovement(true);
    }

    public virtual void ResetDefaultMoveSpeed()
    {
        _moveSpeed = DefaultMoveSpeed;
    }

    public virtual void EnableDisableMovement(bool shouldEnable)
    {
        _isRooted = !shouldEnable;
        if (!shouldEnable)
        {
            _rigidBody.velocity = Vector2.zero;
        }
    }
}