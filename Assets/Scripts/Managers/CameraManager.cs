using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private CinemachineVirtualCamera[] _allVirtualCameras;
    
    private float _fallPanAmount = 0.25f;
    private float _fallYPanTime = 0.35f;
    private float _fallOffsetAmount = -2.5f;
    // private float _fallPantime = 0.35f;
    public float FallSpeedYDampingChangeThreshold = -100f;
    
    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private CinemachineVirtualCamera _currentCamera;
    private CinemachineFramingTransposer _framingTransposer;

    private float _normalYPanAmount;
    private float _normalYOffsetAmount;

    void Start()
    {
        for (int i = 0; i < _allVirtualCameras.Length; i++)
        {
            if (_allVirtualCameras[i].enabled)
            {
                _currentCamera = _allVirtualCameras[i];
                _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        // _normalYPanAmount = _framingTransposer.m_YDamping;
        _normalYOffsetAmount = _framingTransposer.m_TrackedObjectOffset.y;

    }

    public void LerpYDamping(bool isPlayerFalling)
    {
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
        yield break;
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
}
