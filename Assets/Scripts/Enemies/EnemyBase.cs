using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable, IDamageDealer
{
    protected Rigidbody2D _rigidbody2D;
    protected Animator _animator;
    protected SEnemyData _data;

    //reference to other components
    private EnemyMovement _enemyMovement;

    //Status effect attributes
    [NamedArray(typeof(EStatusEffect))] public GameObject[] DebuffEffects;
    private float[] _effectRemainingTimes;
    private SortedDictionary<float, float> _slowRemainingTimes; // str,time

    //Enemy health attribute
    [SerializeField] int EnemyHealth;

    protected virtual void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _effectRemainingTimes = new float[(int)EStatusEffect.MAX];
        _slowRemainingTimes = new SortedDictionary<float, float> (
            Comparer<float>.Create(delegate (float x, float y) { return y.CompareTo(x); })
        );
        Initialise();
    }

    protected abstract void Initialise();

    //Handles the status effect imposed by the player
    private void UpdateStatusEffectTimes()
    {
        float deltaTime = Time.deltaTime;

        for (int i = 0; i < _effectRemainingTimes.Length; i++)
        {
            // slow time is handled separately
            if (i == (int)EStatusEffect.Slow) continue;

            // skip if nothing to update
            if (_effectRemainingTimes[i] == 0.0f) continue;

            // update remaining time for the specific status effect
            _effectRemainingTimes[i] -= deltaTime;
            if (_effectRemainingTimes[i] <= 0)
            {
                SetVFXActive(i, false);
                _effectRemainingTimes[i] = 0.0f;

                if (i == (int)EStatusEffect.Silence)
                {
                    IsSilenced = false;
                }
                else if (i == (int)EStatusEffect.Airborne)
                {
                    IsSilencedExceptCleanse = false;
                }
            }
        }

        //if the enemy is currently in status: "Root", "Airborne" or "Stun" then disable the enemy movement.
        if (_effectRemainingTimes[(int)EStatusEffect.Root] == 0.0f &&
           _effectRemainingTimes[(int)EStatusEffect.Airborne] == 0.0f &&
           _effectRemainingTimes[(int)EStatusEffect.Stun] == 0.0f)
        {
            _enemyMovement.EnableDisableMovement(true);
        }

        //UpdateSlowTimes();
    }
    public virtual void DealDamage(IDamageable target, SDamageInfo damageInfo)
    {
        damageInfo.DamageSource = gameObject.GetInstanceID();
        target.TakeDamage(damageInfo);
    }

    //-------------------------------------------------------------
    // Received Damage Handling
    public void TakeDamage(SDamageInfo damageInfo)
    {
        Utility.PrintDamageInfo(gameObject.name, damageInfo);
    }


}
