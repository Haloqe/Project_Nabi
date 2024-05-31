using System;
using UnityEngine;

public class FireChild : MonoBehaviour
{
    private Fire _parent;
    private CircleCollider2D _collider;
    private SpriteRenderer _renderer;
    private ParticleSystem _part;
    GameObject ChildGameObject1;

    private void Awake()
    {
        _parent = transform.parent.GetComponent<Fire>();
      
        _collider = GetComponent<CircleCollider2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _part = GetComponent<ParticleSystem>();
        ChildGameObject1 = gameObject.transform.GetChild(0).GetChild(0).gameObject;
        Debug.Log(ChildGameObject1);
    }

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("hit!");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        _parent.OnEnemyEnter(collision);
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("collision detected!");
        ChildGameObject1.SetActive(true);
        Debug.Log(ChildGameObject1);
        Debug.Log(ChildGameObject1.activeSelf);
        
    }

    public void Toggle()
    {
        _collider.enabled = !_collider.enabled;  
        //_renderer.enabled = !_renderer.enabled;  
    }
}