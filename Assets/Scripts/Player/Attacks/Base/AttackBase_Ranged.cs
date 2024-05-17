using UnityEngine;

public class AttackBase_Ranged : AttackBase
{
    [Space(15)][SerializeField] private Transform FireTransform;
    private GameObject _bulletObject;

    public override void Start()
    {
        base.Start();
        _damageInfoInit.BaseDamage = 3.0f;
        _attackInfoInit.Damage.TotalAmount = _damageInfoInit.BaseDamage + _playerController.Strength * _damageInfoInit.RelativeDamage;
        _attackInfoInit.CanBeDarkAttack = true;
        _attackInfoInit.ShouldUpdateTension = true;
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
            (Mathf.Sign(_player.localScale.x) < 0 ? Vector3.right : Vector3.zero);
        VFXObject.SetActive(true);
    }

    public void Fire()
    {
        var bullet = Instantiate(_bulletObject, FireTransform.position, Quaternion.identity).GetComponent<Bullet>();
        bullet.Direction = -Mathf.Sign(_player.localScale.x);
        bullet.Owner = this;
        bullet.attackInfo = _attackInfo.Clone();
        bullet.attackInfo.Damage.TotalAmount *= _damageDealer.attackDamageMultipliers[(int)EPlayerAttackType.Ranged];
        bullet.attackInfo.SetAttackDirToMyFront(_damageDealer.gameObject);
    }

    // Called when a shot bullet is destroyed for any reason
    public void OnHit(IDamageable target, Vector3 bulletPos, AttackInfo savedAttackInfo)
    {
        if (activeLegacy) 
            ((Legacy_Ranged)activeLegacy).OnHit(target, bulletPos);
        
        // Deal damage to the target if the bullet is hit
        if (target != null)
            _damageDealer.DealDamage(target, savedAttackInfo);
    }
}