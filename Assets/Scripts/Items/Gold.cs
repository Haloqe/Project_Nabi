using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gold : MonoBehaviour
{
    public int value;
    private Rigidbody2D _rb;
    private Collider2D _collider;
    private Transform _playerTransform;
    private bool _isMovingTowardsPlayer;
    private bool _isInteracting;
    public float impulseForce = 4f;
    private float _lifetimeThreshold = 30.0f;
    private float _distanceThreshold = 100f;
    
    public void SetForce(float force) => impulseForce = force;
    
    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _rb = GetComponent<Rigidbody2D>();
        _playerTransform = PlayerController.Instance.transform;
        _collider.enabled = false;
    }
    
    private void Start()
    {
        // Apply a force to the coin in a random direction
        Vector3 force = Random.onUnitSphere;
        force.z = 0.0f;
        _rb.AddForce(force.normalized * impulseForce, ForceMode2D.Impulse);
        _rb.drag = 1.5f;
        StartCoroutine(WaitCoroutine());
        StartCoroutine(LifetimeCoroutine());
    }

    private IEnumerator WaitCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        _rb.velocity = Vector2.zero;
        _isMovingTowardsPlayer = true;
        _collider.enabled = true;
    }

    private IEnumerator LifetimeCoroutine()
    {
        yield return new WaitForSecondsRealtime(_lifetimeThreshold);
        AutoCollectGold();
    }
    
    private void Update()
    {
        if (_isInteracting || !_isMovingTowardsPlayer) return;
        transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position + Vector3.up, 0.45f);
        if (Vector3.Distance(transform.position, _playerTransform.position) >= _distanceThreshold) AutoCollectGold();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isInteracting) return;
        _isInteracting = true;
        
        StopAllCoroutines();
        PlayerController.Instance.playerInventory.ChangeGoldByAmount(value);
        Destroy(gameObject);
    }

    private void AutoCollectGold()
    {
        PlayerController.Instance.playerInventory.ChangeGoldByAmount(value);
        Destroy(gameObject);
    }
}
