using System.Collections.Generic;
using UnityEngine;

public class AttackBase_Ranged : AttackBase
{
    [SerializeField] private Transform fireTransform;
    private GameObject _bulletPrefab;
    private List<Bullet> _activeBullets;
    
    // SFX
    public AudioClip fireSound;
    public AudioClip[] bulletHitFleshSounds;
    public AudioClip[] bulletHitWallSounds;

    public override void Start()
    {
        base.Start();
        _attackType = ELegacyType.Ranged;
        _damageInfoInit.BaseDamage = 7f;
        _attackInfoInit.Damage.TotalAmount = _damageInfoInit.BaseDamage + _playerController.Strength * _damageInfoInit.RelativeDamage;
        _attackInfoInit.CanBeDarkAttack = true;
        _attackInfoInit.ShouldUpdateTension = true;
        _activeBullets = new List<Bullet>();
        
        InGameEvents.TimeSlowDown += OnTimeManipulate;
        InGameEvents.TimeRevertNormal += OnTimeManipulate;
        Reset();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        InGameEvents.TimeSlowDown -= OnTimeManipulate;
        InGameEvents.TimeRevertNormal -= OnTimeManipulate;
    }
    
    protected override void Reset()
    {
        base.Reset();
        ResetBulletToDefault();
        OnCombatSceneChanged();
    }
    
    protected override void OnCombatSceneChanged()
    {
        _activeBullets.Clear();
    }

    private void OnTimeManipulate()
    {
        foreach (var bullet in _activeBullets)
        {
            bullet.UpdateVelocityOnTimeScaleChange();
        }
    }
    
    public void SetBullet(GameObject bulletPrefab)
    {
        if (bulletPrefab == null) ResetBulletToDefault(); 
        else _bulletPrefab = bulletPrefab;
    }

    private void ResetBulletToDefault()
    {
        _bulletPrefab = PlayerAttackManager.Instance.GetBulletPrefab(EWarrior.MAX);
    }

    public override void Attack()
    {
        _damageDealer.AudioSource.PlayOneShot(fireSound, 0.5f);
        _animator.SetInteger(AttackIndex, (int)ELegacyType.Ranged);
        BasePSRenderer.flip = Mathf.Sign(_player.localScale.x) < 0 ? Vector3.right : Vector3.zero;
        //baseEffector.SetActive(true);
        baseEffector.GetComponent<ParticleSystem>().Play();
    }

    public void Fire()
    {
        var bullet = Instantiate(_bulletPrefab, fireTransform.position, Quaternion.identity).GetComponent<Bullet>();
        bullet.Direction = -Mathf.Sign(_player.localScale.x);
        bullet.Owner = this;
        bullet.attackInfo = _attackInfo.Clone(cloneDamage:true, cloneStatusEffect:false);
        bullet.attackInfo.Damage.TotalAmount *= _damageDealer.attackDamageMultipliers[(int)EPlayerAttackType.Ranged];
        bullet.attackInfo.SetAttackDirToMyFront(_damageDealer.gameObject);
        _activeBullets.Add(bullet);
    }

    // Called when a shot bullet is destroyed for any reason
    public void OnHit(Bullet bullet, IDamageable target, Vector3 bulletPos, AttackInfo savedAttackInfo)
    {
        if (ActiveLegacy) 
            ((Legacy_Ranged)ActiveLegacy).OnHit(target, bulletPos);
        
        // Deal damage to the target if the bullet is hit
        if (target != null)
        {
            _damageDealer.DealDamage(target, savedAttackInfo);
            _damageDealer.AudioSource.PlayOneShot(bulletHitFleshSounds[Random.Range(0,bulletHitFleshSounds.Length)],
                Mathf.Clamp((float)(1.2 / Vector3.Distance(bullet.transform.position, _player.position)), 0.3f, 1f));
        }
        
        _activeBullets.Remove(bullet);
    }
}
