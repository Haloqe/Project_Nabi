using UnityEngine;
using Cinemachine;

public class TestCameraShake : MonoBehaviour
{
    private CinemachineVirtualCamera _vCam;
    private float _shakeIntensity = 1.3f;
    private float _shakeTime = 0.2f;

    private float _remainingTime = 0.0f;
    private CinemachineBasicMultiChannelPerlin _cbmcp;

    private void Awake()
    {
        PlayerEvents.HpChanged += OnPlayerHPChanged;
        PlayerEvents.Defeated += OnPlayerDefeated;
        InGameEvents.EnemySlayed += OnEnemySlayed;
        _vCam = GetComponent<CinemachineVirtualCamera>();
        _cbmcp = _vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _cbmcp.m_AmplitudeGain = 0.0f;
        _remainingTime = 0.0f;
    }
    
    private void OnEnemySlayed(EnemyBase slayedEnemy)
    {
        _cbmcp.m_AmplitudeGain = Mathf.Max(_cbmcp.m_AmplitudeGain, _shakeIntensity * 4);
        _remainingTime += (0.25f - _remainingTime);
    }

    private void OnPlayerHPChanged(float changeAmount, float oldHpRatio, float newHpRatio)
    {
        // TEMP
        if (changeAmount < 0.0f)
        {
            _cbmcp.m_AmplitudeGain = Mathf.Max(_cbmcp.m_AmplitudeGain, _shakeIntensity * Mathf.Clamp(Mathf.Abs(changeAmount), 1, 3.5f));
            _remainingTime += (0.2f - _remainingTime);
        }        
    }

    public void OnPlayerDefeated(bool isRealDeath)
    {
        if (!isRealDeath) return;
        
        // TEMP
        _cbmcp.m_AmplitudeGain = Mathf.Max(_cbmcp.m_AmplitudeGain,_shakeIntensity * 8);
        _remainingTime += (_shakeTime * 2 - _remainingTime);
    }

    public void OnComboAttack()
    {
        // TEMP
        _cbmcp.m_AmplitudeGain = Mathf.Max(_cbmcp.m_AmplitudeGain, _shakeIntensity * 1);
        _remainingTime += (_shakeTime * 1 - _remainingTime);
    }

    public void StopShake()
    {
        _cbmcp.m_AmplitudeGain = 0.0f;
        _remainingTime = 0;
    }

    public void OnMidBossDeath(float deathSequenceTime)
    {
        _cbmcp.m_AmplitudeGain = Mathf.Max(_cbmcp.m_AmplitudeGain,_shakeIntensity * 8);
        _remainingTime = deathSequenceTime;
    }

    private void LateUpdate()
    {
        if (_remainingTime > 0)
        {
            _remainingTime -= Time.unscaledDeltaTime;
            if (_remainingTime <= 0)
            {
                StopShake();
            }
        }
    }
}
