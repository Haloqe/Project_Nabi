using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerDamageReceiver : MonoBehaviour, IDamageable
{
    // reference to other components
    private PlayerMovement _playerMovement;

    // TEMP health attributes
    private float _health = 300;
    private float _maxHealth = 300;

    // status effect attributes
    public bool IsSilenced { get; private set; }
    public bool IsSilencedExceptCleanse { get; private set; }
    [NamedArray(typeof(EStatusEffect))] public GameObject[] DebuffEffects;
    [NamedArray(typeof(EDamageType))] public GameObject[] DamageEffects;
    private int[] _activeDOTCounts;
    private float[] _effectRemainingTimes;
    private SortedDictionary<float, float> _slowRemainingTimes; // str,time

    public bool TempIsDead = false;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private readonly static int IsDead = Animator.StringToHash("IsDead");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void OnRestarted()
    {
        Array.Clear(_effectRemainingTimes, 0, _effectRemainingTimes.Length);
        Array.Clear(_activeDOTCounts, 0, _activeDOTCounts.Length);
        _slowRemainingTimes.Clear();
        _health = _maxHealth;
        IsSilenced = false;
        IsSilencedExceptCleanse = false;
        TempIsDead = false;
    }
    
    private void Start()
    {
        PlayerEvents.defeated += OnDefeated;
        GameEvents.restarted += OnRestarted;
        
        _playerMovement = GetComponent<PlayerMovement>();
        _effectRemainingTimes = new float[(int)EStatusEffect.MAX];
        _slowRemainingTimes = new SortedDictionary<float, float>(
            Comparer<float>.Create(delegate (float x, float y) { return y.CompareTo(x); })
        );
        _activeDOTCounts = new int[(int)EDamageType.MAX];
    }

    private void Update()
    {
        UpdateStatusEffectTimes();
    }

    //updates the remaining time of various status effects
    private void UpdateStatusEffectTimes()
    {
        float deltaTime = Time.deltaTime;
        bool shouldCheckUpdateMovement = false;
        bool shouldCheckUpdateSilenceEx = false;

        for (int i = 0; i < _effectRemainingTimes.Length; i++)
        {
            // slow time is handled separately
            if (i == (int)EStatusEffect.Slow) continue;

            // skip if nothing to update
            if (_effectRemainingTimes[i] == 0.0f) continue;

            // update remaining time for the specific status effect
            _effectRemainingTimes[i] -= deltaTime;
            EStatusEffect currEffect = (EStatusEffect)i;
            if (_effectRemainingTimes[i] <= 0)
            {
                SetVFXActive(i, false);
                _effectRemainingTimes[i] = 0.0f;

                if (i == (int)EStatusEffect.Silence)
                {
                    IsSilenced = false;
                }
                if (!shouldCheckUpdateMovement && 
                    (currEffect == EStatusEffect.Root || currEffect == EStatusEffect.Airborne || currEffect == EStatusEffect.Stun))
                {
                    shouldCheckUpdateMovement = true;
                }
                if (!shouldCheckUpdateSilenceEx && 
                    (currEffect == EStatusEffect.Airborne || currEffect == EStatusEffect.Stun))
                {
                    shouldCheckUpdateSilenceEx = true;
                }
            }
        }

        // Check if need to change attributes
        if (shouldCheckUpdateMovement)
        {
            if (_effectRemainingTimes[(int)EStatusEffect.Root] == 0.0f &&
                _effectRemainingTimes[(int)EStatusEffect.Airborne] == 0.0f &&
                _effectRemainingTimes[(int)EStatusEffect.Stun] == 0.0f)
            {
                _playerMovement.EnableMovement(true);
            }
        }
        if (shouldCheckUpdateSilenceEx)
        {
            if (_effectRemainingTimes[(int)EStatusEffect.Stun] == 0.0f &&
                _effectRemainingTimes[(int)EStatusEffect.Airborne] == 0.0f)
            {
                IsSilencedExceptCleanse = false;
            }
        }

        UpdateSlowTimes();
    }

    private void UpdateSlowTimes()
    {
        if (_slowRemainingTimes.Count == 0) return;

        bool removed = false;
        foreach (float strength in _slowRemainingTimes.Keys.ToList())
        {
            _slowRemainingTimes[strength] -= Time.deltaTime;
            if (_slowRemainingTimes[strength] <= 0.0f)
            {
                _slowRemainingTimes.Remove(strength);
                removed = true;
            }
        }

        // update slow strength (i.e. move speed)
        if (_slowRemainingTimes.Count == 0)
        {
            _playerMovement.ResetMoveSpeed();
            SetVFXActive(EStatusEffect.Slow, false);
        }
        else if (removed)
        {
            _playerMovement.ChangeSpeedByPercentage(_slowRemainingTimes.First().Key);
        }
    }

    #region Damage Dealing and Receiving
    // IDamageable Override
    
    public void TakeDamage(AttackInfo damageInfo)
    {
        // TODO Remove when monster fixed
        if (TempIsDead) return;
        
        // TEMP CODE
        Utility.PrintDamageInfo("Player", damageInfo);
        HandleNewDamage(damageInfo.Damage, damageInfo.AttackerArmourPenetration);
        HandleNewStatusEffects(damageInfo.StatusEffects);
    }    

    private void HandleNewDamage(DamageInfo damage, float attackerArmourPenetration)
    {
        // 플레이어 방어력 처리
        damage.TotalAmount = Mathf.Max(damage.TotalAmount - (PlayerController.Instance.Armour - attackerArmourPenetration), 0);
        StartCoroutine(DamageCoroutine(damage));
    }

    private void OnDefeated()
    {
        StopAllCoroutines();
        _animator.SetBool(IsDead, true);
        TempIsDead = true;
        _spriteRenderer.color = Color.white;

        // TODO should save info somewhere, do progressive updates
    }

    private IEnumerator DamageCoroutine(DamageInfo damage)
    {
        int damageTypeIdx = (int)damage.Type;
        
        // One-shot damage
        if (damage.Duration == 0)
        {
            ChangeHealthByAmount(-damage.TotalAmount);
            yield return null;
        }
        // Damage Over Time (DOT) damage
        // Deals damage.TotalAmount of damage every damage.Tick seconds for damage.Duration
        else
        {
            // Depending on the damage type, start an effect if this is a DOT attack
            if (damage.Duration != 0.0f && ++_activeDOTCounts[damageTypeIdx] == 1)
            {
                if (DamageEffects[damageTypeIdx] != null)
                {
                    DamageEffects[damageTypeIdx].SetActive(true);
                }
            }

            float damagePerTick = damage.TotalAmount / (damage.Duration / damage.Tick + 1);
            var damageCoroutine = StartCoroutine(DOTDamageCoroutine(damagePerTick, damage.Tick));
            yield return new WaitForSeconds(damage.Duration + damage.Tick / 2.0f);

            // Do cleanup
            // Stop applying damage if the duration is over
            StopCoroutine(damageCoroutine);

            // If this is the last effect, activate the VFX
            if (--_activeDOTCounts[damageTypeIdx] == 0 && DamageEffects[damageTypeIdx] != null)
            {
                DamageEffects[damageTypeIdx].SetActive(false);
            }
        }
    }

    private IEnumerator DOTDamageCoroutine(float damagePerTick, float tick)
    {
        while (true)
        {
            // Wait for a tick time and take damage repeatedly
            ChangeHealthByAmount(-damagePerTick);
            yield return new WaitForSeconds(tick);
        }
    }

    public void ChangeHealthByAmount(float amount, bool byEnemy = true)
    {
        // TODO hit/heal effect
        if (amount < 0) StartCoroutine(DamagedRoutine());
        _health = Mathf.Clamp(_health + amount, 0, _maxHealth);
        PlayerEvents.HPChanged.Invoke(amount, GetHPRatio());
        if (_health == 0)
        {
            PlayerEvents.defeated.Invoke();
            GameManager.Instance.IsFirstRun = false;
        }
    }
    
    // TODO FIX: damage visualisation
    private IEnumerator DamagedRoutine()
    {
        _spriteRenderer.color = new Color(0.6f, 0.05f, 0.05f);
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.color = Color.white;
    }

    public float GetHPRatio()
    {
        return _health / _maxHealth;
    }
    #endregion Damage Dealing and Receiving


    #region Status effects handling
    //TODO
    private void HandleNewStatusEffects(List<StatusEffectInfo> statusEffects)
    {
        if (statusEffects.Count == 0) return;
        bool shouldDisableMovement = false;

        foreach (var statusEffect in statusEffects)
        {
            // handle SLOW separately
            if (statusEffect.Effect == EStatusEffect.Slow)
            {
                // reduce movement speed, TODO: jump force
                ApplySlow(statusEffect.Strength, statusEffect.Duration);
                continue;
            }

            // handle other status effects
            UpdateStatusEffectTime(statusEffect.Effect, statusEffect.Duration);

            switch (statusEffect.Effect)
            {
                case EStatusEffect.Blind: // raise attack miss rate
                    // TODO
                    break;

                case EStatusEffect.Stun: // disable movement + skill
                    shouldDisableMovement = true;
                    IsSilencedExceptCleanse = true;
                    break;

                case EStatusEffect.Root: // disable movement
                    shouldDisableMovement = true;
                    break;

                case EStatusEffect.Airborne: // disable movement + skill, airborne
                    shouldDisableMovement = true;
                    IsSilencedExceptCleanse = true;
                    // TODO airborne
                    break;

                case EStatusEffect.Silence: // disable skill
                    IsSilenced = true;
                    break;
            }
        }

        if (shouldDisableMovement)
        {
            _playerMovement.DisableMovement(true);
        }
    }

    private void UpdateStatusEffectTime(EStatusEffect effect, float duration)
    {
        if (duration == 0) return; // TODO to be handled later (장판형 유지스킬)

        int effectIdx = (int)effect;
        // apply new effect
        if (_effectRemainingTimes[effectIdx] == 0)
        {
            SetVFXActive(effectIdx, true);
            Debug.Log("New " + effect.ToString() + " time: " + duration.ToString("0.0000"));
        }
        // or increment effect time
        else
        {
            duration = Mathf.Clamp(duration - _effectRemainingTimes[effectIdx], 0, float.MaxValue);
            Debug.Log("Previous " + effect.ToString() + " time: " + _effectRemainingTimes[effectIdx] + 
                " Updated time: " + (_effectRemainingTimes[effectIdx] + duration).ToString("0.0000"));
        }
        _effectRemainingTimes[effectIdx] += duration;
    }

    private void ApplySlow(float strength, float duration)
    {
        // return if invalid slow
        if (strength == 0 || duration == 0) return;

        // check if this is the first slow
        if (_slowRemainingTimes.Count == 0)
        {
            SetVFXActive(EStatusEffect.Slow, true);
        }

        // increment duration if the same strength slow already exists
        if (_slowRemainingTimes.TryGetValue(strength, out float remainingTime))
        {
            duration = Mathf.Clamp(duration - remainingTime, 0, float.MaxValue);
            _slowRemainingTimes[strength] += duration;
            Debug.Log("Previous slow (" + strength + ") time: " + remainingTime +
                " Updated time: " + (_slowRemainingTimes[strength]).ToString("0.0000"));
            return;
        }

        // if same strength does not exist, check if new slow is necessary
        foreach (var slowStat in _slowRemainingTimes)
        {
            // no need to add new slow if there is a more effective slow 
            if (slowStat.Key > strength && slowStat.Value >= duration) return;
            // remove existing slow if it is less effective
            if (slowStat.Key < strength && slowStat.Value <= duration) _slowRemainingTimes.Remove(slowStat.Key);
        }

        // else, add new slow
        _playerMovement.ChangeSpeedByPercentage(strength, true);
        _slowRemainingTimes.Add(strength, duration);
        Debug.Log("New slow (" + strength + ") time: " + duration.ToString("0.0000"));
    }

    private void SetVFXActive(EStatusEffect effect, bool setActive)
    {
        SetVFXActive((int)effect, setActive);
    }

    private void SetVFXActive(int effectIdx, bool setActive)
    {
        if (DebuffEffects[effectIdx] == null) return;
        DebuffEffects[effectIdx].SetActive(setActive);
    }

    /// <summary>
    ///  현재는 isSliencedExceptCleanse 포함해서 제거함. Silenced는 유지.
    /// </summary>
    public void RemoveAllDebuffs()
    {
        for (int i = 0; i < (int)EStatusEffect.MAX; i++)
        {
            if (i == (int)EStatusEffect.Silence) continue;
            SetVFXActive(i, false);
            _effectRemainingTimes[i] = 0.0f;
        }
        _slowRemainingTimes.Clear();
        _playerMovement.RemoveDebuffs();
    }

    // slow timer와는 별개로 이동속도 버프가 끝난 후 슬로우를 재설정 하기위함.
    public void SetActiveSlow()
    {
        if (_slowRemainingTimes.Count == 0) return;

        float strength =_slowRemainingTimes.First().Value;
        _playerMovement.ChangeSpeedByPercentage(strength);
    }
    #endregion Status effects handling

    #region StatUpgrade
    
    #endregion
}
