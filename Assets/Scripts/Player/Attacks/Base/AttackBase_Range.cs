using System.Collections.Generic;
using UnityEngine;

public class AttackBase_Range : AttackBase
{
    private Legacy_Range _activeLegacy;
    [SerializeField] private Transform FireTransform;
    private GameObject _bulletObject;

    public override void Start()
    {
        _damageInitBase = new SDamageInfo
        {
            Damages = new List<SDamage> { new SDamage(EDamageType.Base, 3) },
            StatusEffects = new List<SStatusEffect>(),
        };
        base.Start();
    }

    public void SetBullet(GameObject bulletPrefab)
    {
        if (bulletPrefab == null) ResetBulletToDefault(); 
        else _bulletObject = bulletPrefab;
    }

    public void ResetBulletToDefault()
    {
        _bulletObject = Resources.Load<GameObject>("Prefabs/Player/Bullet_Default");
    }

    

    public override void Reset()
    {
        base.Reset();
        _activeLegacy = null;    
        ResetBulletToDefault();
        VFXObject.GetComponent<ParticleSystemRenderer>()
            .material.mainTexture = Resources.Load<Texture2D>("Sprites/Player/VFX/Default/Range");
    }

    public override void Attack()
    {
        _animator.SetInteger("AttackIndex", (int)ELegacyType.Range);
        VFXObject.GetComponent<ParticleSystemRenderer>().flip = 
            (Mathf.Sign(gameObject.transform.localScale.x) < 0 ? Vector3.right : Vector3.zero);
        VFXObject.SetActive(true);
    }

    public void Fire()
    {
        var bullet = Instantiate(_bulletObject, FireTransform.position, Quaternion.identity)
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