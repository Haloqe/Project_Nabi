using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBase_Dash : AttackBase
{
    private Legacy_Dash ActiveLegacy;
    private Rigidbody2D _rigidbody2D;
    private float _dashStrength = 8;

    public override void Reset()
    {
        base.Reset();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public override void Attack()
    {
        _animator.SetInteger("AttackIndex", (int)ELegacyType.Dash);
        VFXObject.transform.localScale = new Vector3(Mathf.Sign(gameObject.transform.localScale.x), 1.0f, 1.0f);
        VFXObject.SetActive(true);
        StartCoroutine(Dash());
    }

    private IEnumerator Dash()
    {
        float prevGravity = _rigidbody2D.gravityScale;
        _rigidbody2D.gravityScale = 0f;
        _rigidbody2D.velocity = new Vector2(-transform.localScale.x * _dashStrength, 0f);
        yield return new WaitForSeconds(0.35f);

        VFXObject.SetActive(false);
        _damageDealer.OnAttackEnd(ELegacyType.Dash);
        _rigidbody2D.gravityScale = prevGravity;
        //_rigidbody2D.velocity = new Vector2(0f, 0f);
    }
}