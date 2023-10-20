using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCombat : MonoBehaviour, IDamageDealer, IDamageable
{
    // reference to other components
    private PlayerMovement _playerMovement;

    // heatlh attributes
    private int health = 15;

    // status effect attributes
    [NamedArray(typeof(EStatusEffect))]
    public GameObject[] DebuffEffects;
    private float[] _effectRemainingTimes;
    private SortedDictionary<float, float> _slowRemainingTimes; // str,time

    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _effectRemainingTimes = new float[(int)EStatusEffect.MAX];
        _slowRemainingTimes = new SortedDictionary<float, float>(
            Comparer<float>.Create(delegate (float x, float y) { return y.CompareTo(x); })
        );
        Debug.Assert(DebuffEffects.Length == 6);
    }

    private void Update()
    {
        UpdateStatusEffectTimes();
    }

    private void UpdateStatusEffectTimes()
    {
        bool shouldEnableMovement = false;
        float deltaTime = Time.deltaTime;

        for (int i = 0; i < _effectRemainingTimes.Length; i++)
        {
            // slow time is handled separately
            if (i == (int)EStatusEffect.Slow) continue;

            // skip if nothing to update
            if (_effectRemainingTimes[i] == 0.0f) continue;

            // update remaining time
            _effectRemainingTimes[i] -= deltaTime;
            if (_effectRemainingTimes[i] <= 0)
            {
                SetVFXActive(i, false);
                _effectRemainingTimes[i] = 0.0f;
                if (i == (int)EStatusEffect.Root || i == (int)EStatusEffect.Airborne || i == (int)EStatusEffect.Stun)
                {
                    shouldEnableMovement = true;
                }
            }
        }

        if (shouldEnableMovement) _playerMovement.EnableDisableMovement(true);
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
    public void DealDamage(IDamageable target, SDamageInfo damageInfo)
    {
        target.TakeDamage(AdjustOutgoingDamage(damageInfo));
    }

    public void TakeDamage(SDamageInfo damageInfo)
    {
        // TEMP CODE
        Utility.PrintDamageInfo("player", damageInfo);
        HandleNewDamages(damageInfo.Damages);
        HandleNewStatusEffects(damageInfo.StatusEffects);
        
    }

    private SDamageInfo AdjustOutgoingDamage(SDamageInfo damageInfo)
    {
        // make changes to the damage dealt based on attributes
        return damageInfo;
    }

    // TODO
    private void HandleNewDamages(List<SDamage> damages)
    {
        int damage = (int)IDamageable.CalculateRoughDamage(damages);
        health -= damage;
        Debug.Log("Damaged by " + damage + " | Current health: " + health);
        if (health <= 0) Die();
    }

    private void Die()
    {
        // TEMP CODE
        Debug.Log("Player died.");
        // should save info somewhere, do progressive updates
        Destroy(gameObject);
    }
    #endregion Damage Dealing and Receiving


#region Status effects handling
    //TODO
    private void HandleNewStatusEffects(List<SStatusEffect> statusEffects)
    {
        if (statusEffects.Count == 0) return;
        bool shouldDisableMovement = false;
        bool shouldDisableSkills = false;

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
                    shouldDisableSkills = true;
                    break;

                case EStatusEffect.Root: // disable movement
                    shouldDisableMovement = true;
                    break;

                case EStatusEffect.Airborne: // disable movement + skill, airborne
                    shouldDisableMovement = true;
                    shouldDisableSkills = true;
                    // TODO airborne
                    break;

                case EStatusEffect.Silence: // disable skill
                    shouldDisableSkills = true;
                    break;
            }
        }

        if (shouldDisableMovement)
        {
            _playerMovement.EnableDisableMovement(false);
        }
        if (shouldDisableSkills)
        {
            // TODO
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
        }
        // increment effect time
        _effectRemainingTimes[effectIdx] += duration;
    }

    private void ApplySlow(float strength, float duration)
    {
        // return if invalid slow
        if (strength == 0 || duration == 0) return;

        // check if this is the first slow
        if (_slowRemainingTimes.Count == 0)
        {
            _playerMovement.ChangeSpeedByPercentage(strength);
            _slowRemainingTimes.Add(strength, duration);
            SetVFXActive(EStatusEffect.Slow, true);
            return;
        }

        // increment duration if the same strength slow already exists
        if (_slowRemainingTimes.TryGetValue(strength, out float remainingTime))
        {
            _slowRemainingTimes[strength] += duration;
            return;
        }

        // if same strength does not exist, check if new slow is necessary
        foreach (var slowStat in _slowRemainingTimes)
        {
            // no need to add new slow if there is a more effective slow 
            if (slowStat.Key > strength && slowStat.Value >= duration) return;
            // remove existing slow if it is less effective
            if (slowStat.Key < strength && slowStat.Value < duration) _slowRemainingTimes.Remove(slowStat.Key);
        }

        // else, add new slow
        _playerMovement.ChangeSpeedByPercentage(strength, true);
        _slowRemainingTimes.Add(strength, duration);
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

    public void RemoveAllDebuffs()
    {
        for (int i = 0; i < (int)EStatusEffect.MAX; i++)
        {
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
}
