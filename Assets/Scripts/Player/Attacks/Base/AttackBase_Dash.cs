using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBase_Dash : AttackBase
{
    private Rigidbody2D _rigidbody2D;
    private float _dashStrength = 8;
    private readonly static int AttackIndex = Animator.StringToHash("AttackIndex");

    public override void Start()
    {
        base.Start();
        _attackInfoInit = new AttackInfo();
        Reset();
    }
    
    public override void Reset()
    {
        base.Reset();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        VFXObject.GetComponent<ParticleSystemRenderer>()
            .material.mainTexture = Resources.Load<Texture2D>("Sprites/Player/VFX/Default/Dash");
    }

    public override void Attack()
    {
        _animator.SetInteger(AttackIndex, (int)ELegacyType.Dash);
        VFXObject.transform.localScale = new Vector3(Mathf.Sign(gameObject.transform.localScale.x), 1.0f, 1.0f);
        VFXObject.SetActive(true);
        StartCoroutine(Dash());
    }

    private IEnumerator Dash()
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
        yield return new WaitForSeconds(0.35f);

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