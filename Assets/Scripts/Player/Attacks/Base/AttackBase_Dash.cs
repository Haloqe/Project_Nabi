using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class AttackBase_Dash : AttackBase
{
    private Rigidbody2D _rigidbody2D;
    private float _dashStrength = 2.5f;
    private float _dashDuration = 0.35f;
    
    // Nightshade Dash
    private float _nightShadeDashTimeLimit;
    private GameObject _nightShadeDashPrefab;
    private GameObject _nightShadeDashShadow;
    private readonly static int DashEnd = Animator.StringToHash("DashEnd");
    private readonly static int Teleport = Animator.StringToHash("Teleport");

    public override void Start()
    {
        base.Start();
        _attackType = ELegacyType.Dash;
        _attackInfoInit = new AttackInfo();
        _nightShadeDashPrefab = Resources.Load("Prefabs/Player/NightShadeDash").GameObject();
        _rigidbody2D = _player.GetComponent<Rigidbody2D>();
        Reset();
    }
    
    protected override void Reset()
    {
        base.Reset();
        _nightShadeDashShadow = null;
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

    // Is current dash attack NightShade's second dash (teleport)?
    public bool IsCurrentNightShadeTpDash()
    {
        return _nightShadeDashShadow != null; 
    }
    
    private void NightShadeDash()
    {
        // No active shadow
        if (_nightShadeDashShadow == null)
        {
            // Instantiate shadow at player's position and match direction
            _nightShadeDashShadow = Instantiate(_nightShadeDashPrefab, _player.position, quaternion.identity);
            _nightShadeDashShadow.transform.localScale = _player.localScale;
            
            // Make the shadow dash
            StartCoroutine(nameof(NightShadeDashCoroutine));
        }
        // If active shadow exists, play VFX and teleport to the shadow position
        else
        {
            StartCoroutine(NightShadeTeleportCoroutine());
        }
    }

    private IEnumerator NightShadeTeleportCoroutine()
    {
        // Save info in case shadow disappears while waiting for the effect
        var targetPos = _nightShadeDashShadow.transform.position;
        var targetScale = _nightShadeDashShadow.transform.localScale;
        
        // Start teleport effect
        var shadowAnimator = _nightShadeDashShadow.GetComponent<Animator>(); 
        shadowAnimator.SetTrigger(Teleport);
        yield return null;
        
        // Wait until the effect ends
        while (shadowAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f)
        {
            yield return null;
        }
        
        // Match position and look direction
        _player.position = targetPos;
        if (!_playerMovement.IsMoving) _player.localScale = targetScale;
        
        // Teleport attack
        ((Legacy_Dash)ActiveLegacy).OnDashEnd();
            
        // Reset shadow
        StopCoroutine(nameof(NightShadeDashCoroutine));
        Destroy(_nightShadeDashShadow);
        _nightShadeDashShadow = null;
    }
    
    private IEnumerator NightShadeDashCoroutine()
    {
        // Start Dash
        float tpTimeLimit = 6f;
        float nsMultiplier = 1.5f; 
        var shadowRb = _nightShadeDashShadow.GetComponent<Rigidbody2D>();
        shadowRb.velocity = new Vector2(
            (-_nightShadeDashShadow.transform.localScale.x * _dashStrength * nsMultiplier
            * _playerMovement.MoveSpeed * _playerMovement.moveSpeedMultiplier) / Time.timeScale, 0f);
        yield return new WaitForSecondsRealtime(_dashDuration);

        // End Dash
        _nightShadeDashShadow.GetComponent<Animator>().SetTrigger(DashEnd);
        shadowRb.velocity = Vector2.zero;
        yield return new WaitForSecondsRealtime(tpTimeLimit - _dashDuration);
        
        // Remove shadow when tp time limit ends
        Destroy(_nightShadeDashShadow);
        _nightShadeDashShadow = null;
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
