using System.Collections.Generic;
using UnityEngine;

public class Spider_Web : MonoBehaviour
{
    private static float _stunStrength = 2f;
    private static float _stunDuration = 3f;
    private static float _weaknessStrength = 2f;
    private static float _weaknessDuration = 6f;
    private static List<StatusEffectInfo> _webStatusEffectInfo = new() {
            new StatusEffectInfo(EStatusEffect.Root, _stunStrength, _stunDuration),
            new StatusEffectInfo(EStatusEffect.Weakness, _weaknessStrength, _weaknessDuration)
        };
    private AttackInfo _webAttackInfo = new(_webStatusEffectInfo);

    private void Start()
    {
        Destroy(gameObject, 6f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.gameObject.GetComponentInParent<IDamageable>();
        target?.TakeDamage(_webAttackInfo);
    }
}
