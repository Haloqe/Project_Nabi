using System.Collections;
using Unity.VisualScripting;
using UnityEditor.TextCore.Text;
using UnityEngine;

public class AttackBase_Area : AttackBase
{
    private PlayerInventory _inventory;
    private float _areaRadius;
    private float _areaRadiusMultiplier;

    // Stopping player movement
    private float _prevGravity;
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

/*        _attackInfoInit = new AttackInfo
        {
            Damage = new DamageInfo(EDamageType.Base, 5),
            ShouldUpdateTension = true,
        };*/

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

            case 1:
                vfxAddress = "Prefabs/Player/BombVFX/IncendiaryBombVFX";
                break;

            case 2:
                vfxAddress = "Prefabs/Player/BombVFX/StickyBombVFX";
                break;

            case 3:
                vfxAddress = "Prefabs/Player/BombVFX/BlizzardBombVFX";
                break;

            case 4:
                vfxAddress = "Prefabs/Player/BombVFX/GravityBombVFX";
                break;
        }
        
    }
    private void ReassignDamage(bool isSpawn)
    {
        if (isSpawn)
        {
            if(currentSelectedFlowerIndex == (int)EFlowerType.IncendiaryFlower)
            {
                 _attackInfoInit = new AttackInfo
                 {
                     Damage = new DamageInfo(EDamageType.Base, 2),
                     ShouldUpdateTension = true,
                 };
                 Reset();
            }
            
        }

        else
        {
            switch (currentSelectedFlowerIndex)
            {

                case 1:
                    _attackInfoInit = new AttackInfo
                    {
                        Damage = new DamageInfo(EDamageType.Base, 5),
                        ShouldUpdateTension = true,
                    };
                    break;

                case 2:
                    _attackInfoInit = new AttackInfo
                    {
                        Damage = new DamageInfo(EDamageType.Base, 5),
                        ShouldUpdateTension = true,
                    };

                    bombEffect = new StatusEffectInfo(EStatusEffect.Slow, 0.5f, 3);
                    _attackInfoInit.StatusEffects.Add(bombEffect);
                    break;

                case 3:
                    _attackInfoInit = new AttackInfo
                    {
                        Damage = new DamageInfo(EDamageType.Base, 5),
                        ShouldUpdateTension = true,
                    };

                    bombEffect = new StatusEffectInfo(EStatusEffect.Stun, 1, 3);
                    _attackInfoInit.StatusEffects.Add(bombEffect);
                    break;

                case 4:
                    vfxAddress = "Prefabs/Player/BombVFX/GravityBombVFX";
                    _attackInfoInit = new AttackInfo
                    {
                        Damage = new DamageInfo(EDamageType.Base, 5),
                        ShouldUpdateTension = true,
                    };
                    break;
            }

            Reset();
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
        
        //_attackInfoInit 초기화
        _attackInfoInit = new AttackInfo
        {
            Damage = new DamageInfo(EDamageType.Base, 5),
            ShouldUpdateTension = true,
        };

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
        
    }
    
    public override void Attack()
    {
        // Play player animation
        _animator.SetInteger(AttackIndex, (int)ELegacyType.Area);

        // Zero out movement
        _prevGravity = _rigidbody2D.gravityScale;
        _prevVelocity = _rigidbody2D.velocity;
        _rigidbody2D.gravityScale = 0.0f;
        _rigidbody2D.velocity = Vector3.zero;
        _playerMovement.IsAreaAttacking = true; 

        // Instantiate VFX
        float dir = Mathf.Sign(_player.transform.localScale.x);
        Vector3 playerPos = _player.transform.position;
        Vector3 vfxPos = VFXObject.transform.position;
        Vector3 position = new Vector3(playerPos.x + dir * (vfxPos.x), playerPos.y + vfxPos.y, playerPos.z + vfxPos.z);

        VFXObject.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
        var vfx = Instantiate(VFXObject, position, Quaternion.identity);
        vfx.GetComponent<Bomb>().Owner = this;

        if (currentSelectedFlowerIndex == (int)EFlowerType.GravityFlower)
        {
            //spawn gravity field on completion of bomb VFX
            StartCoroutine(BombExpansion(vfx, position, dir));
        }
        else
        {
            if (currentSelectedFlowerIndex == (int)EFlowerType.IncendiaryFlower)
            {
                FireBundleSpawn();
            }
            
        }

        // Decrement flower
        //_playerController.playerInventory.RemoveFlower(currentSelectedFlowerIndex);
    }

    IEnumerator BombExpansion(GameObject vfx, Vector3 position, float dir)
    {
        yield return new WaitForSeconds(1.4f);
        Debug.Log("Waited for 1.4 seconds!");
        GravityFieldSpawn(position, dir);

    }

    private void FireBundleSpawn()
    {
        if (currentSelectedFlowerIndex == (int)EFlowerType.IncendiaryFlower)
        {
            //instantiate fire VFX
            string fireVfxAddress = "Prefabs/Player/BombVFX/FireBundle";
            GameObject FireVFXObject = Utility.LoadGameObjectFromPath(fireVfxAddress);

            float dir = Mathf.Sign(_player.transform.localScale.x);
            Vector3 playerPos = _player.transform.position;
            Vector3 vfxPos = FireVFXObject.transform.position;
            Vector3 position = new Vector3(playerPos.x + dir * (vfxPos.x), playerPos.y + vfxPos.y, playerPos.z + vfxPos.z);

            FireVFXObject.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
            var vfx = Instantiate(FireVFXObject, position, Quaternion.identity);
            vfx.GetComponent<Fire>().Owner = this;
        }
        
    }

    private void GravityFieldSpawn(Vector3 position, float dir)
    {
        _attackInfoInit = new AttackInfo
        {
            Damage = new DamageInfo(EDamageType.Base, 0),
            ShouldUpdateTension = false,
            GravCorePosition = position
        };

        bombEffect = null;
        bombEffect = new StatusEffectInfo(EStatusEffect.Pull, 75, 5);
        _attackInfoInit.StatusEffects.Add(bombEffect);
        Reset();

        //instantiate Gravity VFX
        string GraivtyFieldVfxAddress = "Prefabs/Player/BombVFX/GravityField";
        GameObject GravityFieldVFXObject = Utility.LoadGameObjectFromPath(GraivtyFieldVfxAddress);


        GravityFieldVFXObject.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
        var vfx = Instantiate(GravityFieldVFXObject, position, Quaternion.identity);
        vfx.GetComponent<GravityField>().Owner = this;

    }

    protected override void OnAttackEnd_PreDelay()
    {
        _rigidbody2D.gravityScale = _prevGravity;
        _rigidbody2D.velocity = new Vector2(_prevVelocity.x, 0); // _prevVelocity
        _playerMovement.IsAreaAttacking = false;
    }

    public void DealDamage(IDamageable target, bool isSpawn)
    {
        ReassignDamage(isSpawn);

        var attackToSend = _attackInfo.Clone();
        attackToSend.Damage.TotalAmount *= _damageDealer.attackDamageMultipliers[(int)EPlayerAttackType.Area];
        Debug.Log("Damage: " + attackToSend.Damage.TotalAmount);
        _damageDealer.DealDamage(target, attackToSend);
    }
}