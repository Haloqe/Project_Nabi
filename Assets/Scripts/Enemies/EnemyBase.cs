using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBase : MonoBehaviour, IDamageable, IDamageDealer
{
    [SerializeField] private int typeID;
    
    // reference to other components
    protected Rigidbody2D _rigidbody2D;
    protected Animator _animator;
    protected EnemyManager _enemyManager;
    public SEnemyData EnemyData;
    protected PlayerController _player;
    
    // Movement
    protected EnemyMovement _movement;
    public GameObject Target;
    protected IDamageable _targetDamageable;
    public float ActionTimeCounter = 0f;
    private bool _playerIsDefeated = false;
 
    //Status effect attributes
    public bool IsSilenced { get; private set; }
    public bool ShouldDisableMovement { get; private set; }
    [NamedArray(typeof(EStatusEffect))] public GameObject[] DebuffEffects;
        private int[] _activeDOTCounts;
    private float[] _effectRemainingTimes;
    private SortedDictionary<float, float> _slowRemainingTimes; // str,time

    // sommer status effect
    private float _sommerStackCount = 0;
    private float _sommerTimeSinceStacked = 0f;
    private int _sommerSubtractedStackPerSecond = 1;

    // sommer 입면환각
    [SerializeField] Vector2 _neighboringEnemyColliderSize;
    [SerializeField] float _neighboringEnemyColliderHeight;

    //Enemy health attribute
    public AttackInfo _damageInfo;
    private float Health;
    private SpriteRenderer _spriteRenderer;
    private float _armour;
    private float _damageCooltimeCounter;
    private float _damageCooltime = 0.3f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>(); 
    }

    protected virtual void Start()
    {
        _player = PlayerController.Instance;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _enemyManager = EnemyManager.Instance;
        EnemyData = _enemyManager.GetEnemyData(typeID); 
        
        _effectRemainingTimes = new float[(int)EStatusEffect.MAX];
        _slowRemainingTimes = new SortedDictionary<float, float> (
            Comparer<float>.Create((x, y) => y.CompareTo(x))
        );
        // DebuffEffects = new GameObject[(int)EStatusEffect.MAX];
        _movement = GetComponent<EnemyMovement>();

        _damageInfo = new AttackInfo();
        _damageInfo.Damage = new DamageInfo((EDamageType)Enum.Parse(typeof(EDamageType), EnemyData.DamageType), EnemyData.DefaultDamage);
        PlayerEvents.defeated += OnPlayerDefeated;

        Target = GameObject.FindWithTag("Player");
        _targetDamageable = Target.gameObject.GetComponent<IDamageable>();
        _movement.Init();

        Health = EnemyData.MaxHealth;
        _armour = EnemyData.DefaultArmour;
    }
    
    private void OnPlayerDefeated()
    {
        StopAllCoroutines();
        _movement.StopMovementCoroutines();
        _playerIsDefeated = true;
    }

    private void OnDestroy()
    {
        PlayerEvents.defeated -= OnPlayerDefeated;
    }

    protected virtual void FixedUpdate()
    {
        UpdateRemainingStatusEffectTimes();
        UpdateMovementState();
        _damageCooltimeCounter += Time.deltaTime;
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
            var randVal = Random.value;
            Debug.Log("[" + gameObject.name + "] " + statusEffect.Effect + " is " + (randVal > statusEffect.Chance ? "not " : "") + "applied " +
                "(Chance " + statusEffect.Chance + ", val: " + randVal.ToString("F2") + ")");
            if (randVal > statusEffect.Chance) continue;
                
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
                    if (_sommerStackCount <= 0)
                    {
                        _movement.ChangeSpeedByPercentage(statusEffect.Strength);
                        ChangeAttackSpeedByPercentage(statusEffect.Strength);
                        // Debug.Log("This is the first stack of sommer. Change in move speed and attack speed is applied. now its move speed is " + _movement._moveSpeed + " and attack speed " + _animator.GetFloat("AttackSpeed"));
                    }
                    _sommerStackCount++;
                    _sommerTimeSinceStacked = 0f;
                    // Debug.Log("Sommer was stacked. There are now " + _sommerStackCount + " stacks.");
                    break;

                case EStatusEffect.Ecstasy:
                    _player.ApplyEcstasyBuff(this);
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
                    if (_movement.MoveType == EEnemyMoveType.Stationary) continue;
                    ShouldDisableMovement = true;
                    IsSilenced = true;
                    _movement.StartPullX(incomingDirectionX, statusEffect.Strength, statusEffect.Duration);
                    break;
                
                case EStatusEffect.Evade or EStatusEffect.Camouflage:
                    if (!_player.AddShadowHost(this)) continue;
                    break;
            }
            UpdateStatusEffectTime(statusEffect.Effect, statusEffect.Duration);
        }

        if (ShouldDisableMovement)
        {
            _movement.DisableMovement();
        }
        ShouldDisableMovement = false;
    }

    private void UpdateStatusEffectTime(EStatusEffect effect, float duration)
    {
        if (duration == 0) return; // TODO to be handled later (장판형 유지스킬)=

        int effectIdx = (int)effect;

        // apply new effect
        if (_effectRemainingTimes[effectIdx] <= 0)
        {
            SetVFXActive(effectIdx, true);
            _effectRemainingTimes[effectIdx] = duration;
            // Debug.Log("it is setting " + effectIdx + " ONNN!!!");
        }
        // or increment effect time
        else
        {
            duration = Mathf.Clamp(duration - _effectRemainingTimes[effectIdx], 0, float.MaxValue);
            _effectRemainingTimes[effectIdx] += duration;
        }
        // Debug.Log("[" + gameObject.name + "] Apply " + effect + " for " + duration.ToString("F") 
        //     + " total: " + _effectRemainingTimes[effectIdx].ToString("F"));
    }

    private void UpdateRemainingStatusEffectTimes()
    {
        float deltaTime = Time.deltaTime;
        
        for (int i = 0; i < _effectRemainingTimes.Length; i++)
        {
            // slow time is handled separately
            if (i == (int)EStatusEffect.Slow) continue;
            if (i == (int)EStatusEffect.Sommer) continue;

            if (_effectRemainingTimes[i] == 0) continue;

            _effectRemainingTimes[i] -= deltaTime;
            EStatusEffect currEffect = (EStatusEffect)i;

            //if there is no remaining time for the status effect imposed on the enemy after calculation
            if (_effectRemainingTimes[i] <= 0)
            {
                SetVFXActive(i, false);
                
                _effectRemainingTimes[i] = 0.0f;

                if (currEffect is EStatusEffect.Sleep or EStatusEffect.Root or EStatusEffect.Airborne or EStatusEffect.Stun or EStatusEffect.Pull)
                {
                    _movement.EnableMovement();
                }

                if (currEffect is EStatusEffect.Airborne or EStatusEffect.Stun or EStatusEffect.Silence or EStatusEffect.Pull)
                {
                    IsSilenced = false;
                }

                if (_player.SommerHypHallucinationPreserv != ELegacyPreservation.MAX && currEffect == EStatusEffect.Ecstasy)
                {
                    var hypHallucinationPreserv = _player.playerDamageDealer.BindingSkillPreservations[(int)EWarrior.Sommer];
                    float hypHallucinationDamageMultiplier = Define.SommerHypHallucinationStats[(int)hypHallucinationPreserv];
                    AttackInfo hypHallucinationDamage = new AttackInfo();
                    hypHallucinationDamage.Damage = new DamageInfo(EDamageType.Base, _player.Strength * hypHallucinationDamageMultiplier);
                    TakeDamage(hypHallucinationDamage);
                }

                switch (currEffect)
                {
                    case EStatusEffect.Ecstasy:
                        _player.RemoveEcstasyBuff(this);
                        break;
                    
                    case EStatusEffect.Evade or EStatusEffect.Camouflage:
                        _player.RemoveShadowHost(this);
                        break;
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
            _movement.ResetMoveSpeed();
            ResetAttackSpeed();
            _sommerTimeSinceStacked = 0;
            _sommerStackCount = 0;
            // Debug.Log("final stack. now its move speed is " + _movement._moveSpeed + " and attack speed " + _animator.GetFloat("AttackSpeed"));
            // HandleNewStatusEffects(new List<StatusEffectInfo> { new StatusEffectInfo(EStatusEffect.Sleep, 1, Define.SleepDuration) }, 0);
            AttackInfo sleepDamage = new AttackInfo();
            sleepDamage.StatusEffects = new List<StatusEffectInfo> { new StatusEffectInfo(EStatusEffect.Sleep, 1, Define.SleepDuration) };
            TakeDamage(sleepDamage);
        }
        else if (_sommerStackCount <= 0)
        {
            SetVFXActive((int)EStatusEffect.Sommer, false);
            _movement.ResetMoveSpeed();
            ResetAttackSpeed();
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

    private void SetVFXActive(EStatusEffect effect, bool setActive) => SetVFXActive((int)effect, setActive);

    private void SetVFXActive(int effectIdx, bool setActive)
    {
        if (effectIdx >= DebuffEffects.Length || DebuffEffects[effectIdx] == null) return;
        DebuffEffects[effectIdx].SetActive(setActive);
    }

    #endregion Status Effects imposed by the player handling

    #region Status Effects imposing on the player handling
    // TODO
    #endregion Status Effects imposing on the player handling

    #region Damage Dealing and Receiving

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_damageCooltimeCounter <= _damageCooltime) return;
        if (other.CompareTag(Target.tag))
        {
            // 황홀경 status effect
            if (_animator.GetBool("IsAttacking") == true && _effectRemainingTimes[(int)EStatusEffect.Ecstasy] > 0)
            {
                if (Random.Range(0.0f, 1.0f) <= 0.5f)
                {
                    Debug.Log(gameObject.name + " missed!");
                    Collider2D[] foesToDamage = Physics2D.OverlapBoxAll(transform.position + new Vector3 (0, _neighboringEnemyColliderHeight, 0), _neighboringEnemyColliderSize, 0, LayerMask.GetMask("Enemy"));
                    AttackInfo damageToFoes = _damageInfo.Clone();
                    damageToFoes.Damage.TotalAmount *= 0.25f;
                    foreach (Collider2D col in foesToDamage)
                    {
                        DealDamage(col.gameObject.GetComponent<IDamageable>(), damageToFoes);
                    }
                    return;
                }
            }
            DealDamage(_targetDamageable, _damageInfo);
            _damageCooltimeCounter = 0f;
        }
    }

    // Dealing damage to the player Handling
    public virtual void DealDamage(IDamageable target, AttackInfo damageInfo)
    {
        damageInfo.AttackerArmourPenetration = EnemyData.DefaultArmourPenetration;
        target.TakeDamage(damageInfo);
    }

    // Received Damage Handling
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    
    public void TakeDamage(AttackInfo damageInfo)
    {
        Utility.PrintDamageInfo(gameObject.name, damageInfo);
        HandleNewStatusEffects(damageInfo.StatusEffects, damageInfo.IncomingDirectionX);
        
        // 입면 환각
        var hypHallucinationPreserv = _player.playerDamageDealer.BindingSkillPreservations[(int)EWarrior.Sommer];
        if (_sommerStackCount <= 0)
        {
            AttackInfo boostedDamageInfo = damageInfo.Clone();
            boostedDamageInfo.Damage.TotalAmount += damageInfo.Damage.TotalAmount * Define.SommerHypHallucinationStats[(int)hypHallucinationPreserv];
            HandleNewDamage(boostedDamageInfo.Damage, boostedDamageInfo.AttackerArmourPenetration);
        }
        // 그 외 경우
        else
        {
            HandleNewDamage(damageInfo.Damage, damageInfo.AttackerArmourPenetration);
        }
    }

    private void HandleNewDamage(DamageInfo damage, float attackerArmourPenetration)
    {
        // 방어력 및 방어관통력 처리
        damage.TotalAmount -= Mathf.Max(0, damage.TotalAmount - Mathf.Max(GetArmour() - attackerArmourPenetration, 0)); 
        
        StartCoroutine(DamageCoroutine(damage));

        // if (asleep) then wake up
        if (_effectRemainingTimes[(int)EStatusEffect.Sleep] > 0)
        {
            _movement.EnableMovement();
            _effectRemainingTimes[(int)EStatusEffect.Sleep] = -1;
            // Debug.Log("sleep ended.");
        }
    }

    public float GetArmour()
    {
        if (_effectRemainingTimes[(int)EStatusEffect.Sleep] > 0) return _armour;
        int sommerPreserv = (int)PlayerController.Instance.playerDamageDealer.BindingSkillPreservations[(int)EWarrior.Sommer];
        return Mathf.Max(0, EnemyData.DefaultArmour - EnemyData.DefaultArmour * Define.SommerSleepArmourReduceAmounts[sommerPreserv]);
    }

    public void ChangeArmourByPercentage(float percentage)
    {
        _armour *= percentage;
    }

    public void ResetArmour()
    {
        _armour = EnemyData.DefaultArmour;
    }

    public void ChangeHealthByAmount(float amount)
    {
        Debug.Log("[" + gameObject.name + "] got damaged " + amount);
        if (amount < 0) StartCoroutine(DamagedRoutine());
        Health = Mathf.Clamp(Health + amount, 0, EnemyData.MaxHealth);
        if (Health == 0) Die();
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
            float damagePerTick = damage.TotalAmount / (damage.Duration / damage.Tick + 1);
            var damageCoroutine = StartCoroutine(DOTDamageCoroutine(damagePerTick, damage.Tick));
            yield return new WaitForSeconds(damage.Duration + damage.Tick / 2.0f);

            // Do cleanup
            // Stop applying damage if the duration is over
            StopCoroutine(damageCoroutine);
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
    
    // TODO FIX: damage visualisation
    private IEnumerator DamagedRoutine()
    {
        _spriteRenderer.color = new Color(0.6f, 0.05f, 0.05f);
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.color = Color.white;
    }

    private void Die()
    {
        // Inform player
        if (_effectRemainingTimes[(int)EStatusEffect.Ecstasy] > 0) 
            _player.RemoveEcstasyBuff(this);
        InGameEvents.EnemySlayed?.Invoke(this);
            
        StopAllCoroutines();
        DropGold();
        Destroy(gameObject);
    }
    #endregion Damage Dealing and Receiving

    #region Enemy Movement
    private void UpdateMovementState()
    {
        if (_playerIsDefeated)
        {
            _movement.Patrol();
            return;
        }
        
        if (_movement.IsRooted) return;
        ActionTimeCounter -= Time.deltaTime;
        
        if (_movement.PlayerIsInAttackRange() || _movement.IsAttackingPlayer)
        {
            _movement.Attack();
        }
        else if (_movement.PlayerIsInDetectRange())
        {
            _movement.IsChasingPlayer = true;
            ActionTimeCounter = EnemyData.ChasePlayerDuration;
            _movement.Chase();
        }
        else if (_movement.IsChasingPlayer)
        {
            _movement.Chase();
        }
        else
        {
            _movement.Patrol();
        }
    }

    public void ChangeAttackSpeedByPercentage(float percentage)
    {
        float initial = _animator.GetFloat("AttackSpeed");
        _animator.SetFloat("AttackSpeed", initial * percentage);
    }

    public void ResetAttackSpeed()
    {
        _animator.SetFloat("AttackSpeed", 1f);
    }
    #endregion Enemy Movement


    private void DropGold()
    {
        int minGoldRange = EnemyData.MinGoldRange;
        int maxGoldRange = EnemyData.MaxGoldRange + (int)Define.EuphoriaEnemyGoldDropBuffStats[(int)_player.EuphoriaEnemyGoldDropBuffPreserv];
        
        // Calculate how much gold to drop
        int goldToDrop = 0;
        if (_player.EuphoriaEnemyGoldDropBuffPreserv == ELegacyPreservation.MAX)
        {
            goldToDrop = Random.Range(minGoldRange, maxGoldRange);
        }
        else
        {
            float random = Mathf.Pow(Random.value, 2);
            goldToDrop = minGoldRange + Mathf.RoundToInt(random * (maxGoldRange - minGoldRange));
        }
        
        // Drop coins
        for (int i = 0; i < goldToDrop / 10 + 1; i++)
        {
            // Instantiate a coin at the enemy's position
            var coin = Instantiate(_enemyManager.GoldPrefab, transform.position, Quaternion.identity).GetComponent<Gold>();
            coin.value = (i < goldToDrop / 10) ? 10 : goldToDrop % 10;
        }
    }

    private void OnBecameVisible()
    {
        _enemyManager.VisibilityChecker.AddEnemyToVisibleList(gameObject);
    }

    private void OnBecameInvisible()
    {
        _enemyManager.VisibilityChecker.RemoveEnemyFromVisibleList(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up + new Vector3 (0, _neighboringEnemyColliderHeight, 0), _neighboringEnemyColliderSize);
    }
}
