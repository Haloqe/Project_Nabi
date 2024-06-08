using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float _length, _startPosition;
    public GameObject Camera;
    public float ParallaxEffect;
    void Start()
    {
        _startPosition = transform.position.x;
        _length = GetComponentInChildren<SpriteRenderer>().bounds.size.x;
    }
    
    void Update()
    {
        float distance = Camera.transform.position.x * ParallaxEffect;
        float movement = Camera.transform.position.x * (1 - ParallaxEffect);

        transform.position = new Vector3(_startPosition + distance, transform.position.y, transform.position.z);

        if (movement > _startPosition + _length) _startPosition += _length;
        else if (movement < _startPosition - _length) _startPosition -= _length;
    }
}