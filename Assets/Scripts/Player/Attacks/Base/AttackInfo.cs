using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AttackInfo
{
    public DamageInfo Damage;
    public List<StatusEffectInfo> StatusEffects;
    public int IncomingDirectionX;
    public float AttackerArmourPenetration;
    public Vector3 GravCorePosition;
    // 나이트셰이드 어둠 완충 공격이 될 수 있는가?
    public bool CanBeDarkAttack;
    // 나이트셰이드 흡혈이 들어가는 공격인가?
    public bool ShouldLeech;
    // 장력 게이지가 올라가는 공격인가?
    public bool ShouldUpdateTension;
    

    public AttackInfo()
    {
        Damage = new DamageInfo();
        StatusEffects = new List<StatusEffectInfo>();
    }
    
    public AttackInfo(DamageInfo damageInfo)
    {
        Damage = damageInfo;
        StatusEffects = new List<StatusEffectInfo>();
    }

    public AttackInfo(List<StatusEffectInfo> statusEffectInfos)
    {
        Damage = new DamageInfo();
        StatusEffects = statusEffectInfos;
    }
    
    public AttackInfo(DamageInfo damageInfo, List<StatusEffectInfo> statusEffectInfos)
    {
        Damage = damageInfo;
        StatusEffects = statusEffectInfos;
    }
    
    public AttackInfo(DamageInfo damageInfo, List<StatusEffectInfo> statusEffectInfos, int dir, Vector3 position, bool canBeDarkAttack, bool shouldUpdateTension) 
        : this(damageInfo, statusEffectInfos)
    {
        GravCorePosition = position;
        IncomingDirectionX = dir;
        CanBeDarkAttack = canBeDarkAttack;
        ShouldUpdateTension = shouldUpdateTension;
    }

    // Only X is considered
    public void SetAttackDirToMyFront(GameObject attacker)
    {
        // For player, x < 0 is right and x > 0 is left
        IncomingDirectionX = (int)Mathf.Sign(attacker.transform.localScale.x); 

        // For non-player objects, it is opposite
        if (!attacker.CompareTag("Player")) IncomingDirectionX *= -1;
    }

    public AttackInfo Clone(bool cloneDamage, bool cloneStatusEffect)
    {
        var info = new AttackInfo(Damage, StatusEffects, IncomingDirectionX, GravCorePosition, CanBeDarkAttack, ShouldUpdateTension);
        if (cloneDamage && info.Damage != null)
        {
            info.Damage = info.Damage.Clone();
        }
        if (cloneStatusEffect && info.StatusEffects != null)
        {
            info.StatusEffects = GetClonedStatusEffect();
        }
        return info;
    }

    public List<StatusEffectInfo> GetClonedStatusEffect()
    {
        var clone = new List<StatusEffectInfo>();
        foreach (var t in StatusEffects)
        {
            clone.Add(t.Clone());
        }
        return clone;
    }
}

[Serializable]
public class DamageInfo
{
    public EDamageType Type;
    public float TotalAmount;
    public float Duration; // zero if one-shot damage
    public float Tick;     // the dot damage tick

    public DamageInfo() { }
    public DamageInfo(EDamageType type, float amount)
    {
        Type = type;
        TotalAmount = amount;
        Duration = 0;
        Tick = 0;
    }
    public DamageInfo(EDamageType type, float amount, float duration) : this(type, amount)
    {
        Duration = duration;
        Tick = Define.DefaultDamageTick;
    }
    public DamageInfo(EDamageType type, float amount, float duration, float interval) : this(type, amount, duration)
    {
        Tick = interval;
    }

    public DamageInfo Clone()
    {
        return new DamageInfo(Type, TotalAmount, Duration, Tick);
    }
}

[Serializable]
public class StatusEffectInfo
{
    public EStatusEffect Effect;
    public float Strength;
    public float Duration; // zero if continuous, auto-removed skill (e.g. 장판스킬)
    public float Chance;

    public StatusEffectInfo() { }
    public StatusEffectInfo(EStatusEffect type)
    {
        Effect = type;
        Strength = 1;
        Duration = 0;
        Chance = 1;
    }
    public StatusEffectInfo(EStatusEffect type, float strength) : this(type)
    {
        Strength = strength;
    }
    public StatusEffectInfo(EStatusEffect type, float strength, float duration) : this(type, strength)
    {
        Duration = duration;
    }
    public StatusEffectInfo(EStatusEffect type, float strength, float duration, float chance) : this(type, strength, duration)
    {
        Chance = chance;
    }

    public StatusEffectInfo Clone()
    {
        return new StatusEffectInfo(Effect, Strength, Duration, Chance);
    }
}