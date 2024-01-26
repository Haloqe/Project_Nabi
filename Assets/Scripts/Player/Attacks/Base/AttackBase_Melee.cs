using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class AttackBase_Melee : AttackBase
{
    // VFX
    public GameObject VFXObjCombo;
    private Material _defaultVFXComboMaterial;

    // Collider
    private List<int> _affectedEnemies;
    private Collider2D _colliderBase;
    private Collider2D _colliderCombo;

    public Legacy_Melee ActiveLegacy;
    private float animLength;

    // Combo attack
    private float _comboTimeLimit = 1.3f;
    private float _comboTimer;
    private int _comboStack = 0;
    private float _comboDelay;
    private float _baseDelay;

    // Damage
    private SDamageInfo _damageCombo;
    private SDamageInfo _damageInitCombo;
    
    // Legacy
    private ELegacyPreservation _activeLegacyPreservation;

    public override void Start()
    {
        _baseDelay = 0.0f;
        _defaultVFXComboMaterial = VFXObjCombo.GetComponent<ParticleSystemRenderer>().sharedMaterial;
        _colliderBase = VFXObject.GetComponent<BoxCollider2D>();
        _colliderCombo = VFXObjCombo.GetComponent<BoxCollider2D>();
        _affectedEnemies = new List<int>();
        _damageInitBase = new SDamageInfo
        {
            Damages = new List<SDamage> { new SDamage(EDamageType.Base, 5) },
            StatusEffects = new List<SStatusEffect>(),
        };
        _damageInitCombo = new SDamageInfo
        {
            Damages = new List<SDamage> { new SDamage(EDamageType.Base, 10) },
            StatusEffects = new List<SStatusEffect>(),
        };
        base.Start();
    }

    public override void Reset()
    {
        base.Reset();
        
        // Combo
        _comboDelay = 0;// .2f;
        _comboTimer = 0.0f;
        _comboStack = 0;
        
        // Damage
        ActiveLegacy = null;
        _damageBase = _damageInitBase;
        _damageCombo = _damageInitCombo;
        _affectedEnemies.Clear();

        ResetVFXs();
    }

    public override void ResetVFXs()
    {
        base.ResetVFXs();
        VFXObjCombo.GetComponent<ParticleSystemRenderer>().sharedMaterial = _defaultVFXComboMaterial;
    }

    private void Update()
    {
        // Return if not attacking
        if (_comboStack == 0 || _comboStack == 3) return;

        // Update combo timer if attacking
        _comboTimer += Time.deltaTime;
        // Reset combo stack if time limit reached
        if (_comboTimer > _comboTimeLimit)
        {
            _comboStack = 0;
            _comboTimer = 0.0f;
        }        
    }

    public override void Attack()
    {
        // Add combo stack
        _comboStack++;
        _comboTimer = 0.0f;

        // Dir: Positive if left, negative if right
        float dir = Mathf.Sign(gameObject.transform.localScale.x);

        // Combo Attack
        if (_comboStack == 3)
        {
            _animator.SetBool("IsMeleeCombo", true);
            _attackPostDelay = _comboDelay;
            _comboStack = 0; // Reset combo stack
            VFXObjCombo.GetComponent<ParticleSystemRenderer>().flip = (dir < 0 ? Vector3.right : Vector3.zero);
            VFXObjCombo.SetActive(true);
        }
        // Base Attack
        else
        {
            _animator.SetBool("IsMeleeCombo", false);
            _attackPostDelay = _baseDelay;
            VFXObject.GetComponent<ParticleSystemRenderer>().flip = (dir < 0 ? Vector3.right : Vector3.zero);
            VFXObject.SetActive(true);
        }      

        _animator.SetInteger("AttackIndex", (int)_attackType);
    }

    public void OnComboHit()
    {
        FindObjectOfType<TestCameraShake>().OnComboAttack();
    }

    public void ActivateCollider()
    {
        // Combo Attack
        if (_comboStack == 0)
        {
            _colliderCombo.enabled = true;
        }
        // Base Attack
        else
        {
            _colliderBase.enabled = true;
        }
    }

    public void DeactivateCollider()
    {
        // Combo Attack
        if (_comboStack == 0)
        {
            _colliderCombo.enabled = false;
        }
        // Base Attack
        else
        {
            _colliderBase.enabled = false;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") == false) return;
        if (Utility.IsObjectInList(collision.gameObject, _affectedEnemies)) return;

        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target == null) return;

        _affectedEnemies.Add(collision.gameObject.GetInstanceID());
        _damageDealer.DealDamage(target, _comboStack == 0 ? _damageCombo : _damageBase);
    }

    protected override void OnAttackEnd_PreDelay()
    {
        base.OnAttackEnd_PreDelay();
        _affectedEnemies.Clear();
    }
}