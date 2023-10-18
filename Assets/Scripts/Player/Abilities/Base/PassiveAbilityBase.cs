using Player.Abilities.Base;
using System.Collections;
using UnityEngine;

public abstract class PassiveAbilityBase : AbilityBase
{
    protected override void Start()
    {
        Activate();
    }

    public override void Activate()
    {
        
    }
}
