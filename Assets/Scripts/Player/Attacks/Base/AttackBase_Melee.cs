using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AttackBase_Melee : AttackBase
{
    // VFX
    [SerializeField] private GameObject _vfxObjBase;
    [SerializeField] private GameObject _vfxObjCombo;

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

    public override void Initialise()
    {
        base.Initialise();
        _baseDelay = 0.0f;
        _colliderBase = _vfxObjBase.GetComponent<BoxCollider2D>();
        _colliderCombo = _vfxObjCombo.GetComponent<BoxCollider2D>();
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
    }

    public override void Reset()
    {
        base.Reset();
        _comboDelay = 0.2f;
        _comboTimer = 0.0f;
        _comboStack = 0;
        ActiveLegacy = null;
        _damageBase = _damageInitBase;
        _damageCombo = _damageInitCombo;
        _affectedEnemies.Clear();
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
            if (dir < 0)
                _vfxObjCombo.GetComponent<ParticleSystemRenderer>().flip = Vector3.right;
            else
                _vfxObjCombo.GetComponent<ParticleSystemRenderer>().flip = Vector3.zero;
            _vfxObjCombo.SetActive(true);
            Debug.Log("Combo");
        }
        // Base Attack
        else
        {
            _animator.SetBool("IsMeleeCombo", false);
            if (dir < 0)
                _vfxObjBase.GetComponent<ParticleSystemRenderer>().flip = Vector3.right;
            else
                _vfxObjBase.GetComponent<ParticleSystemRenderer>().flip = Vector3.zero;
            _attackPostDelay = _baseDelay;
            _vfxObjBase.SetActive(true);
            Debug.Log("Base " + _comboStack);
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