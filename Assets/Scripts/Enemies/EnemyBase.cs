using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
 
public class EnemyBase : MonoBehaviour, IDamageable, IDamageDealer
{
    // reference to other components
    protected Rigidbody2D _rigidbody2D;
    protected Animator _animator;
    protected SEnemyData _data;

    // Movement
    protected EnemyMovement _movement = null;

    //Status effect attributes
    public bool IsSilenced { get; private set; }
    public bool ShouldDisableMovement { get; private set; }
    [NamedArray(typeof(EStatusEffect))] public GameObject[] DebuffEffects;
    private float[] _effectRemainingTimes;
    private SortedDictionary<float, float> _slowRemainingTimes; // str,time

    // sommer status effect
    private float _sommerStackCount = 0;
    private float _sommerTimeSinceStacked = 0f;
    private int _sommerSubtractedStackPerSecond = 1;

    // 이런것들 나중에 엑셀으로 뺴면 좋을듯
    // [SerializeField] private float SommerStackCoolTime = 10f;
    [SerializeField] private float SommerReducedSpeed = 0.3f;
    [SerializeField] private float SommerReducedDamage = 0.3f;
    [SerializeField] private float SleepDuration = 5f;
    [SerializeField] private float SleepBaseStrength = 1f; // idk what this does
    
    //Enemy health attribute
    public AttackInfo _damageInfoTEMP;
    [SerializeField] protected float Health = 1000f;

    protected virtual void Start()
    { 
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _effectRemainingTimes = new float[(int)EStatusEffect.MAX];
        _slowRemainingTimes = new SortedDictionary<float, float> (
            Comparer<float>.Create(delegate (float x, float y) { return y.CompareTo(x); })
        );
        DebuffEffects = new GameObject[(int)EStatusEffect.MAX];
        _movement = GetComponent<EnemyMovement>();

        _damageInfoTEMP = new AttackInfo();
        _damageInfoTEMP.Damages.Add(new DamageInfo(EDamageType.Base, 5));
        PlayerEvents.defeated += OnPlayerDefeated;

        Initialise();         
    }
    private void OnPlayerDefeated()
    {
        
    }

    protected virtual void FixedUpdate()
    {
        UpdateRemainingStatusEffectTimes();
    }

    protected virtual void Initialise() { }

    #region Status Effects imposed by the player handling
    private void HandleNewStatusEffects(List<StatusEffectInfo> statusEffects, int incomingDirectionX = 0)
    {
        if (statusEffects == null) return;

        //StatusEffectInfo has four instance variables: effect, strength, duration, chance
        foreach (var statusEffect in statusEffects)
        {
            // Handle chance
            var randVal = UnityEngine.Random.value;
            Debug.Log("[" + gameObject.name + "] " + statusEffect.Effect + " is " + (randVal > statusEffect.Chance ? "not " : "") + "applied " +
                "(Chance " + statusEffect.Chance + ", val: " + randVal.ToString("F2") + ")");
            if (randVal > statusEffect.Chance)
            {
                continue;
            }
                
            // handle SLOW separately
            if (statusEffect.Effect == EStatusEffect.Slow)
            {
                ApplySlow(statusEffect.Strength, statusEffect.Duration);
                continue;
            }
            //handles status effect other than slow

            switch (statusEffect.Effect)
            {
                case EStatusEffect.Sommer:
                    _movement.ChangeSpeedByPercentage(SommerReducedSpeed);
                    // assumed that there's only 1 element in the list. might need edits
                    _damageInfoTEMP.Damages[0] = new DamageInfo(EDamageType.Base, _damageInfoTEMP.Damages[0].TotalAmount * SommerReducedDamage);
                    _sommerStackCount++;
                    _sommerTimeSinceStacked = 0f;
                    break;

                case EStatusEffect.Sleep:
                    ShouldDisableMovement = true;
                    //TODO
                    break;

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
                
                case EStatusEffect.Pull:
                    if (_movement.moveType == EEnemyMoveType.Stationary) continue;
                    ShouldDisableMovement = true;
                    IsSilenced = true;
                    _movement.StartPullX(incomingDirectionX, statusEffect.Strength, statusEffect.Duration);
                    break;
            }
            UpdateStatusEffectTime(statusEffect.Effect, statusEffect.Duration);
        }

        if (ShouldDisableMovement)
        {
            _movement.DisableMovement();
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
            _effectRemainingTimes[effectIdx] = duration;
        }
        // or increment effect time
        else
        {
            duration = Mathf.Clamp(duration - _effectRemainingTimes[effectIdx], 0, float.MaxValue);
            _effectRemainingTimes[effectIdx] += duration;
        }
        Debug.Log("[" + gameObject.name + "] Apply " + effect + " for " + duration.ToString("F") 
            + " total: " + _effectRemainingTimes[effectIdx].ToString("F"));
    }

