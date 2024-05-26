using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider_Web : MonoBehaviour
{
    private static float _rootStrength = 2f;
    private static float _rootDuration = 3f;
    private static float _weaknessStrength = 2f;
    private static float _weaknessDuration = 6f;
    private static List<StatusEffectInfo> _webStatusEffectInfo =
        new() {
            new StatusEffectInfo(EStatusEffect.Root, _rootStrength, _rootDuration),
            new StatusEffectInfo(EStatusEffect.Weakness, _weaknessStrength, _weaknessDuration)
        };
    private AttackInfo _webAttackInfo = new(_webStatusEffectInfo);

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
