using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class AttackInfo
{
    public DamageInfo Damage;
    public List<StatusEffectInfo> StatusEffects;
    public int IncomingDirectionX;
    public float AttackerArmourPenetration;

    public AttackInfo()
    {
        Damage = new DamageInfo();
        StatusEffects = new List<StatusEffectInfo>();
    }
    
    public AttackInfo(DamageInfo damageInfo, List<StatusEffectInfo> statusEffectInfos)
    {
        Damage = damageInfo;
        StatusEffects = statusEffectInfos;
    }
    
    public AttackInfo(DamageInfo damageInfo, List<StatusEffectInfo> statusEffectInfos, int dir) : this(damageInfo, statusEffectInfos)
    {
        IncomingDirectionX = dir;
    }

    // Only X is considered
    public void SetAttackDirToMyFront(GameObject attacker)
    {
        // For player, x < 0 is right and x > 0 is left
        IncomingDirectionX = (int)Mathf.Sign(attacker.transform.localScale.x); 

        // For non-player objects, it is opposite
        if (!attacker.CompareTag("Player")) IncomingDirectionX *= -1;
    }

    public AttackInfo Clone()
    {
        return new AttackInfo(Damage, StatusEffects, IncomingDirectionX);
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
}

[Serializable]
public class StatusEffectInfo
{
    public EStatusEffect Effect;
    public float Strength;
    public float Duration; // zero if continuous, auto-removed skill (e.g. 장판스킬)
    public float Chance;

    public StatusEffectInfo() { }
    public StatusEffectInfo(EStatusEffect type) : this()
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
}