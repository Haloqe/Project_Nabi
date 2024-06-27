using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBase_Area : AttackBase
{
    private PlayerInventory _inventory;
    private float _areaRadius;
    private float _areaRadiusMultiplier;
    private AttackInfo[] _bombAttackInfos;
    private bool _isInitialised = false;
    
    // VFX Gameobjects
    private GameObject[] _bombVFXs;
    private GameObject _incendiaryFollowupVFX;
    private GameObject _gravityFollowupVFX;

    // Stopping player movement
    private Vector3 _prevVelocity;
    private Rigidbody2D _rigidbody2D;

    //detecting which flower to use
    private int _currSelectedFlower;
    
    // SFX
    [SerializeField] private AudioClip[] explodeSounds;
    
    public override void Start()
    {
        base.Start();
        _currSelectedFlower = 0;
        _rigidbody2D = _player.GetComponent<Rigidbody2D>();
        _inventory = _playerController.GetComponent<PlayerInventory>();
        _attackType = ELegacyType.Area;
        
        // Preload gameobjects
        _bombVFXs = new GameObject[]
        {
            null,
            Utility.LoadGameObjectFromPath("Prefabs/Player/BombVFX/IncendiaryBombVFX"),
            Utility.LoadGameObjectFromPath("Prefabs/Player/BombVFX/StickyBombVFX"),
            Utility.LoadGameObjectFromPath("Prefabs/Player/BombVFX/BlizzardBombVFX"),
            Utility.LoadGameObjectFromPath("Prefabs/Player/BombVFX/GravityBombVFX"),
        };
        _incendiaryFollowupVFX = Utility.LoadGameObjectFromPath("Prefabs/Player/BombVFX/FireBundle");
        _gravityFollowupVFX = Utility.LoadGameObjectFromPath("Prefabs/Player/BombVFX/GravityField");
        
        // Attack info
        _bombAttackInfos = new AttackInfo[]
        {
            null,
            new AttackInfo
            {
                Damage = new DamageInfo(EDamageType.Base, 20),
                ShouldUpdateTension = false,
            },
            new AttackInfo
            {
                Damage = new DamageInfo(EDamageType.Base, 7),
                StatusEffects = new List<StatusEffectInfo>{new StatusEffectInfo(EStatusEffect.Slow, 0.5f, 3)},
                ShouldUpdateTension = false,
            },
            new AttackInfo
            {
                Damage = new DamageInfo(EDamageType.Base, 10),
                StatusEffects = new List<StatusEffectInfo>{new StatusEffectInfo(EStatusEffect.Stun, 1, 3)},
                ShouldUpdateTension = false,
            },
            new AttackInfo
            {
                Damage = new DamageInfo(EDamageType.Base, 7),
                ShouldUpdateTension = false,
            },
        };
        _attackInfo = _bombAttackInfos[(int)EFlowerType.IncendiaryFlower];
        baseEffector = _bombVFXs[(int)EFlowerType.IncendiaryFlower];
        _isInitialised = true;
    }

    protected override void OnCombatSceneChanged()
    {
        
    }
    
    public void UpdateVFX(int flowerIdx)
    {
        _currSelectedFlower = flowerIdx;
        if (!_isInitialised) return;
        baseEffector = _bombVFXs[_currSelectedFlower];
        _attackInfo = _bombAttackInfos[_currSelectedFlower];
    }

    public bool CheckAvailability()
    {
        UIManager.Instance.DisplayFlowerBombUI();
        if (_inventory.GetNumberOfFlowers(_inventory.GetCurrentSelectedFlower()) <= 0)
        {
            _inventory.noFlowerVFX.SetActive(true);
            return false;
        }
        return true;
    }
    
    public override void Attack()
    {
        // Play player animation
        _animator.SetInteger(AttackIndex, (int)ELegacyType.Area);

        // Zero out movement
        _prevVelocity = _rigidbody2D.velocity;
        _rigidbody2D.gravityScale = 0.0f;
        _rigidbody2D.velocity = Vector3.zero;
        _playerMovement.isAreaAttacking = true; 

        // Instantiate VFX
        float dir = Mathf.Sign(_player.localScale.x);
        Vector3 playerPos = _player.transform.position;
        Vector3 vfxPos = baseEffector.transform.position;
        Vector3 position = new Vector3(playerPos.x + dir * vfxPos.x, playerPos.y + vfxPos.y, playerPos.z + vfxPos.z);
        
        baseEffector.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
        var vfx = Instantiate(baseEffector, position, Quaternion.identity);
        vfx.GetComponent<Bomb>().Owner = this;

        // Any followups?
        if (_currSelectedFlower == (int)EFlowerType.GravityFlower)
        {
            StartCoroutine(BombExpansion(position));
        }
        else if (_currSelectedFlower == (int)EFlowerType.IncendiaryFlower)
        {
            StartCoroutine(FireBundleSpawn(position));
        }

        // Remove the used flower bomb
        _playerController.playerInventory.RemoveFlower(_inventory.GetCurrentSelectedFlower());
    }

    private IEnumerator BombExpansion(Vector3 position)
    {
        yield return new WaitForSecondsRealtime(1.4f);
        GravityFieldSpawn(position);
    }

    private IEnumerator FireBundleSpawn(Vector3 baseEffectorPos)
    {
        yield return new WaitForSecondsRealtime(1.25f);
        Debug.Log("Firebundlespawn");
        var vfx = Instantiate(_incendiaryFollowupVFX, baseEffectorPos, Quaternion.identity);
        vfx.GetComponent<Fire>().Owner = this;
    }

    private void GravityFieldSpawn(Vector3 position)
    {
        var gravityField = Instantiate(_gravityFollowupVFX, position, Quaternion.identity).GetComponent<GravityField>();
        gravityField.Owner = this;
        gravityField.GravCorePos = position;
    }

    protected override void OnAttackEnd_PreDelay()
    {
        _rigidbody2D.gravityScale = _playerController.DefaultGravityScale / Time.timeScale;
        _rigidbody2D.velocity = new Vector2(_prevVelocity.x, 0);
        _playerMovement.isAreaAttacking = false;
    }

    public void DealDamage(IDamageable target, AttackInfo followupAttackInfo = null)
    {
        AttackInfo attackInfoToSend;
        if (followupAttackInfo == null)
        {
            attackInfoToSend = _attackInfo.Clone(cloneDamage:true, cloneStatusEffect:false);
            attackInfoToSend.Damage.TotalAmount *= _damageDealer.attackDamageMultipliers[(int)EPlayerAttackType.Area];
        }
        else
        {
            attackInfoToSend = followupAttackInfo.Clone(cloneDamage:true, cloneStatusEffect:false);
        }
        _damageDealer.DealDamage(target, attackInfoToSend);
    }

    public void PlayExplosionSound()
    {
        _damageDealer.AudioSource.PlayOneShot(explodeSounds[Random.Range(0, explodeSounds.Length)]);
    }
}
