using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackBase_Area : AttackBase
{
    
    private float _areaRadius;
    private float _areaRadiusMultiplier;
    private List<SBombInfo> _bombIdentification = new List<SBombInfo>
        {
            new SBombInfo {Name = "NectarFlower", Description = "체력 회복.", SpriteIndex = 0},
            new SBombInfo {Name = "IncendiaryBombVFX", Description = "적을 불태운다.", SpriteIndex = 1},
            new SBombInfo {Name = "StickyBombVFX", Description = "슬로우 제공.", SpriteIndex = 2},
            new SBombInfo {Name = "BlizzardBombVFX", Description = "스턴 제공.", SpriteIndex = 3},
            new SBombInfo {Name = "GravityBombVFX", Description = "끌어당김.", SpriteIndex = 4}};

    // Stopping player movement
    private float _prevGravity;
    private Vector3 _prevVelocity;
    private Rigidbody2D _rigidbody2D;

    //detecting which flower to use
    public int currentSelectedFlowerIndex;
    public string vfxAddress;

    public override void Start()
    {
       //_attackType = ELegacyType.Area;
        vfxAddress = "Prefabs/Player/BombVFX/IncendiaryBombVFX";
        VFXObject = Utility.LoadGameObjectFromPath(vfxAddress);
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _attackInfoInit = new AttackInfo
        {
            Damage = new DamageInfo(EDamageType.Base, 15),
        };
        base.Start();
    }

    //call this function whens
    public void SwitchVFX()
    {
        CheckCurrentFlower();
        ReassignVFXAddress();
        Debug.Log(vfxAddress);
        VFXObject = Utility.LoadGameObjectFromPath(vfxAddress);
    }

    //fetch the address of current selected flower if the number is not 0 
    public void CheckCurrentFlower()
    {
        currentSelectedFlowerIndex = FindObjectOfType<PlayerInventory>().GetCurrentSelectedFlower();
        Debug.Log("current selected flower is: " + currentSelectedFlowerIndex);

    }

    public int CheckNumberOfCurrentFlower()
    {
        int number = FindObjectOfType<PlayerInventory>().GetNumberOfFlowers(currentSelectedFlowerIndex);
        return number;
    }
    public void ReassignVFXAddress()
    {  
        switch (currentSelectedFlowerIndex)
        {
            case 0:
                Debug.Log("Healed");
                break;

            case 1:
                vfxAddress = "Prefabs/Player/BombVFX/IncendiaryBombVFX";
                Debug.Log(vfxAddress);
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
        
    public override void Reset()
    {
        base.Reset();
        _areaRadiusMultiplier = 1f;
        activeLegacy = null;
        _attackPostDelay = 0.0f;
    }

    public override void Attack()
    {
        _animator.SetInteger("AttackIndex", (int)ELegacyType.Area);

        CheckCurrentFlower();
        if (CheckNumberOfCurrentFlower() <= 0)
        {
            Debug.Log("There is no flower bomb to use!");
            return;
        }
            
        // Zero out movement
        _prevGravity = _rigidbody2D.gravityScale;
        _prevVelocity = _rigidbody2D.velocity;
        _rigidbody2D.gravityScale = 0.0f;
        _rigidbody2D.velocity = Vector3.zero;
        GetComponent<PlayerMovement>().IsAreaAttacking = true; 

        // Instantiate VFX
        float dir = Mathf.Sign(gameObject.transform.localScale.x);
        Vector3 playerPos = gameObject.transform.position;
        Vector3 vfxPos = VFXObject.transform.position;
        Vector3 position = new Vector3(playerPos.x + dir * (vfxPos.x), playerPos.y + vfxPos.y, playerPos.z + vfxPos.z);

        VFXObject.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
        var vfx = Instantiate(VFXObject, position, Quaternion.identity);
        vfx.GetComponent<Bomb>().Owner = this;

        FindObjectOfType<PlayerInventory>().DecreaseToFlower(currentSelectedFlowerIndex);
    }

    protected override void OnAttackEnd_PreDelay()
    {
        _rigidbody2D.gravityScale = _prevGravity;
        _rigidbody2D.velocity = new Vector2(_prevVelocity.x, 0); // _prevVelocity
        _playerMovement.IsAreaAttacking = false;
    }

    public void DealDamage(IDamageable target)
    {
        _damageDealer.DealDamage(target, _attackInfo);
    }
}