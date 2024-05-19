using System;
using UnityEngine;

public class FireChild : MonoBehaviour
{
    private Fire _parent;
    private CircleCollider2D _collider;
    private SpriteRenderer _renderer;
    private ParticleSystem _part;

    private void Awake()
    {
        _parent = transform.parent.GetComponent<Fire>();
        _collider = GetComponent<CircleCollider2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _part = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("hit!");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        /*
        if (collision.gameObject.CompareTag("Ground")){
            Debug.Log("hit ground!");
            _parent.OnGroundContact(collision);
        }*/
            
        if (collision.gameObject.CompareTag("Enemy")){
            _parent.OnEnemyEnter(collision);
        }
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy") == false) return;
    }

    public void Toggle()
    {
        _collider.enabled = !_collider.enabled;  
        //_renderer.enabled = !_renderer.enabled;  
    }
}