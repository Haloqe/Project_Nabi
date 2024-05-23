using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    public Vector3 MinOffset;
    public Vector3 MaxOffset;
    public float Speed;

    private bool _isPlayerOnPlatform;
    private Vector3 _minPos;
    private Vector3 _maxPos;
    public bool isMovingTowardsMin;

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _minPos = transform.position - MinOffset;
        _maxPos = transform.position + MaxOffset;
        StartCoroutine(MoveCoroutine());
    }

    IEnumerator MoveCoroutine()
    {
        while (true)
        {
            Vector3 target = isMovingTowardsMin ? _minPos : _maxPos;
            _rigidbody2D.velocity = (target - transform.position).normalized * Speed;
            while (Vector3.Distance(transform.position, target) > 0.01f) yield return null;
            isMovingTowardsMin = !isMovingTowardsMin;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.gameObject;
        if (player.CompareTag("Player"))
        {
            var movementComp = player.GetComponent<PlayerMovement>();
            if (movementComp.IsOnMovingPlatform) return;
            
            _isPlayerOnPlatform = true;
            movementComp.IsOnMovingPlatform = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_isPlayerOnPlatform) return;

        var player = collision.gameObject;
        if (player.CompareTag("Player"))
        {
            _isPlayerOnPlatform = false;
            player.GetComponent<PlayerMovement>().IsOnMovingPlatform = false;
            player.transform.rotation = Quaternion.identity;
            player.transform.localScale = new Vector3(player.transform.localScale.x, 1, 1);
        }
    }
}
