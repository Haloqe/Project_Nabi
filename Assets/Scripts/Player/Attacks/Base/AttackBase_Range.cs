using System.Collections.Generic;
using UnityEngine;

public class AttackBase_Range : AttackBase
{
    // VFX
    [SerializeField] private GameObject _vfxObj;

    private Legacy_Range _activeLegacy;
    [SerializeField] private Transform FireTransform;
    [SerializeField] private GameObject BulletObject;

    public override void Initialise()
    {
        base.Initialise();
        _damageInitBase = new SDamageInfo
        {
            Damages = new List<SDamage> { new SDamage(EDamageType.Base, 3) },
            StatusEffects = new List<SStatusEffect>(),
        };
    }

    public override void Reset()
    {
        base.Reset();
        _activeLegacy = null;        
    }

    public override void Attack()
    {
        _animator.SetInteger("AttackIndex", (int)ELegacyType.Range);
        _vfxObj.GetComponent<ParticleSystemRenderer>().flip = 
            (Mathf.Sign(gameObject.transform.localScale.x) < 0 ? Vector3.right : Vector3.zero);
        _vfxObj.SetActive(true);
    }

    public void Fire()
    {
        var bullet = Instantiate(BulletObject, FireTransform.position, Quaternion.identity)
            .GetComponent<Bullet>();
        bullet.Direction = -Mathf.Sign(gameObject.transform.localScale.x);
        bullet.Owner = this;
    }

    public void DealDamage(IDamageable target)
    {
        // TODO: Damage alteration from the legacy
        _damageDealer.DealDamage(target, _damageBase);
    }
}