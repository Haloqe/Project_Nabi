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
        PlayerEvents.HPChanged += OnPlayerHPChanged;
        PlayerEvents.defeated += OnPlayerDefeated;
        _vCam = GetComponent<CinemachineVirtualCamera>();
        _cbmcp = _vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _cbmcp.m_AmplitudeGain = 0.0f;
        _remainingTime = 0.0f;
    }

    public void OnPlayerHPChanged(float changeAmount, float hpRatio)
    {
        // TEMP
        if (changeAmount < 0.0f)
        {
            _cbmcp.m_AmplitudeGain = _shakeIntensity * Mathf.Clamp(Mathf.Abs(changeAmount) / 3f, 1, 3);
            _remainingTime += (_shakeTime - _remainingTime);
        }        
    }

    public void OnPlayerDefeated()
    {
        // TEMP
        _cbmcp.m_AmplitudeGain = _shakeIntensity * 5;
        _remainingTime += (_shakeTime*2 - _remainingTime);
    }

    public void StopShake()
    {
        _cbmcp.m_AmplitudeGain = 0.0f;
        _remainingTime -= _shakeTime;
    }

    private void LateUpdate()
    {
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
