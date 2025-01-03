using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemyBase : MonoBehaviour, IDamageable, IDamageDealer
{
    public int typeID;

    // reference to other components
    protected Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private EnemyManager _enemyManager;
    public SEnemyData EnemyData;
    private PlayerController _player;
    private SpriteRenderer[] _spriteRenderers;
    private UIManager _uiManager;
    private Material _originalMaterial;
    
    // Pattern
    protected EnemyPattern Pattern;
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
    public AttackInfo DamageInfo;
    public float Health { get; private set; }
    public float maxHealth;
    private float _armour;
    private float _damageCooltimeCounter;
    private float _damageCooltime = 0.3f;
    private bool _isDead;
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int AttackSpeed = Animator.StringToHash("AttackSpeed");
    
    // sfx
    private AudioSource _audioSource;
    [SerializeField] private AudioClip _hitAudio; 
    
    private void Awake()
    {
        _uiManager = UIManager.Instance;
        _player = PlayerController.Instance;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _enemyManager = EnemyManager.Instance;
        EnemyData = _enemyManager.GetEnemyData(typeID);
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        _originalMaterial = _spriteRenderers[0].material;
        _audioSource = GetComponent<AudioSource>();
        
        _effectRemainingTimes = new float[(int)EStatusEffect.MAX];
        _slowRemainingTimes = new SortedDictionary<float, float> (
            Comparer<float>.Create((x, y) => y.CompareTo(x))
        );
        
        Target = _player.gameObject;
        _targetDamageable = Target.GetComponent<IDamageable>();
        Pattern = GetComponent<EnemyPattern>();
        
        DamageInfo = new AttackInfo();
        DamageInfo.Damage = new DamageInfo((EDamageType)Enum.Parse(typeof(EDamageType), EnemyData.DamageType), EnemyData.DefaultDamage);

        maxHealth = maxHealth == 0 ? EnemyData.MaxHealth : maxHealth;
        Health = maxHealth;
        _armour = EnemyData.DefaultArmour;
    }

    public void UpdateAttackInfo(AttackInfo curAttackInfo)
    {
        DamageInfo = curAttackInfo;
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        Health = newMaxHealth;
    }
    
    private void OnPlayerDefeated(bool isRealDeath)
    {
        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
            spriteRenderer.material = _originalMaterial;

        StopAllCoroutines();
        Pattern.StopMovementCoroutines();
        _playerIsDefeated = true;
    }

    private void OnDestroy()
    {
        PlayerEvents.Defeated -= OnPlayerDefeated;
        if (_effectRemainingTimes != null)
        {
            if (_effectRemainingTimes[(int)EStatusEffect.Ecstasy] > 0)
                _player.RemoveEcstasyBuff(this);
            if (_effectRemainingTimes[(int)EStatusEffect.Evade] > 0)
                _player.RemoveShadowHost(this);
        }
        if (_enemyManager != null) _enemyManager.RemoveEnemyFromSpawnedList(this);
    }

    protected virtual void Update()
    {
        if (_isDead) return;
        UpdateRemainingStatusEffectTimes();
        UpdateEnemyPatternState();
        _damageCooltimeCounter += Time.unscaledDeltaTime;
    }

    protected virtual void Initialise() { }

    #region Status Effects imposed by the player handling
    private void HandleNewStatusEffects(List<StatusEffectInfo> statusEffects, Vector3 gravCorePosition, int incomingDirectionX = 0)
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
                        Pattern.ChangeSpeedByPercentage(statusEffect.Strength);
                        ChangeAttackSpeedByPercentage(statusEffect.Strength);
                        Debug.Log("This is the first stack of sommer." +
                                  "Change in move speed and attack speed is applied. now its move speed is " +
                                  Pattern.MoveSpeed + " and attack speed " + _animator.GetFloat("AttackSpeed"));
                    }
                    _sommerStackCount++;
                    _sommerTimeSinceStacked = 0f;
                    Debug.Log("Sommer was stacked. There are now " + _sommerStackCount + " stacks.");
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
                    if (Pattern.MoveType == EEnemyMoveType.Stationary) continue;
                    ShouldDisableMovement = true;
                    IsSilenced = true;
                    Pattern.StartPullX(incomingDirectionX, statusEffect.Strength, statusEffect.Duration);
                    break;
                
                case EStatusEffect.Evade or EStatusEffect.Camouflage:
                    if (!_player.AddShadowHost(this)) continue;
                    break;
                
                case EStatusEffect.GravityPull:
                    if (Pattern.MoveType == EEnemyMoveType.Stationary) continue;
                    IsSilenced = true;
                    Pattern.StartGravityPull(gravCorePosition, statusEffect.Strength, statusEffect.Duration);
                    break;
            }
            UpdateStatusEffectTime(statusEffect.Effect, statusEffect.Duration);
        }

        if (ShouldDisableMovement)
        {
            Pattern.DisableMovement();
        }
        ShouldDisableMovement = false;
    }

    private void UpdateStatusEffectTime(EStatusEffect effect, float duration)
    {
        if (duration == 0) return; 
        int effectIdx = (int)effect;

        // Apply new effect
        if (_effectRemainingTimes[effectIdx] <= 0)
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
        float deltaTime = Time.unscaledDeltaTime;
        
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
                Debug.Log("[" + gameObject.name + "] " + currEffect + " ended");
                
                _effectRemainingTimes[i] = 0.0f;

                if (currEffect is EStatusEffect.Sleep or EStatusEffect.Root or EStatusEffect.Airborne or EStatusEffect.Stun or EStatusEffect.Pull)
                {
                    Pattern.EnableMovement();
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

        _sommerTimeSinceStacked += Time.unscaledDeltaTime;

        if (_sommerTimeSinceStacked > _effectRemainingTimes[(int)EStatusEffect.Sommer])
            _sommerStackCount -= _sommerSubtractedStackPerSecond * Time.unscaledDeltaTime;

        if (_sommerStackCount >= 4)
        {
            Pattern.ResetMoveSpeed();
            ResetAttackSpeed();
            _sommerTimeSinceStacked = 0;
            _sommerStackCount = 0;
            Debug.Log("final stack. now its move speed is " + Pattern.MoveSpeed +
                      " and attack speed " + _animator.GetFloat("AttackSpeed"));
            HandleNewStatusEffects(new List<StatusEffectInfo> { new StatusEffectInfo(EStatusEffect.Sleep, 1, Define.SleepDuration) }, Vector3.zero);
            AttackInfo sleepDamage = new AttackInfo();
            sleepDamage.StatusEffects = new List<StatusEffectInfo> { new StatusEffectInfo(EStatusEffect.Sleep, 1, Define.SleepDuration) };
            TakeDamage(sleepDamage);
        }
        else if (_sommerStackCount <= 0)
        {
            SetVFXActive((int)EStatusEffect.Sommer, false);
            Pattern.ResetMoveSpeed();
            ResetAttackSpeed();
            _effectRemainingTimes[(int)EStatusEffect.Sommer] = 0;
        }
    }

    private void UpdateSlowTimes()
    {
        if (_slowRemainingTimes == null || _slowRemainingTimes.Count == 0) return;

        bool removed = false;
        foreach (float strength in _slowRemainingTimes.Keys.ToList())
        {
            _slowRemainingTimes[strength] -= Time.unscaledDeltaTime;
            if (_slowRemainingTimes[strength] <= 0.0f)
            {
                _slowRemainingTimes.Remove(strength);
                removed = true;
            }
        }

        // update slow strength (i.e. move speed)
        if (_slowRemainingTimes.Count == 0)
        {
            Pattern.ResetMoveSpeed();
            SetVFXActive(EStatusEffect.Slow, false);
        }
        else if (removed)
        {
            Pattern.ChangeSpeedByPercentage(_slowRemainingTimes.First().Key);
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
        Pattern.ChangeSpeedByPercentage(strength);
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
        if (_isDead) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerAbility"))
        {
            if (_audioSource)
            {
                _audioSource.pitch = Random.Range(0.8f, 1.2f);
                _audioSource.PlayOneShot(_hitAudio);   
            }
            int direction = 1;
            if (_player.transform.localScale.x >= 0) direction = -1;
            Instantiate(_enemyManager.TakeDamageVFXPrefab, other.ClosestPoint(transform.position), 
                Quaternion.Euler(new Vector3(-15f, direction * 90f, 0)));
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerAbility_IncludeWall"))
        {
            int direction = 1;
            if (_player.transform.localScale.x <= 0) direction = -1;
            Instantiate(_enemyManager.TakeDamageVFXPrefab, other.transform.position, 
                Quaternion.Euler(new Vector3(-15f, direction * 90f, 0)));
            return;
        }
        
        if (_damageCooltimeCounter <= _damageCooltime) return;
        if (!other.CompareTag(Target.tag)) return;
        
        // 황홀경 status effect
        if (_animator.GetBool(IsAttacking) && _effectRemainingTimes[(int)EStatusEffect.Ecstasy] > 0)
        {
            if (Random.Range(0.0f, 1.0f) <= 0.5f)
            {
                Debug.Log("Ecstasy: Attacking other enemies");
                Collider2D[] foesToDamage =
                    Physics2D.OverlapBoxAll(transform.position +new Vector3 (0, _neighboringEnemyColliderHeight, 0),
                        _neighboringEnemyColliderSize, 0, LayerMask.GetMask("Enemy"));
                AttackInfo damageToFoes = DamageInfo.Clone(cloneDamage:true, cloneStatusEffect:false);
                damageToFoes.Damage.TotalAmount *= 0.25f;
                foreach (Collider2D col in foesToDamage)
                {
                    DealDamage(col.gameObject.GetComponent<IDamageable>(), damageToFoes);
                }
                return;
            }
        }
        DealDamage(_targetDamageable, DamageInfo);
        _damageCooltimeCounter = 0f;
    }

    // Dealing damage to the player Handling
    public virtual void DealDamage(IDamageable target, AttackInfo damageInfo)
    {
        damageInfo.AttackerArmourPenetration = EnemyData.DefaultArmourPenetration;
        target.TakeDamage(damageInfo.Clone(cloneDamage:true, cloneStatusEffect:true));
    }

    // Received Damage Handling
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    
    public void TakeDamage(AttackInfo attackInfo)
    {
        HandleNewStatusEffects(attackInfo.StatusEffects, attackInfo.GravCorePosition, attackInfo.IncomingDirectionX);
        
        // 입면 환각
        var hypHallucinationPreserv = _player.playerDamageDealer.BindingSkillPreservations[(int)EWarrior.Sommer];
        if (_sommerStackCount <= 0)
        {
            DamageInfo boostedDamageInfo = attackInfo.Damage.Clone();
            boostedDamageInfo.TotalAmount += attackInfo.Damage.TotalAmount * Define.SommerHypHallucinationStats[(int)hypHallucinationPreserv];
            HandleNewDamage(boostedDamageInfo, attackInfo.AttackerArmourPenetration, attackInfo.ShouldLeech, attackInfo.ShouldUpdateTension);
        }
        // 그 외 경우
        else
        {
            HandleNewDamage(attackInfo.Damage.Clone(), attackInfo.AttackerArmourPenetration, attackInfo.ShouldLeech, attackInfo.ShouldUpdateTension);
        }
    }

    private void HandleNewDamage(DamageInfo damage, float attackerArmourPenetration, bool shouldLeech, bool shouldUpdateTension)
    {
        // 방어력 및 방어 관통력 처리
        var realDamage = Mathf.Max(0, damage.TotalAmount - Mathf.Max(GetArmour() - attackerArmourPenetration, 0));
        Debug.Log($"[{name}] Received {damage.TotalAmount}, Armour {GetArmour()}, Attacker's ArmourPen {attackerArmourPenetration}, Final: {realDamage}");
        damage.TotalAmount = realDamage;
        if (damage.TotalAmount == 0) return;
        StartCoroutine(DamageCoroutine(damage));
        
        // 흡혈 여부
        if (shouldLeech)
        {
            _player.playerDamageDealer.Leech(realDamage);
        }
        // 장력 게이지
        if (shouldUpdateTension) _uiManager.IncrementTensionGaugeUI();

        // if (asleep) then wake up
        if (_effectRemainingTimes[(int)EStatusEffect.Sleep] > 0)
        {
            Pattern.EnableMovement();
            _effectRemainingTimes[(int)EStatusEffect.Sleep] = -1;
            // Debug.Log("sleep ended.");
        }
    }

    private float GetArmour()
    {
        if (_effectRemainingTimes[(int)EStatusEffect.Sleep] > 0) return _armour;
        int sommerPreserv = (int)_player.playerDamageDealer.BindingSkillPreservations[(int)EWarrior.Sommer];
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

    private void ChangeHealthByAmount(float amount)
    {
        if (_isDead) return;
        
        Health = Mathf.Clamp(Health + amount, 0, maxHealth);
        if (amount < 0) StartCoroutine(DamagedRoutine(amount));
        if (Health == 0) Die();
    }

    private IEnumerator DamageCoroutine(DamageInfo damage)
    {
        // One-shot damage
        if (damage.Duration == 0)
        {
            ChangeHealthByAmount(-damage.TotalAmount);
            yield return null;
        }
        // Damage Over Time (DOT) damage
        // Deals damage.TotalAmount of damage every damage. Tick seconds for damage.Duration
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
    
    private IEnumerator DamagedRoutine(float amount)
    {
        Pattern.OnTakeDamage(amount, maxHealth);
        
        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
            spriteRenderer.material = _enemyManager.FlashMaterial;
        yield return new WaitForSeconds(0.05f);
        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
            spriteRenderer.material = _originalMaterial;
    }

    public void Die(bool shouldDropReward = true)
    {
        _isDead = true;
        
        // Inform player
        InGameEvents.EnemySlayed.Invoke(this);
        switch (typeID)
        {
            case 4:
                InGameEvents.MidBossSlayed?.Invoke();
                AudioManager.Instance.StopBgm(1.5f);
                StartCoroutine(OnMidBossDeath());
                return;
            
            case 5:
                StartCoroutine(OnBossDeath());
                return;
        }

        StopAllCoroutines();
        if (shouldDropReward)
        {
            DropGold();
            DropSoulShard();
            DropArbor();
        }

        Instantiate(_enemyManager.DeathVFXPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private IEnumerator OnMidBossDeath()
    {
        Pattern.OnDeath();
        yield return new WaitForSeconds(6f);
        transform.position = new Vector3(-9f, -5.5f, 0);
        for (int i = 0; i < 3; i++)
        {
            DropGold();
            DropSoulShard();
            yield return new WaitForSeconds(0.8f);
        }
        StopAllCoroutines();
        Destroy(gameObject);
    }

    private IEnumerator OnBossDeath()
    {
        for (int i = 0; i < 3; i++)
        {
            DropGold();
            DropSoulShard();
            yield return new WaitForSeconds(0.8f);
        }
        Pattern.OnDeath();
        yield return new WaitForSeconds(10f);
        StopAllCoroutines();
        Destroy(gameObject);
    }
    
    #endregion Damage Dealing and Receiving

    #region Enemy Pattern
    private void UpdateEnemyPatternState()
    {
        if (_playerIsDefeated)
        {
            Pattern.Patrol();
            return;
        }
        
        if (Pattern.IsRooted) return;
        ActionTimeCounter -= Time.deltaTime;
        
        if (Pattern.PlayerIsInAttackRange() || Pattern.IsAttackingPlayer)
        {
            Pattern.Attack();
            return;
        }
        
        if (Pattern.PlayerIsInDetectRange())
        {
            Pattern.IsChasingPlayer = true;
            ActionTimeCounter = EnemyData.ChasePlayerDuration;
            Pattern.Chase();
            return;
        }
        
        if (Pattern.IsChasingPlayer)
        {
            Pattern.Chase();
            if (ActionTimeCounter <= 0) Pattern.IsChasingPlayer = false;
            return;
        }

        Pattern.Patrol();
    }

    public void ChangeAttackSpeedByPercentage(float percentage)
    {
        float initial = _animator.GetFloat(AttackSpeed);
        _animator.SetFloat(AttackSpeed, initial * percentage);
        Pattern.AttackSpeed *= percentage;
    }

    public void ResetAttackSpeed()
    {
        _animator.SetFloat(AttackSpeed, 1f);
        Pattern.AttackSpeed = 1f;
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
        _uiManager.DisplayGoldPopUp(goldToDrop);
        for (int i = 0; i < goldToDrop / 5 + 1; i++) 
        {   
            // Instantiate a coin at the enemy's position
            var coin = Instantiate(_enemyManager.GoldPrefab, transform.position, Quaternion.identity).GetComponent<Gold>();
            coin.value = (i < goldToDrop / 5) ? 5 : goldToDrop % 5;
        }
    }

    private void DropSoulShard()
    {
        int amount = EnemyData.SoulShardDropAmount;
        float chance = EnemyData.SoulShardDropChance;
        int amountToDrop = 0;
        for (int i = 0; i < amount; i++)
        {
            if (Random.value <= chance) amountToDrop++;
        }
        if (amountToDrop == 0) return;
        
        float angleStep = 180f / Mathf.Max(1, amountToDrop - 1);
        float currentAngle = amountToDrop == 1 ? 0 : -90f;

        for (int i = 0; i < amountToDrop; i++)
        {
            GameObject soulShard = Instantiate(_enemyManager.SoulShardPrefab, transform.position + new Vector3(0,0.1f, 0), Quaternion.identity);
            Rigidbody2D rb = soulShard.GetComponent<Rigidbody2D>();
            Vector2 direction = new Vector2(Mathf.Sin(currentAngle * Mathf.Deg2Rad), Mathf.Abs(Mathf.Sin(currentAngle * Mathf.Deg2Rad)));
            rb.AddForce(direction * 5f, ForceMode2D.Impulse);
            currentAngle += angleStep;
        }
    }
    
    private void DropArbor()
    {
        float chance = EnemyData.ArborDropChance;
        if (Random.value > chance) return;
        GameObject arbor = Instantiate(_enemyManager.GetRandomArbor(), transform.position, Quaternion.identity);
        var randDir = Random.onUnitSphere;
        randDir = new Vector3(randDir.x, Mathf.Abs(randDir.y), 0);
        arbor.GetComponent<Rigidbody2D>().AddForce(randDir * Random.Range(7,10), ForceMode2D.Impulse);
        arbor.GetComponent<Arbor>().tensionController = _uiManager.TensionController;
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
