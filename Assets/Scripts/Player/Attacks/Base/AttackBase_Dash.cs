using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class AttackBase_Dash : AttackBase
{
    private Rigidbody2D _rigidbody2D;
    private float _dashStrength = 8;
    private float _dashDuration = 0.35f;
    private readonly static int AttackIndex = Animator.StringToHash("AttackIndex");
    
    // Nightshade Dash
    private float _nightShadeDashTimeLimit;
    private GameObject _nightShadeDashPrefab;
    private GameObject _nightShadeDashShadow;

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
        _rigidbody2D = GetComponent<Rigidbody2D>();
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
            VFXObject.transform.localScale = new Vector3(Mathf.Sign(gameObject.transform.localScale.x), 1.0f, 1.0f);
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
            _nightShadeDashShadow = Instantiate(_nightShadeDashPrefab, transform.position, quaternion.identity);
            _nightShadeDashShadow.transform.localScale = transform.localScale;
            
            // Make the shadow dash
            StartCoroutine(nameof(NightShadeDashCoroutine));
        }
        // If active shadow exists, play VFX and teleport to the shadow position
        else
        {
            StartCoroutine(NightShadeTeleportCoroutine());
        }
    }

    public IEnumerator NightShadeTeleportCoroutine()
    {
        // Start teleport effect
        var shadowAnimator = _nightShadeDashShadow.GetComponent<Animator>(); 
        shadowAnimator.SetTrigger("Teleport");
        yield return null;
        
        // Wait until the effect ends
        while (shadowAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f)
        {
            yield return null;
        }
        
        // Match position and look direction
        transform.position = _nightShadeDashShadow.transform.position;
        if (!_playerMovement.IsMoving) transform.localScale = _nightShadeDashShadow.transform.localScale;
        
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
        shadowRb.velocity = new Vector2(-_nightShadeDashShadow.transform.localScale.x * _dashStrength * nsMultiplier, 0f);
        yield return new WaitForSeconds(_dashDuration);

        // End Dash
        _nightShadeDashShadow.GetComponent<Animator>().SetTrigger("DashEnd");
        shadowRb.velocity = Vector2.zero;
        yield return new WaitForSeconds(tpTimeLimit - _dashDuration);
        
        // Remove shadow when tp time limit ends
        Destroy(_nightShadeDashShadow);
        _nightShadeDashShadow = null;
    }

    private IEnumerator DashCoroutine()
    {
        // Start Dash
        float prevGravity = _rigidbody2D.gravityScale;
        _rigidbody2D.gravityScale = 0f;
        _rigidbody2D.velocity = new Vector2(-transform.localScale.x * _dashStrength, 0f);
        Coroutine legacyCoroutine = null;
        if (activeLegacy)
        {
            ((Legacy_Dash)activeLegacy).OnDashBegin();
            legacyCoroutine = StartCoroutine(((Legacy_Dash)activeLegacy).DashSpawnCoroutine());
        }
        yield return new WaitForSeconds(_dashDuration);

        // End Dash
        VFXObject.SetActive(false);
        _damageDealer.OnAttackEnd(ELegacyType.Dash);
        _rigidbody2D.gravityScale = prevGravity;
        if (activeLegacy)
        {
            ((Legacy_Dash)activeLegacy).OnDashEnd();
            if (legacyCoroutine != null) StopCoroutine(legacyCoroutine);
        }
    }
}