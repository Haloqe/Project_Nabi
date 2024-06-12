using System.Collections;
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
    private int _currSelectedFlower;
    public string vfxAddress;
    StatusEffectInfo bombEffect;
    
    // SFX
    [SerializeField] private AudioClip[] explodeSounds;
    
    public override void Start()
    {
        base.Start();
        _currSelectedFlower = 0;
        _rigidbody2D = _player.GetComponent<Rigidbody2D>();
        _inventory = _playerController.GetComponent<PlayerInventory>();
        _attackType = ELegacyType.Area;
        vfxAddress = "Prefabs/Player/BombVFX/IncendiaryBombVFX";
        baseEffector = Utility.LoadGameObjectFromPath(vfxAddress);
        Reset();
    }

    protected override void OnCombatSceneChanged()
    {
        
    }
    
    public void UpdateVFX(int flowerIdx)
    {
        _currSelectedFlower = flowerIdx;
        // TODO: 미리 로드해둔 게임오브젝트, 어택인포 리스트에서 해당 폭탄꽃 찾아서 assign 
        ReassignVFXAddress();
        baseEffector = Utility.LoadGameObjectFromPath(vfxAddress);
    }
    
    private void ReassignVFXAddress()
    {
        switch (_currSelectedFlower)
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
    
    // TODO 계속 다시 로드하지 말고 미리 로드 후 assign
    private void ReassignDamage(bool isSpawn)
    {
        if (isSpawn)
        {
            if(_currSelectedFlower == (int)EFlowerType.IncendiaryFlower)
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
            switch (_currSelectedFlower)
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
        }
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

        if (_currSelectedFlower == (int)EFlowerType.GravityFlower)
        {
            //spawn gravity field on completion of bomb VFX
            StartCoroutine(BombExpansion(position, dir));
        }
        else
        {
            if (_currSelectedFlower == (int)EFlowerType.IncendiaryFlower)
            {
                FireBundleSpawn();
            }
        }

        // Decrement flower
        _playerController.playerInventory.RemoveFlower(_inventory.GetCurrentSelectedFlower());
    }

    private IEnumerator BombExpansion(Vector3 position, float dir)
    {
        yield return new WaitForSecondsRealtime(1.4f);
        GravityFieldSpawn(position, dir);
    }

    private void FireBundleSpawn()
    {
        // TODO: 이걸 다시 체크할 필요가 있을까? 위에서 이미 해당 경우에만 패쓰하고,
        // 플레이어가 화염 폭탄꽃 직후 꽃을 바꿔서 아래 조건문이 FALSE로 나온다면 FIRE bundle이 스폰되지 않게 됨.
        if (_currSelectedFlower != (int)EFlowerType.IncendiaryFlower) return;
        
        //instantiate fire VFX
        // TODO: 이것도 여기서 또 로드하지 말고 미리 로드해놓기. 어차피 같은 prefab 오브젝트의 인스턴스를 생성하는 것 뿐임. 
        string fireVfxAddress = "Prefabs/Player/BombVFX/FireBundle";
        GameObject FireVFXObject = Utility.LoadGameObjectFromPath(fireVfxAddress);

        float dir = Mathf.Sign(_player.localScale.x);
        Vector3 playerPos = _player.position;
        Vector3 vfxPos = FireVFXObject.transform.position;
        Vector3 position = new Vector3(playerPos.x + dir * (vfxPos.x), playerPos.y + vfxPos.y, playerPos.z + vfxPos.z);

        FireVFXObject.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
        var vfx = Instantiate(FireVFXObject, position, Quaternion.identity);
        vfx.GetComponent<Fire>().Owner = this;
    }

    private void GravityFieldSpawn(Vector3 position, float dir)
    {
        // TODO: 어택인포 미리 생성해놓고 스왑
        _attackInfoInit = new AttackInfo
        {
            Damage = new DamageInfo(EDamageType.Base, 0),
            ShouldUpdateTension = false,
            GravCorePosition = position,
        };

        bombEffect = null;
        bombEffect = new StatusEffectInfo(EStatusEffect.GravityPull, 75, 5);
        _attackInfoInit.StatusEffects.Add(bombEffect);
        
        // TODO: Reset 남용하지 않기. 이니셜라이즈가 정말로 필요한가?
        Reset();

        //instantiate Gravity VFX
        // TODO: 이것도 마찬가지. 
        string GraivtyFieldVfxAddress = "Prefabs/Player/BombVFX/GravityField";
        GameObject GravityFieldVFXObject = Utility.LoadGameObjectFromPath(GraivtyFieldVfxAddress);

        GravityFieldVFXObject.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
        var vfx = Instantiate(GravityFieldVFXObject, position, Quaternion.identity);
        vfx.GetComponent<GravityField>().Owner = this;
    }

    protected override void OnAttackEnd_PreDelay()
    {
        _rigidbody2D.gravityScale = _playerController.DefaultGravityScale / Time.timeScale;
        _rigidbody2D.velocity = new Vector2(_prevVelocity.x, 0);
        _playerMovement.isAreaAttacking = false;
    }

    public void DealDamage(IDamageable target, bool isSpawn)
    {
        // TODO: 데미지를 입히는 시점에 데미지 정보를 Reassign할 필요가 있을까? 플레이어 공격력 대비 데미지라면 모르겠지만 고정 데미지라면
        // 처음 바인딩을 바꿀 때만 assign해도 충분함. 불필요한 작업임
        ReassignDamage(isSpawn);
        
        // TODO: 위에서 AttackInfoInit을 바꾸지 말고 AttackInfo만 수정. 이미 디스코드에서 다 얘기한 내용인데 대부분 안 고쳐져있음 ㅜㅜ
        // 네가 위에서 바꾼건 다 attackInfoInit인데 실제 보내는 데미지는 attackInfo임. 
        // 네 코드가 작동한 이유는 단지 네가 Reset함수를 계속 호출했고 Reset()내부에서 attackInfo = attackInfoInit.clone()을 했기 때문임
        // AttackInfoInit을 건드리지 말고 AttackInfo만 수정하거나 , attackinfo가 없다고 생각하고 attackinfoinit을 데미지로 보낼 것
        var attackToSend = _attackInfo.Clone(cloneDamage:true, cloneStatusEffect:false);
        attackToSend.Damage.TotalAmount *= _damageDealer.attackDamageMultipliers[(int)EPlayerAttackType.Area];
        Debug.Log("Damage: " + attackToSend.Damage.TotalAmount);
        _damageDealer.DealDamage(target, attackToSend);
    }

    public void PlayExplosionSound()
    {
        _damageDealer.AudioSource.PlayOneShot(explodeSounds[Random.Range(0, explodeSounds.Length)]);
    }
}
