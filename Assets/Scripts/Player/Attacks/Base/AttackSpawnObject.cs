using UnityEngine;

public class AttackSpawnObject : MonoBehaviour
{
    public ELegacyType attackParentType;    // The attackBase type that spawned this object
    private PlayerDamageDealer _playerDamageDealer;
    
    private AttackInfo _attackInfo = new AttackInfo();
    public bool IsAttached;
    public bool ShouldManuallyDestroy;

    [Space(10)] [Header("Damage")] 
    public bool ShouldInflictDamage;
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        relativeDamages = new float[4];

    [Space(10)] [Header("Status Effect")] 
    public bool ShouldInflictStatusEffect;
    [NamedArray(typeof(ELegacyPreservation))] public StatusEffectInfo[] 
        StatusEffectInfos = new StatusEffectInfo[4];

    private void Awake()
    {
        _playerDamageDealer = PlayerController.Instance.playerDamageDealer;
        var activeLegacy = _playerDamageDealer.AttackBases[(int)attackParentType].activeLegacy;
        EStatusEffect warriorSpecificEffect = PlayerAttackManager.Instance
            .GetWarriorStatusEffect(activeLegacy.warrior, _playerDamageDealer.GetStatusEffectLevel(activeLegacy.warrior));
        SetAttackInfo(_playerDamageDealer.AttackBases[(int)attackParentType].activeLegacy.preservation);
        SetStatusEffect(warriorSpecificEffect);
    }
    
    protected void SetStatusEffect(EStatusEffect statusEffect)
    {
        foreach (var info in StatusEffectInfos)
        {
            info.Effect = statusEffect;
        }    
    }

    private void SetAttackInfo(ELegacyPreservation preservation)
    {
        if (ShouldInflictDamage)
        {
            _attackInfo.Damage.TotalAmount = PlayerController.Instance.Strength * relativeDamages[(int)preservation];
        }
        if (ShouldInflictStatusEffect)
        {
            _attackInfo.StatusEffects.Add(StatusEffectInfos[(int)preservation]);
            
            // For pull effect, need to set direction
            if (StatusEffectInfos[(int)preservation].Effect == EStatusEffect.Pull)
            {
                _attackInfo.SetAttackDirToMyFront(_playerDamageDealer.gameObject);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target = other.gameObject.GetComponent<IDamageable>();
        if (target != null && (ShouldInflictDamage || ShouldInflictStatusEffect))
        {
            _playerDamageDealer.DealDamage(target, _attackInfo);
        }
    }
}
