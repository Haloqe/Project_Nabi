using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider_Web : MonoBehaviour
{
    private static float _stunStrength = 2f;
    private static float _stunDuration = 3f;
    private static float _weaknessStrength = 2f;
    private static float _weaknessDuration = 6f;

    private AttackInfo _webAttackInfo;

    private void Awake()
    {
        var webStatusEffectInfo = new List<StatusEffectInfo> {
            new StatusEffectInfo(EStatusEffect.Root, _stunStrength, _stunDuration),
            new StatusEffectInfo(EStatusEffect.Weakness, _weaknessStrength, _weaknessDuration)
        };
        _webAttackInfo = new AttackInfo(webStatusEffectInfo);
    }
    
    private void Start()
    {
        StartCoroutine(DestroySelf());
    }

    private IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(6f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.gameObject.GetComponentInParent<IDamageable>();
        target?.TakeDamage(_webAttackInfo);
    }
}
