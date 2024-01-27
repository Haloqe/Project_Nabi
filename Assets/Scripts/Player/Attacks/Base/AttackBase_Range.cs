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

    // Called when a shot bullet is destroyed for any reason
    public void OnBulletDestroy(IDamageable target, Vector3 bulletPos)
    {
        if (_activeLegacy) _activeLegacy.OnBulletDestroy(bulletPos);
        
        // Deal damage to the target if the bullet is hit
        if (target != null)
        {
            // TODO: Adjust damage from the legacy
            _damageDealer.DealDamage(target, _damageBase);
        }
    }
    
    public override void BindActiveLegacy(LegacySO legacyAsset)
    {
        legacyAsset.PlayerTransform = gameObject.transform;
        _activeLegacy = (Legacy_Range)legacyAsset;
        RecalculateDamages();
    }
    
    public void RecalculateDamages()
    {
        // TODO 대장장이
        
        // Legacy - Damage
        var newBaseDamage = _damageInitBase.Damages[0];
        newBaseDamage.TotalAmount *= _activeLegacy.BaseDamageMultiplier[(int)_activeLegacyPreservation];
        _damageBase.Damages[0] = newBaseDamage;
        
        // Legacy - Status effect
        var newStatusEffectsBase = _damageBase.StatusEffects;
        EStatusEffect warriorSpecificEffect =
            PlayerAttackManager.Instance.GetWarriorStatusEffect(_activeLegacy.Warrior,
                _damageDealer.GetStatusEffectLevel(_activeLegacy.Warrior));
        var newEffect = new SStatusEffect(warriorSpecificEffect,
            _activeLegacy.StatusEffectStrength[(int)_activeLegacyPreservation],
            _activeLegacy.StatusEffectDuration[(int)_activeLegacyPreservation]);
        newStatusEffectsBase.Add(newEffect);
        _damageBase.StatusEffects = newStatusEffectsBase;
    }
}