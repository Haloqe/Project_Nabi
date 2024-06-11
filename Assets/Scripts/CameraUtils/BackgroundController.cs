using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float _lengthX, _lengthY, _startPositionX, _startPositionY;
    public GameObject Camera;
    public float ParallaxEffect;
    void Start()
    {
        _startPositionX = transform.position.x;
        _startPositionY = transform.position.y;
        _lengthX = GetComponentInChildren<SpriteRenderer>().bounds.size.x;
        _lengthY = GetComponentInChildren<SpriteRenderer>().bounds.size.y;
    }
    
    void Update()
    {
        float distanceX = Camera.transform.position.x * ParallaxEffect;
        float movementX = Camera.transform.position.x * (1 - ParallaxEffect);
        float distanceY = Camera.transform.position.y * ParallaxEffect;
        float movementY = Camera.transform.position.y * (1 - ParallaxEffect);

        transform.position =
            new Vector3(_startPositionX + distanceX, _startPositionY + distanceY, transform.position.z);

        if (movementX > _startPositionX + _lengthX) _startPositionX += 2 * _lengthX;
        else if (movementX < _startPositionX - _lengthX) _startPositionX -= 2 * _lengthX;
        if (movementY > _startPositionY + _lengthY) _startPositionY += 2 * _lengthY;
        else if (movementY < _startPositionY - _lengthY) _startPositionY -= 2 * _lengthY;
    }
}