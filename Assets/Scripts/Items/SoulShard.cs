using System;
using System.Collections;
using UnityEngine;

public class SoulShard : MonoBehaviour
{
    private bool _isInteracting;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }
    
    private IEnumerator BounceCoroutine()
    {
        Vector3 lowestPos = transform.position;
        Vector3 highestPos = transform.position + new Vector3(0, 0.3f, 0);
        
        while (true)
        {
            for (float time = 0; time < 2 * 2; time += Time.unscaledDeltaTime)
            {
                float progress = Mathf.PingPong(time, 2) / 2;
                transform.position = Vector3.Lerp(lowestPos, highestPos, progress);
                yield return null;
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isInteracting) return;
        _isInteracting = true;
        PlayerController.Instance.playerInventory.ChangeSoulShardByAmount(1);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("CollisionEnter");
        if (_rb.velocity.y <= 0.01f)
        {
            Debug.Log("StartBounce");
            _rb.gravityScale = 0f;
            StartCoroutine(BounceCoroutine());
        }
    }
}