using System.Collections.Generic;
using UnityEngine;

public class AttackBase_Ranged : AttackBase
{
    [Space(15)][SerializeField] private Transform FireTransform;
    private GameObject _bulletObject;
    private readonly static int AttackIndex = Animator.StringToHash("AttackIndex");

    public override void Start()
    {
        base.Start();
        _damageInfoInit.BaseDamage = 3.0f;
        _attackInfoInit.Damage.TotalAmount = _damageInfoInit.BaseDamage + _playerController.Strength * _damageInfoInit.RelativeDamage;
        Reset();
    }

    public void SetBullet(GameObject bulletPrefab)
    {
        if (bulletPrefab == null) ResetBulletToDefault(); 
        else _bulletObject = bulletPrefab;
    }

    private void ResetBulletToDefault()
    {
        _bulletObject = Resources.Load<GameObject>("Prefabs/Player/Bullets/Bullet_Default");
    }

    public override void Reset()
    {
        base.Reset();
        activeLegacy = null;    
        ResetBulletToDefault();
        VFXObject.GetComponent<ParticleSystemRenderer>()
            .material.mainTexture = Resources.Load<Texture2D>("Sprites/Player/VFX/Default/Ranged");
    }

    public override void Attack()
    {
        _animator.SetInteger(AttackIndex, (int)ELegacyType.Ranged);
        VFXObject.GetComponent<ParticleSystemRenderer>().flip = 
            (Mathf.Sign(gameObject.transform.localScale.x) < 0 ? Vector3.right : Vector3.zero);
        VFXObject.SetActive(true);
    }

    public void Fire()
    {
        var bullet = Instantiate(_bulletObject, FireTransform.position, Quaternion.identity).GetComponent<Bullet>();
        bullet.Direction = -Mathf.Sign(gameObject.transform.localScale.x);
        bullet.Owner = this;
        bullet.attackInfo = _attackInfo.Clone();
        bullet.attackInfo.SetAttackDirToMyFront(_damageDealer.gameObject);
    }

    // Called when a shot bullet is destroyed for any reason
    public void OnBulletDestroy(IDamageable target, Vector3 bulletPos, AttackInfo savedAttackInfo)
    {
        Debug.Log(target);
        if (activeLegacy) 
            ((Legacy_Ranged)activeLegacy).OnBulletDestroy(bulletPos);
        
        // Deal damage to the target if the bullet is hit
        if (target != null)
            _damageDealer.DealDamage(target, savedAttackInfo);
    }
}