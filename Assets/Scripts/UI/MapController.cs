using System.Collections;
using UnityEngine;

public class MapController : UIControllerBase
{
    // Map cameras
    private Transform _mapCamera;
    private Transform _minimapCamera;
    private Camera _mapCameraComponent;
    
    // Navigate
    private bool _isLerping;
    private bool _isNavigating;
    private Vector2 _navigationValue;
    
    // Zoom
    private bool _isZooming;
    private float _zoomValue;
    private float _camSize;
    private float _defaultCamSize;
    private float _minCamSize = 10;
    private float _maxCamSize = 60;

    private void Awake()
    {
        _mapCamera = GameObject.Find("InGameCameras").transform.Find("Map Camera");
        _minimapCamera = Camera.main.transform.Find("Minimap Camera");
        _mapCameraComponent = _mapCamera.GetComponent<Camera>();
        _defaultCamSize = _mapCameraComponent.orthographicSize;
    }

    private void OnEnable()
    {
        _isLerping = false;
        _isNavigating = false;
        _isZooming = false;
        _navigationValue = Vector2.zero;
        _camSize = _mapCameraComponent.orthographicSize = _defaultCamSize;
    }
    
    private void Update()
    {
        if (_isLerping) return;
        if (_isZooming)
        {
            _mapCameraComponent.orthographicSize = 
                _camSize = Mathf.Clamp(_mapCameraComponent.orthographicSize + _zoomValue / 10.0f, _minCamSize, _maxCamSize);
        }
        if (_isNavigating)
            _mapCamera.position += (Vector3)_navigationValue / (2.0f * _defaultCamSize / _camSize);
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
            _mapCamera.position = Vector3.Lerp(start, target, time / Vector3.Distance(start, target) * 120f);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        _isLerping = false;
    }
    
    public override void OnNavigate(Vector2 value)
    {
        _isNavigating = value != Vector2.zero;
        _navigationValue = value;
    }
    
    public override void OnSubmit()
    {
        return;
    }
    public override void OnClose()
    {
        return;
    }
    
    public override void OnTab()
    {
        return;
    }

    public void OnZoom(float value)
    {
        _isZooming = value != 0;
        _zoomValue = value;
    } 
}