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
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        _parent.OnEnemyEnter(collision);
    }

    public void ToggleCollider()
    {
        _collider.enabled = !_collider.enabled;  
    }
}