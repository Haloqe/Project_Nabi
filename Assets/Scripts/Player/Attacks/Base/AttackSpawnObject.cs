using UnityEngine;

public class AttackSpawnObject : MonoBehaviour
{
    private float _attackerStrength;
    public PlayerDamageDealer PlayerDamageDealer;
    private AttackInfo _attackInfo = new AttackInfo();
    public bool IsAttached;
    public bool ShouldManuallyDestroy;

    [Space(10)] [Header("Damage")] 
    public bool ShouldInflictDamage;
    [NamedArray(typeof(ELegacyPreservation))] public SDamageInfo[] 
        DamageInfo = new SDamageInfo[4];

    [Space(10)] [Header("Status Effect")] 
    public bool ShouldInflictStatusEffect;
    [NamedArray(typeof(ELegacyPreservation))] public StatusEffectInfo[] 
        StatusEffectInfos = new StatusEffectInfo[4];

    private void Awake()
    {
        _attackerStrength = PlayerController.Instance.Strength;
    }
    
    public void SetStatusEffect(EStatusEffect statusEffect)
    {
        foreach (var info in StatusEffectInfos)
        {
            info.Effect = statusEffect;
        }    
    }
    
    public void SetAttackInfo(ELegacyPreservation preservation)
    {
        if (ShouldInflictDamage)
        {
            _attackInfo.Damage.TotalAmount = DamageInfo[(int)preservation].BaseDamage + _attackerStrength * DamageInfo[(int)preservation].RelativeDamage;
        }
        if (ShouldInflictStatusEffect)
        {
            _attackInfo.StatusEffects.Add(StatusEffectInfos[(int)preservation]);
            
            // For pull effect, need to set direction
            if (StatusEffectInfos[(int)preservation].Effect == EStatusEffect.Pull)
            {
                _attackInfo.SetAttackDirToMyFront(PlayerDamageDealer.gameObject);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target = other.gameObject.GetComponent<IDamageable>();
        if (target != null && (ShouldInflictDamage || ShouldInflictStatusEffect))
        {
            PlayerDamageDealer.DealDamage(target, _attackInfo);
        }
    }
}
