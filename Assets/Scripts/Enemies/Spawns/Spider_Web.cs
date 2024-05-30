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
    private float destroyTimer = 6f;
    
    private void Start()
    {
        StartCoroutine(DestroySelf());
    }

    private void Update()
    {
        destroyTimer -= Time.deltaTime;
    }

    private IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(6f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var webStatusEffectInfo = new List<StatusEffectInfo> {
            new StatusEffectInfo(EStatusEffect.Root, _stunStrength, 
                Mathf.Min(_stunDuration, destroyTimer)),
            new StatusEffectInfo(EStatusEffect.Weakness, _weaknessStrength, 
                Mathf.Min(_weaknessDuration, destroyTimer))
        };
        
        IDamageable target = collision.gameObject.GetComponentInParent<IDamageable>();
        target?.TakeDamage(new AttackInfo(webStatusEffectInfo));
    }
}
