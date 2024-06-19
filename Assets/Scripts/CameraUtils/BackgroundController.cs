using Unity.VisualScripting;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float _lengthX, _lengthY, _startPositionX, _startPositionY;
    public GameObject _camera;
    public float ParallaxEffect;
    void Start()
    {
        _camera = CameraManager.Instance.inGameMainCamera.GameObject();
        _startPositionX = transform.position.x;
        _startPositionY = transform.position.y;
        _lengthX = GetComponentInChildren<SpriteRenderer>().bounds.size.x;
        _lengthY = GetComponentInChildren<SpriteRenderer>().bounds.size.y;
    }
    
    void Update()
    {
        float distanceX = _camera.transform.position.x * ParallaxEffect;
        float movementX = _camera.transform.position.x * (1 - ParallaxEffect);
        float distanceY = _camera.transform.position.y * ParallaxEffect;
        float movementY = _camera.transform.position.y * (1 - ParallaxEffect);

        transform.position =
            new Vector3(_startPositionX + distanceX, _startPositionY + distanceY, transform.position.z);

        if (movementX > _startPositionX + _lengthX) _startPositionX += 2 * _lengthX;
        else if (movementX < _startPositionX - _lengthX) _startPositionX -= 2 * _lengthX;
        if (movementY > _startPositionY + _lengthY) _startPositionY += 2 * _lengthY;
        else if (movementY < _startPositionY - _lengthY) _startPositionY -= 2 * _lengthY;
    }
}