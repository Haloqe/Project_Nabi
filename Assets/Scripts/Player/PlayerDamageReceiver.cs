using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerDamageReceiver : MonoBehaviour, IDamageable
{
    // reference to other components
    private PlayerMovement _playerMovement;
    private PlayerController _playerController;
    private UIManager _uiManager;
    private GameManager _gameManager;
    private GameObject _resurrectionVFX;
    private Material _flashMaterial;
    private Material _originalMaterial;
    
    // Health attributes
    private float _currHealth;
    public float MaxHealth => BaseHealth + additionalHealth;
    public float BaseHealth { get; private set; }
    public float additionalHealth;
    public bool canResurrect;
    private bool _resurrectAnimEnded;
    private bool _shouldNotTakeDamage;

    // status effect attributes
    private bool _isResurrectFinished;
    public bool IsSilenced { get; private set; }
    public bool IsSilencedExceptCleanse { get; private set; }
    [NamedArray(typeof(EDamageType))] public GameObject[] DamageEffects;
    private int[] _activeDOTCounts;
    private float[] _effectRemainingTimes;
    private SortedDictionary<float, float> _slowRemainingTimes; // str,time
    private List<Coroutine> _activeDamageCoroutines;
    
    // Extra
    public float debuffTimeReductionRatio;
    [FormerlySerializedAs("damageReduction")] public float damageReductionRatio;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private readonly static int IsDead = Animator.StringToHash("IsDead");
    private readonly static int ShouldResurrect = Animator.StringToHash("ShouldResurrect");

    private void Awake()
    {
        BaseHealth = 200;//000;
        _currHealth = MaxHealth;
    }
    
    private void OnRestarted()
    {
        RemoveAllDebuffs();
        IsSilenced = false;
        IsSilencedExceptCleanse = false;
        _shouldNotTakeDamage = false;
        additionalHealth = 0;
        debuffTimeReductionRatio = 0;
        damageReductionRatio = 0;
        _currHealth = MaxHealth;
        canResurrect = _gameManager.PlayerMetaData.metaUpgradeLevels[(int)EMetaUpgrade.Resurrection] != -1;
        _slowRemainingTimes.Clear();
        _activeDamageCoroutines.Clear();
    }
    
    private void Start()
    {
        _resurrectionVFX = transform.Find("ResurrectionVFX").gameObject;
        _activeDamageCoroutines = new List<Coroutine>();
        canResurrect = false;
        _uiManager = UIManager.Instance;
        _gameManager = GameManager.Instance;
        _playerController = PlayerController.Instance;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _flashMaterial = Resources.Load("Materials/FlashMaterial") as Material;
        _originalMaterial = _spriteRenderer.material;
        
        PlayerEvents.Defeated += OnPlayerDefeated;
        GameEvents.Restarted += OnRestarted;
        GameEvents.CombatSceneChanged += OnCombatSceneChanged;
        
        _playerMovement = GetComponent<PlayerMovement>();
        _effectRemainingTimes = new float[(int)EStatusEffect.MAX];
        _slowRemainingTimes = new SortedDictionary<float, float>(
            Comparer<float>.Create(delegate (float x, float y) { return y.CompareTo(x); })
        );
        _activeDOTCounts = new int[(int)EDamageType.MAX];
    }

    private void OnDestroy()
    {
        if (GetComponent<PlayerController>().IsToBeDestroyed) return;
        PlayerEvents.Defeated -= OnPlayerDefeated;
        GameEvents.Restarted -= OnRestarted;
        GameEvents.CombatSceneChanged -= OnCombatSceneChanged;
    }

    private void OnCombatSceneChanged()
    {
        RemoveAllDebuffs();
    }
    
    private void Update()
    {
        UpdateRemainingStatusEffectTimes();
    }

    //updates the remaining time of various status effects
    private void UpdateRemainingStatusEffectTimes()
    {
        float deltaTime = Time.unscaledDeltaTime;
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
                _playerController.SetVFXActive(i, false);
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
            _playerMovement.ResetMoveSpeed();
            _playerController.SetVFXActive(EStatusEffect.Slow, false);
        }
        else if (removed)
        {
            _playerMovement.ChangeSpeedByPercentage(_slowRemainingTimes.First().Key);
        }
    }

    #region Damage Dealing and Receiving
    // IDamageable Override

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public float[] GetEffectRemainingTimes()
    {
        return _effectRemainingTimes;
    }
    
    public void TakeDamage(AttackInfo damageInfo)
    {
        if (_playerMovement.isDashing) return;
        if (_shouldNotTakeDamage) return;
        
        // Attempt evade
        if (Random.value <= _playerController.EvasionRate)
        {
            _uiManager.DisplayPlayerEvadePopUp();
            return;
        }
        
        // Evade failed?
        HandleNewDamage(damageInfo.Damage.Clone(), damageInfo.AttackerArmourPenetration);
        HandleNewStatusEffects(damageInfo.StatusEffects);
    }    

    private void HandleNewDamage(DamageInfo damage, float attackerArmourPenetration)
    {
        // 플레이어 방어력 처리
        damage.TotalAmount = Mathf.Max(damage.TotalAmount - (_playerController.Armour - attackerArmourPenetration), 0);
        
        // 피해 감소
        damage.TotalAmount -= damage.TotalAmount * damageReductionRatio;
        
        Debug.Log("Player damaged: " + damage.TotalAmount);
        StartCoroutine(DamageCoroutine(damage));
    }
    
    private IEnumerator DamageCoroutine(DamageInfo damage)
    {
        // Run the actual coroutine
        var damageCoroutine = StartCoroutine(DamageCoroutineInternal(damage));
        _activeDamageCoroutines.Add(damageCoroutine);
        yield return damageCoroutine;

        // Coroutine has finished, perform cleanup
        _activeDamageCoroutines.Remove(damageCoroutine); 
    }

    private void OnPlayerDefeated()
    {
        _shouldNotTakeDamage = true;
        StopAllCoroutines();
        _animator.SetBool(IsDead, true);
        _spriteRenderer.color = Color.white;
    }

    private IEnumerator DamageCoroutineInternal(DamageInfo damage)
    {
        int damageTypeIdx = (int)damage.Type;
        
        // One-shot damage
        if (damage.Duration == 0)
        {
            ChangeHealthByAmount(-damage.TotalAmount);
            if (_playerController.updateTensionUponHit) _uiManager.IncrementTensionGaugeUI();
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
            var damageCoroutine = StartCoroutine(DOTDamageCoroutineInternal(damagePerTick, damage.Tick));
            _activeDamageCoroutines.Add(damageCoroutine);
            yield return new WaitForSeconds(damage.Duration + damage.Tick / 2.0f);

            // Do cleanup
            // Stop applying damage if the duration is over
            StopCoroutine(damageCoroutine);
            _activeDamageCoroutines.Remove(damageCoroutine);

            // If this is the last effect, activate the VFX
            if (--_activeDOTCounts[damageTypeIdx] == 0 && DamageEffects[damageTypeIdx] != null)
            {
                DamageEffects[damageTypeIdx].SetActive(false);
            }
        }
    }

    private IEnumerator DOTDamageCoroutineInternal(float damagePerTick, float tick)
    {
        while (true)
        {
            // Wait for a tick time and take damage repeatedly
            ChangeHealthByAmount(-damagePerTick);
            if (_playerController.updateTensionUponHit) _uiManager.IncrementTensionGaugeUI();
            yield return new WaitForSeconds(tick);
        }
    }

    public void ChangeHealthByAmount(float changeAmount, bool byEnemy = true)
    {
        // TODO hit/heal effect
        float prevHPRatio = GetHPRatio();
        if (changeAmount < 0) StartCoroutine(DamagedRoutine());
        _currHealth = Mathf.Clamp(_currHealth + changeAmount, 0, MaxHealth);
        PlayerEvents.HpChanged.Invoke(changeAmount, prevHPRatio, GetHPRatio());
        if (_currHealth == 0) Die();
    }

    private void Die()
    {
        _spriteRenderer.material = _originalMaterial;
        // Can resurrect?
        if (canResurrect) StartCoroutine(ResurrectCoroutine());
        // Die
        else PlayerEvents.Defeated.Invoke();
    }

    private void StopAllDamageCoroutines()
    {
        foreach (var coroutine in _activeDamageCoroutines)
        {
            StopCoroutine(coroutine);
        }
        _activeDamageCoroutines.Clear();
    }
    
    private IEnumerator ResurrectCoroutine()
    {
        // Start resurrect
        PlayerEvents.StartResurrect.Invoke();
        Array.Clear(_effectRemainingTimes, 0, _effectRemainingTimes.Length);
        Array.Clear(_activeDOTCounts, 0, _activeDOTCounts.Length);
        StopAllDamageCoroutines();
        _shouldNotTakeDamage = true;
        _slowRemainingTimes.Clear();
        _spriteRenderer.color = Color.white;
        canResurrect = false;
        _isResurrectFinished = false;
        IsSilenced = false;
        
        IsSilencedExceptCleanse = false;
        _animator.SetBool(IsDead, true);
        _animator.SetBool(ShouldResurrect, true);

        yield return null;
        yield return null;
        _animator.SetBool(IsDead, false); // prevent transition to death animation
        
        // Wait for animation to end
        yield return new WaitUntil(() => _isResurrectFinished);
        
        // End resurrect
        PlayerEvents.EndResurrect.Invoke();
        ChangeHealthByAmount(MaxHealth 
            * Define.MetaResurrectionHealthRatio[_gameManager.PlayerMetaData.metaUpgradeLevels[(int)EMetaUpgrade.Resurrection]]);
        
        _animator.Rebind();
        _animator.Update(0f);
        _shouldNotTakeDamage = false;
    }

    public void OnResurrectAnimationStart()
    {
        _resurrectionVFX.SetActive(true);
    }
    
    public void OnResurrectAnimationEnd()
    {
        _isResurrectFinished = true;
    }
    
    // TODO FIX: damage visualisation
    private IEnumerator DamagedRoutine()
    {
        // _spriteRenderer.color = new Color(0.267f, 0.9f, 0.99f);
        // yield return new WaitForSeconds(0.1f);
        // _spriteRenderer.color = Color.white;
        _spriteRenderer.material = _flashMaterial;
        yield return new WaitForSecondsRealtime(0.1f);
        _spriteRenderer.material = _originalMaterial;
    }

    public float GetHPRatio()
    {
        return _currHealth / MaxHealth;
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
            UpdateNewStatusEffectTime(statusEffect.Effect, statusEffect.Duration);

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

    private void UpdateNewStatusEffectTime(EStatusEffect effect, float duration)
    {
        if (duration == 0) return; 

        int effectIdx = (int)effect;
        // apply new effect
        if (_effectRemainingTimes[effectIdx] == 0)
        {
            _playerController.SetVFXActive(effectIdx, true);
            Debug.Log("New " + effect + " time: " + duration.ToString("0.0000"));
        }
        // or increment effect time
        else
        {
            duration = Mathf.Clamp(duration - _effectRemainingTimes[effectIdx], 0, float.MaxValue);
            Debug.Log("Previous " + effect + " time: " + _effectRemainingTimes[effectIdx] + 
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
            _playerController.SetVFXActive(EStatusEffect.Slow, true);
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
    
    /// <summary>
    ///  현재는 isSliencedExceptCleanse 포함해서 제거함. Silenced는 유지.
    /// </summary>
    private void RemoveAllDebuffs()
    {
        for (int i = 0; i < (int)EStatusEffect.MAX; i++)
        {
            if (i == (int)EStatusEffect.Silence) continue;
            _playerController.SetVFXActive(i, false);
            _effectRemainingTimes[i] = 0.0f;
        }
        _slowRemainingTimes.Clear();
        _playerMovement.RemoveDebuffs();
        Array.Clear(_activeDOTCounts, 0, _activeDOTCounts.Length);
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
