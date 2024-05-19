using UnityEngine;

public class AttackBase_Area : AttackBase
{
    private PlayerInventory _inventory;
    private float _areaRadius;
    private float _areaRadiusMultiplier;

    // Stopping player movement
    private Vector3 _prevVelocity;
    private Rigidbody2D _rigidbody2D;

    //detecting which flower to use
    public int currentSelectedFlowerIndex;
    public string vfxAddress;
    StatusEffectInfo bombEffect;

    public override void Start()
    {
        _attackType = ELegacyType.Area;
        vfxAddress = "Prefabs/Player/BombVFX/IncendiaryBombVFX";
        VFXObject = Utility.LoadGameObjectFromPath(vfxAddress);
        base.Start();
        _rigidbody2D = _player.GetComponent<Rigidbody2D>();
        _attackInfoInit = new AttackInfo
        {
            Damage = new DamageInfo(EDamageType.Base, 5),
            ShouldUpdateTension = true,
        };
        _inventory = _playerController.playerInventory;
        Reset();
    }

    public void SwitchVFX(int flowerIdx)
    {
        currentSelectedFlowerIndex = flowerIdx;
        ReassignVFXAddress();
        VFXObject = Utility.LoadGameObjectFromPath(vfxAddress);
    }
    
    private void ReassignVFXAddress()
    {  
        switch (currentSelectedFlowerIndex)
        {
            case 0:
                Debug.Log("Will heal you on R");
                break;

            case 1:
                vfxAddress = "Prefabs/Player/BombVFX/IncendiaryBombVFX";
                break;

            case 2:
                vfxAddress = "Prefabs/Player/BombVFX/StickyBombVFX";
                bombEffect = new StatusEffectInfo(EStatusEffect.Slow, 1, 3);
                _attackInfoInit.StatusEffects.Add(bombEffect);
                Reset();
                break;

            case 3:
                vfxAddress = "Prefabs/Player/BombVFX/BlizzardBombVFX";
                bombEffect = new StatusEffectInfo(EStatusEffect.Stun, 1, 3);
                _attackInfoInit.StatusEffects.Add(bombEffect);
                Reset();
                break;

            case 4:
                vfxAddress = "Prefabs/Player/BombVFX/GravityBombVFX";
                break;
        }
        
    }
        
    public override void Reset()
    {
        //base.Reset();
        _attackInfo = _attackInfoInit.Clone();
        //_damageInfo = _damageInfoInit;
        //VFXObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = _defaultVFXMaterial;
        _areaRadiusMultiplier = 1f;
        activeLegacy = null;
        _attackPostDelay = 0.0f;
    }

    public bool CheckAvailability()
    {
        // Is flower available?
        if (_inventory.GetNumberOfFlowers(currentSelectedFlowerIndex) <= 0)
        {
            _inventory.noFlowerVFX.SetActive(true);
            return false;
        }
        
        return currentSelectedFlowerIndex != 0;
        // _playerController.Heal(15);
        // return;
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
        Vector3 vfxPos = VFXObject.transform.position;
        Vector3 position = new Vector3(playerPos.x + dir * vfxPos.x, playerPos.y + vfxPos.y, playerPos.z + vfxPos.z);
        
        VFXObject.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
        var vfx = Instantiate(VFXObject, position, Quaternion.identity);
        vfx.GetComponent<Bomb>().Owner = this;
        //FollowUpVFX();
        
        // Decrement flower
        _playerController.playerInventory.RemoveFlower(currentSelectedFlowerIndex);
    }

    private void FollowUpVFX()
    {
        if (currentSelectedFlowerIndex == (int)EFlowerType.IncendiaryFlower)
        {
            //instantiate fire VFX
            string fireVfxAddress = "Prefabs/Player/BombVFX/FireBundleTEST";
            GameObject FireVFXObject = Utility.LoadGameObjectFromPath(fireVfxAddress);

            float dir = Mathf.Sign(_player.localScale.x);
            Vector3 playerPos = _player.position;
            Vector3 vfxPos = FireVFXObject.transform.position;
            Vector3 position = new Vector3(playerPos.x + dir * (vfxPos.x), playerPos.y + vfxPos.y, playerPos.z + vfxPos.z);

            FireVFXObject.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
            var vfx = Instantiate(FireVFXObject, position, Quaternion.identity);
            vfx.GetComponent<Fire>().Owner = this;
        }
    }

    protected override void OnAttackEnd_PreDelay()
    {
        _rigidbody2D.gravityScale = _playerController.DefaultGravityScale / Time.timeScale;
        _rigidbody2D.velocity = new Vector2(_prevVelocity.x, 0); // _prevVelocity
        _playerMovement.isAreaAttacking = false;
    }

    public void DealDamage(IDamageable target)
    {
        var attackToSend = _attackInfo.Clone();
        attackToSend.Damage.TotalAmount *= _damageDealer.attackDamageMultipliers[(int)EPlayerAttackType.Area];
        Debug.Log("Damage: " + attackToSend.Damage.TotalAmount);
        _damageDealer.DealDamage(target, attackToSend);
    }
}