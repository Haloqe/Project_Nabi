using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorpion_Electricity : MonoBehaviour
{
    private bool _playerIsInElectricity;
    
    private static float _baseDamage = 1f;
    private static DamageInfo _electricityDamageInfo = new(EDamageType.Base, _baseDamage);
    private AttackInfo _electricityAttackInfo = new(_electricityDamageInfo);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.gameObject.GetComponentInParent<IDamageable>();
        _playerIsInElectricity = true;
        
        if (target != null) StartCoroutine(GiveElectricityDamage(target));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IDamageable target = collision.gameObject.GetComponentInParent<IDamageable>();
        if (target != null) _playerIsInElectricity = false;
    }

    private IEnumerator GiveElectricityDamage(IDamageable player)
    {
        while (_playerIsInElectricity)
        {
            player.TakeDamage(_electricityAttackInfo);
            yield return new WaitForSeconds(0.5f);
        }
    }
}