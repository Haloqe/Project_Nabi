using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class MapController : MonoBehaviour
{
    private Transform _mapCamera;
    private Transform _minimapCamera;
    private Camera _mapCameraComponent;
    
    private bool _isLerping;
    private bool _isNavigating;
    private bool _isZooming;
    
    private Vector2 _navigationValue;
    private float _zoomValue;
    private float _defaultSize;

    private void Awake()
    {
        _mapCamera = GameObject.Find("Cameras").transform.Find("Map Camera");
        _minimapCamera = Camera.main.transform.Find("Minimap Camera");
        _mapCameraComponent = _mapCamera.GetComponent<Camera>();
        _defaultSize = _mapCameraComponent.orthographicSize;
    }

    private void OnEnable()
    {
        _isLerping = false;
        _isNavigating = false;
        _isZooming = false;
        _navigationValue = Vector2.zero;
        _mapCameraComponent.orthographicSize = _defaultSize;
    }
    private void Update()
    {
        if (_isLerping) return;
        if (_isNavigating)
            _mapCamera.position += (Vector3)_navigationValue / 2.0f;
        if (_isZooming)
        {
            _mapCameraComponent.orthographicSize =
                Mathf.Clamp(_mapCameraComponent.orthographicSize + _zoomValue / 10.0f, 10, 60);
        }
    }

    public void ResetMapCamera(bool shouldLerp = false)
    {
        if (shouldLerp) StartCoroutine(CameraLerpCoroutine());
        else _mapCamera.position = _minimapCamera.position;
    }

    private IEnumerator CameraLerpCoroutine()
    {
        if (_isLerping) yield break;
        
        _isLerping = true;
        Vector3 start = _mapCamera.position;
        Vector3 target = _minimapCamera.position;
        float time = 0f;

        while (_mapCamera.position != target)
        {
            _mapCamera.position = Vector3.Lerp(start, target, time / Vector3.Distance(start, target) * 100f);
            time += Time.deltaTime;
            yield return null;
        }

        _isLerping = false;
    }
    
    public void OnNavigate(Vector2 value)
    {
        _isNavigating = value != Vector2.zero;
        _navigationValue = value;
    }
    
    public void OnZoom(float value)
    {
        _isZooming = value != 0;
        _zoomValue = value;
    } 
}