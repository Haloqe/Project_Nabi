using System.Collections;
using UnityEngine;

public class AttackBase_Melee : AttackBase
{
    // VFX
    [SerializeField] private GameObject _vfxObjBase;
    [SerializeField] private GameObject _vfxObjCombo;

    public Legacy_Melee ActiveLegacy;
    private float animLength;

    // Combo attack
    public SDamageInfo ComboDamage_Init;
    public SDamageInfo ComboDamage;
    private float _comboTimeLimit = 1.3f;
    private float _comboTimer;
    private int _comboStack = 0;
    private float _comboDelay;
    private float _baseDelay;

    public override void Initialise()
    {
        base.Initialise();
        _baseDelay = 0.0f;
    }

    public override void Reset()
    {
        base.Reset();
        _comboDelay = 0.2f;
        _comboTimer = 0.0f;
        _comboStack = 0;
        ActiveLegacy = null;
        ComboDamage = ComboDamage_Init;
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
            _vfxObjBase.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
            _vfxObjCombo.SetActive(true);
            Debug.Log("Combo");
        }
        // Base Attack
        else
        {
            _animator.SetBool("IsMeleeCombo", false);
            _vfxObjBase.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
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
}