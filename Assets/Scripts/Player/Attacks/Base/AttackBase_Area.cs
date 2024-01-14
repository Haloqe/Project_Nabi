using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackBase_Area : AttackBase
{
    private Legacy_Area _activeLegacy;
    private float _areaRadius;
    private float _areaRadiusMultiplier;

    // Stopping player movement
    private float _prevGravity;
    private Vector3 _prevVelocity;
    private Rigidbody2D _rigidbody2D;

    public override void Initialise()
    {
        base.Initialise();
        _attackType = ELegacyType.Area;
        _isAttached = false;
        _vfxObject = Utility.LoadGameObjectFromPath("Prefabs/Player/AttackVFXs/Area_Default");
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _damageInitBase = new SDamageInfo
        {
            Damages = new List<SDamage> { new SDamage(EDamageType.Base, 15) },
            StatusEffects = new List<SStatusEffect>(),
        };
    }

    public override void Reset()
    {
        base.Reset();
        _areaRadiusMultiplier = 1f;
        _activeLegacy = null;
        _attackPostDelay = 0.0f;
    }

    public override void Attack()
    {
        _animator.SetInteger("AttackIndex", (int)_attackType);

        // Zero out movement
        _prevGravity = _rigidbody2D.gravityScale;
        _prevVelocity = _rigidbody2D.velocity;
        _rigidbody2D.gravityScale = 0.0f;
        _rigidbody2D.velocity = Vector3.zero;
        GetComponent<PlayerMovement>().IsAreaAttacking = true;

        // Instantiate VFX
        float dir = Mathf.Sign(gameObject.transform.localScale.x);
        Vector3 playerPos = gameObject.transform.position;
        Vector3 vfxPos = _vfxObject.transform.position;
        Vector3 position = new Vector3(playerPos.x + dir * (vfxPos.x), playerPos.y + vfxPos.y, playerPos.z + vfxPos.z);

        _vfxObject.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
        var vfx = Instantiate(_vfxObject, position, Quaternion.identity);
        vfx.GetComponent<Bomb>().Owner = this;
    }

    protected override void OnAttackEnd_PreDelay()
    {
        _rigidbody2D.gravityScale = _prevGravity;
        _rigidbody2D.velocity = new Vector2(_prevVelocity.x, 0); // _prevVelocity
        GetComponent<PlayerMovement>().IsAreaAttacking = false;
    }

    public void DealDamage(IDamageable target)
    {
        _damageDealer.DealDamage(target, _damageBase);
    }
}