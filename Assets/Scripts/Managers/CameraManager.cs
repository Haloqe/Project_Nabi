using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public CinemachineVirtualCamera[] AllVirtualCameras;
    public AudioListener inGameAudioListener;
    public Camera inGameMainCamera;
    public Camera mapCamera;
    public Camera minimapCamera;
    public Camera uiCamera;
    
    private float _fallPanAmount = 0.25f;
    private float _fallYPanTime = 0.35f;
    private float _fallOffsetAmount = -2.5f;
    // private float _fallPantime = 0.35f;
    public float FallSpeedYDampingChangeThreshold = -100f;
    
    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    public CinemachineVirtualCamera CurrentCamera { get; private set; }
    private CinemachineFramingTransposer _framingTransposer;

    private float _normalYPanAmount;
    private float _normalYOffsetAmount;
    private bool _isFramingTransposed = false;
    
    void Start()
    {
        for (int i = 0; i < AllVirtualCameras.Length; i++)
        {
            if (AllVirtualCameras[i].enabled)
            {
                CurrentCamera = AllVirtualCameras[i];
                _framingTransposer = CurrentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        // _normalYPanAmount = _framingTransposer.m_YDamping;
        if (_framingTransposer == null) return;
        
        _normalYOffsetAmount = _framingTransposer.m_TrackedObjectOffset.y;
        _isFramingTransposed = true;
    }

    public void LerpYDamping(bool isPlayerFalling)
    {
        if (!_isFramingTransposed) return;
        // StartCoroutine(LerpYDampingAction(isPlayerFalling));
        StartCoroutine(LerpYOffsetAction(isPlayerFalling));
    }

    private IEnumerator LerpYDampingAction(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;

        float startDampAmount = _framingTransposer.m_YDamping;
        float endDampAmount = 0f;
        
        if (isPlayerFalling)
        {
            endDampAmount = _fallPanAmount;
            LerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = _normalYPanAmount;
        }
        
        float timer = 0f;
        while (timer < _fallYPanTime)
        {
            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (timer / _fallYPanTime));
            _framingTransposer.m_YDamping = lerpedPanAmount;
            
            timer += Time.deltaTime;
            yield return null;
        }

        IsLerpingYDamping = false;
    }

    private IEnumerator LerpYOffsetAction(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;
        
        float startOffsetAmount = _framingTransposer.m_TrackedObjectOffset.y;
        float endOffsetAmount = 0f;

        if (isPlayerFalling)
        {
            endOffsetAmount = _fallOffsetAmount;
            LerpedFromPlayerFalling = true;
        }
        else
        {
            endOffsetAmount = _normalYOffsetAmount;
        }
        
        float timer = 0f;
        while (timer < _fallYPanTime)
        {
            float lerpedPanAmount = Mathf.Lerp(startOffsetAmount, endOffsetAmount, timer / _fallYPanTime);
            _framingTransposer.m_TrackedObjectOffset =
                new Vector3(_framingTransposer.m_TrackedObjectOffset.x, lerpedPanAmount, 0);

            timer += Time.deltaTime;
            yield return null;
        }
        
        IsLerpingYDamping = false;
    }
    
    public void SwapCamera(CinemachineVirtualCamera camera2)
    {
        if (!isActiveAndEnabled) return;
        CinemachineVirtualCamera camera1 = CurrentCamera;
        Debug.Log(camera1 + " switched to " + camera2);
        camera1.enabled = false;
        camera2.enabled = true;
        StartCoroutine(AssignCollider(Array.IndexOf(AllVirtualCameras, camera2)));
        CurrentCamera = camera2;
        _framingTransposer = CurrentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        _isFramingTransposed = _framingTransposer != null;
        GameObject followObject = GameObject.Find("CameraFollowingObject");
        if (followObject != null) CurrentCamera.Follow = followObject.transform;
    }

    private IEnumerator AssignCollider(int cameraIndex)
    {
        if (cameraIndex == 1) yield break;
        yield return null;
        CompositeCollider2D collider = GameObject.Find(cameraIndex.ToString()).GetComponent<CompositeCollider2D>();
        AllVirtualCameras[cameraIndex].GetComponent<CinemachineConfiner2D>().m_BoundingShape2D = collider;
        if (cameraIndex is 5 or 6)
            CurrentCamera.Follow = PlayerController.Instance.gameObject.transform;
    }
}
