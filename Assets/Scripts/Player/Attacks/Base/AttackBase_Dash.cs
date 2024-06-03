using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class AttackBase_Dash : AttackBase
{
    // Base Dash
    private Rigidbody2D _rigidbody2D;
    private readonly float _dashStrength = 2.5f;
    private readonly float _dashDuration = 0.35f;
    
    // Nightshade Dash
    private readonly float _nightShadeDashTimeLimit = 4.5f;
    private readonly float _nightShadeDashSpeedMultiplier = 1.3f;
    private bool _hasAliveNightShadeShadow;
    private bool _isTeleporting;
    private GameObject _nightShadeDashPrefab;
    private GameObject _nightShadeDashShadow;
    private Animator _nightShadeDashShadowAnimator;
    private Rigidbody2D _nightShadeDashShadowRigidbody;
    private readonly static int DashStart = Animator.StringToHash("DashStart");
    private readonly static int DashEnd = Animator.StringToHash("DashEnd");
    private readonly static int Teleport = Animator.StringToHash("Teleport");

    public override void Start()
    {
        base.Start();
        _attackType = ELegacyType.Dash;
        _attackInfoInit = new AttackInfo();
        _nightShadeDashPrefab = Resources.Load("Prefabs/Effects/Player/NightShadeDash").GameObject();
        _rigidbody2D = _player.GetComponent<Rigidbody2D>();
        Reset();
    }
    
    protected override void Reset()
    {
        base.Reset();
        OnCombatSceneChanged();
    }

    protected override void OnCombatSceneChanged()
    {
        StopAllCoroutines();
        _nightShadeDashShadow = null;
        _hasAliveNightShadeShadow = false;
        _isTeleporting = false;
        _nightShadeDashShadow = Instantiate(_nightShadeDashPrefab);
        _nightShadeDashShadowAnimator = _nightShadeDashShadow.GetComponent<Animator>();
        _nightShadeDashShadowRigidbody = _nightShadeDashShadow.GetComponent<Rigidbody2D>();
        _nightShadeDashShadow.SetActive(false);
    }
    
    public override void Attack()
    {
        if (ActiveLegacy != null && ActiveLegacy.warrior == EWarrior.NightShade)
        {
            NightShadeDash();
        }
        else
        {
            _animator.SetInteger(AttackIndex, (int)ELegacyType.Dash);
            baseEffector.transform.localScale = new Vector3(Mathf.Sign(_player.localScale.x), 1.0f, 1.0f);
            baseEffector.SetActive(true);
            StartCoroutine(DashCoroutine());
        }
    }

    // Currently teleporting?
    public bool IsCurrentlyNightShadeTeleporting()
    {
        return _isTeleporting;
    }
    
    // Is current dash attack NightShade's second dash (teleport)?
    public bool IsCurrentNightShadeTpDash()
    {
        return _hasAliveNightShadeShadow && !_isTeleporting; 
    }
    
    private void NightShadeDash()
    {
        // No active shadow
        if (!_hasAliveNightShadeShadow)
        {
            // Reactivate shadow
            _nightShadeDashShadow.SetActive(true);
            _hasAliveNightShadeShadow = true;
            
            // Reset animation to its initial state
            _nightShadeDashShadowAnimator.SetTrigger(DashStart);
            
            // Reposition shadow at player's position and match direction
            _nightShadeDashShadow.transform.position = _player.position;
            _nightShadeDashShadow.transform.localScale = _player.localScale;
            
            // Make the shadow dash
            StartCoroutine(nameof(NightShadeDashCoroutine));
        }
        // If an active shadow exists, play VFX and teleport to the shadow position
        else
        {
            StartCoroutine(NightShadeTeleportCoroutine());
        }
    }

    private IEnumerator NightShadeTeleportCoroutine()
    {
        // Start teleport effect
        _isTeleporting = true;
        _nightShadeDashShadowAnimator.SetTrigger(Teleport);
        yield return null;
        
        // Wait until the effect ends
        while (_nightShadeDashShadowAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
        
        // Match position and look direction
        _player.position = _nightShadeDashShadow.transform.position;
        if (!_playerMovement.IsMoving) _player.localScale = _nightShadeDashShadow.transform.localScale;
        
        // Teleport attack
        ((Legacy_Dash)ActiveLegacy).OnDashEnd();
            
        // Reset shadow
        StopCoroutine(nameof(NightShadeDashCoroutine));
        _nightShadeDashShadow.SetActive(false);
        _hasAliveNightShadeShadow = false;
        _isTeleporting = false;
    }
    
    private IEnumerator NightShadeDashCoroutine()
    {
        // Start Dash
        _nightShadeDashShadowRigidbody.velocity = new Vector2(
            (-_nightShadeDashShadow.transform.localScale.x * _dashStrength * _nightShadeDashSpeedMultiplier
            * _playerMovement.MoveSpeed * _playerMovement.moveSpeedMultiplier) / Time.timeScale, 0f);
        yield return new WaitForSecondsRealtime(_dashDuration);

        // End Dash
        _nightShadeDashShadowAnimator.SetTrigger(DashEnd);
        _nightShadeDashShadowRigidbody.velocity = Vector2.zero;
        yield return new WaitForSecondsRealtime(_nightShadeDashTimeLimit - _dashDuration);
        
        // Remove shadow when tp time limit ends
        _nightShadeDashShadow.SetActive(false);
        _hasAliveNightShadeShadow = false;
    }

    private IEnumerator DashCoroutine()
    {
        // Start Dash
        _rigidbody2D.gravityScale = 0f;
        _rigidbody2D.velocity = new Vector2(
            (-_player.localScale.x * _dashStrength * _playerMovement.MoveSpeed 
            * _playerMovement.moveSpeedMultiplier) / Time.timeScale, 0f);
        Coroutine legacyCoroutine = null;
        
        // Legacy extra effect?
        if (ActiveLegacy)
        {
            ((Legacy_Dash)ActiveLegacy).OnDashBegin();
            legacyCoroutine = StartCoroutine(((Legacy_Dash)ActiveLegacy).DashSpawnCoroutine());
        }
        
        // Delay during dash
        yield return new WaitForSecondsRealtime(_dashDuration);

        // End Dash
        baseEffector.SetActive(false);
        _damageDealer.OnAttackEnd(ELegacyType.Dash);
        _rigidbody2D.gravityScale = _playerController.DefaultGravityScale / Time.timeScale;
        _rigidbody2D.velocity = Vector2.zero;
        
        // Legacy extra effect?
        if (ActiveLegacy)
        {
            ((Legacy_Dash)ActiveLegacy).OnDashEnd();
            if (legacyCoroutine != null) StopCoroutine(legacyCoroutine);
        }
    }
}
