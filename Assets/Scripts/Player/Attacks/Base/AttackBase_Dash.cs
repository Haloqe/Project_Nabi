using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class AttackBase_Dash : AttackBase
{
    private Rigidbody2D _rigidbody2D;
    private float _dashStrength = 2.5f;
    private float _dashDuration = 0.35f;
    private readonly static int AttackIndex = Animator.StringToHash("AttackIndex");
    
    // Nightshade Dash
    private float _nightShadeDashTimeLimit;
    private GameObject _nightShadeDashPrefab;
    private GameObject _nightShadeDashShadow;
    private readonly static int DashEnd = Animator.StringToHash("DashEnd");
    private readonly static int Teleport = Animator.StringToHash("Teleport");

    public override void Start()
    {
        base.Start();
        _attackInfoInit = new AttackInfo();
        _nightShadeDashPrefab = Resources.Load("Prefabs/Player/NightShadeDash").GameObject();
        Reset();
    }
    
    public override void Reset()
    {
        base.Reset();
        _rigidbody2D = _player.GetComponent<Rigidbody2D>();
        VFXObject.GetComponent<ParticleSystemRenderer>()
            .material.mainTexture = Resources.Load<Texture2D>("Sprites/Player/VFX/Default/Dash");
        _nightShadeDashShadow = null;
    }

    public override void Attack()
    {
        if (activeWarrior == EWarrior.NightShade)
        {
            NightShadeDash();
        }
        else
        {
            _animator.SetInteger(AttackIndex, (int)ELegacyType.Dash);
            VFXObject.transform.localScale = new Vector3(Mathf.Sign(_player.localScale.x), 1.0f, 1.0f);
            VFXObject.SetActive(true);
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
        _player.position = _nightShadeDashShadow.transform.position;
        if (!_playerMovement.IsMoving) _player.localScale = _nightShadeDashShadow.transform.localScale;
        
        // Teleport attack
        ((Legacy_Dash)activeLegacy).OnDashEnd();
            
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
        if (activeLegacy)
        {
            ((Legacy_Dash)activeLegacy).OnDashBegin();
            legacyCoroutine = StartCoroutine(((Legacy_Dash)activeLegacy).DashSpawnCoroutine());
        }
        
        // Delay during dash
        yield return new WaitForSecondsRealtime(_dashDuration);

        // End Dash
        VFXObject.SetActive(false);
        _damageDealer.OnAttackEnd(ELegacyType.Dash);
        _rigidbody2D.gravityScale = _playerController.DefaultGravityScale / Time.timeScale;
        _rigidbody2D.velocity = Vector2.zero;
        
        // Legacy extra effect?
        if (activeLegacy)
        {
            ((Legacy_Dash)activeLegacy).OnDashEnd();
            if (legacyCoroutine != null) StopCoroutine(legacyCoroutine);
        }
    }
}