    private void UpdateRemainingStatusEffectTimes()
    {
        //아직 다시 움직이게 하는거 , 사일런스 풀리는 것 밖에 구현 안되어있음
        float deltaTime = Time.deltaTime;
        
        for (int i = 0; i < _effectRemainingTimes.Length; i++)
        {
            // slow time is handled separately
            if (i == (int)EStatusEffect.Slow) continue;
            if (i == (int)EStatusEffect.Sommer) continue;

            if (_effectRemainingTimes[i] == 0) continue;

            _effectRemainingTimes[i] -= deltaTime;
            EStatusEffect currEffect = (EStatusEffect)i;
            //Debug.Log(currEffect + "'s RemainingTime: " + _effectRemainingTimes[i]);

            //if there is no remaining time for the status effect imposed on the enemy after calculation
            if (_effectRemainingTimes[i] <= 0)
            {
                SetVFXActive(i, false);
                
                _effectRemainingTimes[i] = 0.0f;

                if (currEffect == EStatusEffect.Sleep || currEffect == EStatusEffect.Root || currEffect == EStatusEffect.Airborne 
                    || currEffect == EStatusEffect.Stun || currEffect == EStatusEffect.Pull)
                {
                    _movement.EnableMovement();
                }

                if (currEffect == EStatusEffect.Airborne || currEffect == EStatusEffect.Stun || currEffect == EStatusEffect.Silence 
                    || currEffect == EStatusEffect.Pull)
                {
                    IsSilenced = false;
                }
            }
        }

        UpdateSlowTimes();
        UpdateSommerTimes();
    }

    private void UpdateSommerTimes()
    {
        if (_sommerStackCount <= 0) return;

        _sommerTimeSinceStacked += Time.deltaTime;

        if (_sommerTimeSinceStacked > _effectRemainingTimes[(int)EStatusEffect.Sommer])
            _sommerStackCount -= _sommerSubtractedStackPerSecond * Time.deltaTime;

        if (_sommerStackCount >= 10)
        {
            _sommerTimeSinceStacked = 0;
            _sommerStackCount = 0;
            HandleNewStatusEffects(new List<StatusEffectInfo> { new StatusEffectInfo(EStatusEffect.Sleep, SleepBaseStrength, SleepDuration) }, 0);
        }
        else if (_sommerStackCount <= 0)
        {
            _movement.ChangeSpeedByPercentage(1f);
            _damageInfoTEMP.Damages[0] = new DamageInfo(EDamageType.Base, _damageInfoTEMP.Damages[0].TotalAmount);
            _effectRemainingTimes[(int)EStatusEffect.Sommer] = 0;
        }
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
            _movement.ResetMoveSpeed();
            SetVFXActive(EStatusEffect.Slow, false);
        }
        else if (removed)
        {
            _movement.ChangeSpeedByPercentage(_slowRemainingTimes.First().Key);
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
        _movement.ChangeSpeedByPercentage(strength);
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
    public virtual void DealDamage(IDamageable target, AttackInfo damageInfo)
    {
        //damageInfo.DamageSource = gameObject.GetInstanceID();
        target.TakeDamage(damageInfo);
    }

    // Received Damage Handling
    public void TakeDamage(AttackInfo damageInfo)
    {
        Utility.PrintDamageInfo(gameObject.name, damageInfo);
        HandleNewDamages(damageInfo.Damages);
        HandleNewStatusEffects(damageInfo.StatusEffects, damageInfo.IncomingDirectionX);
    }

    private void HandleNewDamages(List<DamageInfo> damages)
    {
        int damage = (int)IDamageable.CalculateRoughDamage(damages);
        ReduceHealth(damage);

        // if (asleep) then wake up
        if (_effectRemainingTimes[(int)EStatusEffect.Sleep] > 0)
        {
            _movement.EnableMovement();
            _effectRemainingTimes[(int)EStatusEffect.Sleep] = 0;
        }
    }

    private void ReduceHealth(float reduceAmount)
    {
        Health -= reduceAmount;
        Debug.Log(gameObject.name + "'s Current health: " + Health + "(-" + reduceAmount + ")");
        if (Health <= 0) { Die(); }
        StartCoroutine(DamagedRoutine());
    }
    
    // TEMP TODO FIX: For damage visualisation
    private IEnumerator DamagedRoutine()
    {
        GetComponent<SpriteRenderer>().color = new Color(0.6f, 0.05f, 0.05f);
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void Die()
    {
        StopAllCoroutines();
        Destroy(gameObject);
        // TEMP code
        Debug.Log(gameObject.name + " died.");
    }
    #endregion Damage Dealing and Receiving
}
