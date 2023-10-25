using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TestCameraShake : MonoBehaviour
{
    private CinemachineVirtualCamera _vCam;
    private float _shakeIntensity = 1f;
    private float _shakeTime = 0.2f;

    private float _remainingTime = 0.0f;
    private CinemachineBasicMultiChannelPerlin _cbmcp;

    private void Awake()
    {
        _vCam = GetComponent<CinemachineVirtualCamera>();
        _cbmcp = _vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _cbmcp.m_AmplitudeGain = 0.0f;
        _remainingTime = 0.0f;
    }

    public void StartShake()
    {
        _cbmcp.m_AmplitudeGain = _shakeIntensity;
        _remainingTime += (_shakeTime - _remainingTime);
    }

    public void StopShake()
    {
        _cbmcp.m_AmplitudeGain = 0.0f;
        _remainingTime -= _shakeTime;
    }

    private void LateUpdate()
    {
        if (Input.GetKey(KeyCode.Y))
        {
            StartShake();
        }

        if (_remainingTime > 0)
        {
            _remainingTime -= Time.deltaTime;
            if (_remainingTime <= 0)
            {
                StopShake();
            }
        }
    }
}
