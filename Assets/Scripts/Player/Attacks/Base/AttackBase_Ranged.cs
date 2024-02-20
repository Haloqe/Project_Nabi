using System.Collections.Generic;
using UnityEngine;

public class AttackBase_Ranged : AttackBase
{
    [Space(15)][SerializeField] private Transform FireTransform;
    private GameObject _bulletObject;

    public override void Start()
    {
        _damageInitBase = new AttackInfo();
        _damageInitBase.Damages.Add(new DamageInfo(EDamageType.Base, 3));
        base.Start();
    }

    public void SetBullet(GameObject bulletPrefab)
    {
        if (bulletPrefab == null) ResetBulletToDefault(); 
        else _bulletObject = bulletPrefab;
    }

    public void ResetBulletToDefault()
    {
        _bulletObject = Resources.Load<GameObject>("Prefabs/Player/Bullets/Bullet_Default");
    }

    public override void Reset()
    {
        base.Reset();
        _activeLegacy = null;    
        ResetBulletToDefault();
        VFXObject.GetComponent<ParticleSystemRenderer>()
            .material.mainTexture = Resources.Load<Texture2D>("Sprites/Player/VFX/Default/Ranged");
    }

    public override void Attack()
    {
        _animator.SetInteger("AttackIndex", (int)ELegacyType.Ranged);
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
        bullet.attackInfo = _damageBase.Clone();
        bullet.attackInfo.SetAttackDirToMyFront(_damageDealer.gameObject);
    }

    // Called when a shot bullet is destroyed for any reason
    public void OnBulletDestroy(IDamageable target, Vector3 bulletPos, AttackInfo savedAttackInfo)
    {
        if (_activeLegacy) 
            ((Legacy_Ranged)_activeLegacy).OnBulletDestroy(bulletPos);
        
        // Deal damage to the target if the bullet is hit
        if (target != null)
        {
            savedAttackInfo.Damages.Clear();
            _damageDealer.DealDamage(target, savedAttackInfo);
        }
    }
}