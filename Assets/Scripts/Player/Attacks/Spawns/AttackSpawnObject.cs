using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSpawnObject : MonoBehaviour
{
    public ELegacyType attackParentType;             // The attackBase type that spawned this object
    private PlayerDamageDealer _playerDamageDealer;
    private List<IDamageable> _attackersList;
    private WaitForSeconds _dealIntervalWait;
    
    private AttackInfo _attackInfo = new AttackInfo();
    public bool IsAttachedToPlayer;
    public bool AutoDestroy = true;
    public float DealInterval = 0;

    [Space(10)] [Header("Damage")] 
    public bool HasDamage;
    [NamedArray(typeof(ELegacyPreservation))] public float[] 
        relativeDamages = new float[4];
    public bool ShouldUpdateTension;

    [Space(10)] [Header("Status Effect")] 
    public bool HasStatusEffect;
    [NamedArray(typeof(ELegacyPreservation))] public StatusEffectInfo[] 
        StatusEffectInfos = new StatusEffectInfo[4];

    private void Awake()
    {
        _dealIntervalWait = new WaitForSeconds(DealInterval);
        _attackersList = new List<IDamageable>();
        _playerDamageDealer = PlayerController.Instance.playerDamageDealer;
        var activeLegacy = _playerDamageDealer.AttackBases[(int)attackParentType].ActiveLegacy;
        EStatusEffect warriorSpecificEffect = PlayerAttackManager.Instance
            .GetWarriorStatusEffect(activeLegacy.warrior, _playerDamageDealer.GetStatusEffectLevel(activeLegacy.warrior));
        SetAttackInfo(_playerDamageDealer.AttackBases[(int)attackParentType].ActiveLegacy.preservation);
        SetWarriorSpecificStatusEffect(warriorSpecificEffect);
    }
    
    private void SetWarriorSpecificStatusEffect(EStatusEffect statusEffect)
    {
        foreach (var info in StatusEffectInfos)
        {
            info.Effect = statusEffect;
        }    
    }

    private void SetAttackInfo(ELegacyPreservation preservation)
    {
        if (HasDamage)
        {
            _attackInfo.Damage.TotalAmount = PlayerController.Instance.Strength * relativeDamages[(int)preservation];
        }
        if (HasStatusEffect)
        {
            _attackInfo.StatusEffects.Add(StatusEffectInfos[(int)preservation]);
        }
        _attackInfo.ShouldUpdateTension = ShouldUpdateTension;
        _attackInfo.SetAttackDirToMyFront(_playerDamageDealer.gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target = other.gameObject.GetComponentInParent<IDamageable>();
        if (target == null || _attackersList.Contains(target) || !HasDamage && !HasStatusEffect) return;
        _attackersList.Add(target);
        StartCoroutine(DealCoroutine(target));
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        IDamageable target = other.gameObject.GetComponent<IDamageable>();
        if (target != null)
        {
            _attackersList.Remove(target);
        }
    }

    private IEnumerator DealCoroutine(IDamageable target)
    {
        // Initial damage
        _playerDamageDealer.DealDamage(target, _attackInfo);
        if (DealInterval <= 0) yield break;
        
        // While target is not dead and is within deal area
        while (target != null && _attackersList.Contains(target))
        {
            yield return _dealIntervalWait;
            _playerDamageDealer.DealDamage(target, _attackInfo);
        }
    }
}
