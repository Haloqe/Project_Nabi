using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable, IDamageDealer
{
    // reference to other components
    private EnemyMovement _enemyMovement;
    protected Rigidbody2D _rigidbody2D;
    protected Animator _animator;
    protected SEnemyData _data;

    //Status effect attributes
    public bool IsSilenced { get; private set; }
    public bool ShouldDisableMovement { get; private set; }
    [NamedArray(typeof(EStatusEffect))] public GameObject[] DebuffEffects;
    private float[] _effectRemainingTimes;
    private SortedDictionary<float, float> _slowRemainingTimes; // str,time

    //Enemy health attribute
    [SerializeField] int enemyHealth = 30;

    protected virtual void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _effectRemainingTimes = new float[(int)EStatusEffect.MAX];
        _slowRemainingTimes = new SortedDictionary<float, float> (
            Comparer<float>.Create(delegate (float x, float y) { return y.CompareTo(x); })
        );
        Initialise();
    }

    private void Update()
    {
        UpdateRemainingStatusEffectTimes();
    }

    protected abstract void Initialise();

    #region Status Effects imposed by the player handling
    private void HandleNewStatusEffects(List<SStatusEffect> statusEffects)
    {
        //SStatusEffect has three instance variables: effect, strength, duration
       
        if (statusEffects.Count == 0) return;

        foreach (var statusEffect in statusEffects)
        {
            // handle SLOW separately
            if (statusEffect.Effect == EStatusEffect.Slow)
            {
                ApplySlow(statusEffect.Strength, statusEffect.Duration);
                continue;
            }

            //handles status effect other than slow
            else
            {
                UpdateStatusEffectTime(statusEffect.Effect, statusEffect.Duration);
            }
            
            switch (statusEffect.Effect)
            {
                case EStatusEffect.Blind: // raise attack miss rate
                    // TODO
                    break;

                case EStatusEffect.Stun: // disable movement + skill
                    ShouldDisableMovement = true;
                    IsSilenced = true;
                    break;

                case EStatusEffect.Root: // disable movement
                    ShouldDisableMovement = true;
                    break;

                case EStatusEffect.Airborne: // disable movement + skill, airborne
                    ShouldDisableMovement = true;
                    IsSilenced = true;
                    // TODO airborne
                    break;

                case EStatusEffect.Silence: // disable skill
                    IsSilenced = true;
                    break;
            }
        }

        if (ShouldDisableMovement)
        {
            _enemyMovement.EnableDisableMovement(false);
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

    private void UpdateRemainingStatusEffectTimes()
    {
        //아직 다시 움직이게 하는거 , 사일런스 풀리는 것 밖에 구현 안되어있음
        float deltaTime = Time.deltaTime;

        for (int i = 0; i < _effectRemainingTimes.Length; i++)
        {
            if (_effectRemainingTimes[i] == 0.0f) continue;

            //slow is handled seperately in another method to be called at the end of this function
            else if (i == (int)EStatusEffect.Slow) continue;

            else
            {
                _effectRemainingTimes[i] -= deltaTime;
                EStatusEffect currEffect = (EStatusEffect)i;

                //if there is no remaining time for the status effect imposed on the enemy after calculation
                if (_effectRemainingTimes[i] <= 0)
                {
                    SetVFXActive(i, false);

                    _effectRemainingTimes[i] = 0.0f;

                    if (currEffect == EStatusEffect.Root || currEffect == EStatusEffect.Airborne || currEffect == EStatusEffect.Stun)
                    {
                        _enemyMovement.EnableDisableMovement(true);
                    }

                    if (currEffect == EStatusEffect.Airborne || currEffect == EStatusEffect.Stun || currEffect == EStatusEffect.Silence)
                    {
                        IsSilenced = false;
                    }
                }
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
            _enemyMovement.ResetMoveSpeed();
            SetVFXActive(EStatusEffect.Slow, false);
        }
        else if (removed)
        {
            _enemyMovement.ChangeSpeedByPercentage(_slowRemainingTimes.First().Key);
        }
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
        _enemyMovement.ChangeSpeedByPercentage(strength);
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

    #endregion Status Effects imposed by the player handling

    #region Status Effects imposing on the player handling
    // TO-DO
    #endregion Status Effects imposing on the player handling

    #region Damage Dealing and Receiving
    // Dealing damage to the player Handling
    public virtual void DealDamage(IDamageable target, SDamageInfo damageInfo)
    {
        damageInfo.DamageSource = gameObject.GetInstanceID();
        target.TakeDamage(damageInfo);
    }

    // Received Damage Handling
    public void TakeDamage(SDamageInfo damageInfo)
    {
        Utility.PrintDamageInfo(gameObject.name, damageInfo);
        HandleNewDamages(damageInfo.Damages);
        HandleNewStatusEffects(damageInfo.StatusEffects);
    }
    private void HandleNewDamages(List<SDamage> damages)
    {
        int damage = (int)IDamageable.CalculateRoughDamage(damages);
        enemyHealth -= damage;
        Debug.Log("Player: Current health: " + enemyHealth + "(-" + damage + ")");
        if (enemyHealth <= 0) { Die(); }
    }

    private void Die()
    {
        Destroy(gameObject);
        // TEMP code
        Debug.Log(gameObject.name + " died.");

    }
    #endregion Damage Dealing and Receiving

}
