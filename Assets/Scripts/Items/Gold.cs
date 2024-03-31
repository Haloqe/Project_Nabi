using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gold : MonoBehaviour
{
    public int value;
    private Rigidbody2D _rb;
    private Transform _playerTransform;
    private bool _isMovingTowardsPlayer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerTransform = PlayerController.Instance.transform;
    }
    
    private void Start()
    {
        // Apply a force to the coin in a random direction
        Vector3 force = Random.onUnitSphere;
        force.z = 0.0f;
        //force.y = Mathf.Abs(force.y); // Ensure the force is always upwards
        _rb.AddForce(force.normalized * 2.8f, ForceMode2D.Impulse);
        _rb.drag = 1.5f;
        StartCoroutine(WaitCoroutine());
    }

    private IEnumerator WaitCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        _rb.velocity = Vector2.zero;
        _isMovingTowardsPlayer = true;
    }
    
    private void Update()
    {
        if (!_isMovingTowardsPlayer) return;
        transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position + Vector3.up, 0.35f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerController.Instance.playerInventory.ChangeGoldByAmount(value);
        Destroy(gameObject);
    }
}