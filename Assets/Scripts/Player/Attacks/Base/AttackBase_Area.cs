using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.UIElements;

public class AttackBase_Area : AttackBase
{
    
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
       //_attackType = ELegacyType.Area;
        vfxAddress = "Prefabs/Player/BombVFX/IncendiaryBombVFX";
        VFXObject = Utility.LoadGameObjectFromPath(vfxAddress);
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _attackInfoInit = new AttackInfo
        {
            Damage = new DamageInfo(EDamageType.Base, 5),
        };
        base.Start();
        Reset();
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
        base.Reset();
        //_attackInfo = _attackInfoInit.Clone();
        //_damageInfo = _damageInfoInit;
        //VFXObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = _defaultVFXMaterial;
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
        
        if (currentSelectedFlowerIndex == 0)
        {
            FindObjectOfType<PlayerController>().Heal(15);
            Debug.Log("Healed");
            return;
            //TODO: don't play the throwing motion at all
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

        FollowUpVFX();

        //FindObjectOfType<PlayerInventory>().DecreaseToFlower(currentSelectedFlowerIndex);
    }

    public void FollowUpVFX()
    {
        if (currentSelectedFlowerIndex == 1)
        {
            //instantiate fire VFX
            string fireVfxAddress = "Prefabs/Player/BombVFX/FireBundleTEST";
            GameObject FireVFXObject = Utility.LoadGameObjectFromPath(fireVfxAddress);

            float dir = Mathf.Sign(gameObject.transform.localScale.x);
            Vector3 playerPos = gameObject.transform.position;
            Vector3 vfxPos = FireVFXObject.transform.position;
            Vector3 position = new Vector3(playerPos.x + dir * (vfxPos.x), playerPos.y + vfxPos.y, playerPos.z + vfxPos.z);

            FireVFXObject.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
            var vfx = Instantiate(FireVFXObject, position, Quaternion.identity);
            //vfx.GetComponent<Fire>().Owner = this;
            for (int i = 0; i < vfx.transform.childCount; i++)
            {
                Transform child = vfx.transform.GetChild(i);
                child.GetComponent<Fire>().Owner = this;
            }
        }
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
        Debug.Log("Gave" + _attackInfo.Damage.TotalAmount + "damage!");
    }
}