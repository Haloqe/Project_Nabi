using JetBrains.Annotations;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D.IK;

//This skill has two parts of execution: (1)striking chain, (2)retrieving chain
//The enemy could therefore get hit twice by the two parts of execution. 
//And the enemy that gets hit in the first part of execution could be different from the enemy
//that gets hit in the second exectuion. 
public class PiercingChain : ActiveAbilityBase
{
    private List<int> _affectedEnemies;
    private int _maxTargetNum;
    public List<SDamage> temp;
    public SDamage specificDamage;
    public SDamageInfo halvedDamageInfo;

    protected override void Initialise()
    {
        _affectedEnemies = new List<int>();
        _maxTargetNum = 5;
        specificDamage = new SDamage();
        halvedDamageInfo = new SDamageInfo();
    }

    protected override void ActivateAbility()
    {
        _affectedEnemies.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") == false)
        {
            Debug.LogError(_data.Name_EN + " trigger with non-enemy");
            return;
        }
        if (_affectedEnemies.Count >= _maxTargetNum) return;
        if (Utility.IsObjectInList(collision.gameObject, _affectedEnemies)) return;

        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target == null) return;


        _affectedEnemies.Add(collision.gameObject.GetInstanceID());

        halvedDamageInfo = _data.DamageInfo;
        specificDamage = _data.DamageInfo.Damages[0];

        //change to half amount
        specificDamage.Amount = specificDamage.Amount / 2;
        halvedDamageInfo.Damages[0] = specificDamage;

        //first Hit - get the first half of the damage
        _owner.DealDamage(target, halvedDamageInfo);

        //After the first hit, give the enemy stun status effect
        //TO-DO: Enemybases에서 enemy가 받을 stun 정의하고 바꾸기

        //second Hit - get the second half of the damage
        _owner.DealDamage(target, halvedDamageInfo);



    }
}