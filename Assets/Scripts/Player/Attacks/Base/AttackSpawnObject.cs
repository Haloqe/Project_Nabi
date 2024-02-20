using System.Collections.Generic;
using UnityEngine;

public class AttackSpawnObject : MonoBehaviour
{
    public PlayerDamageDealer PlayerDamageDealer;
    private AttackInfo _attackInfo = new AttackInfo();
    public bool IsAttached;
    public bool ShouldManuallyDestroy;

    [Space(10)] [Header("Damage")] 
    public bool ShouldInflictDamage;
    [NamedArray(typeof(ELegacyPreservation))] public DamageInfo[] 
        DamageInfos = new DamageInfo[4];

    [Space(10)] [Header("Status Effect")] 
    public bool ShouldInflictStatusEffect;
    [NamedArray(typeof(ELegacyPreservation))] public StatusEffectInfo[] 
        StatusEffectInfos = new StatusEffectInfo[4];

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
            _attackInfo.Damages.Add(DamageInfos[(int)preservation]);
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
