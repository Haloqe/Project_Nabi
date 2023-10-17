using Enums.PlayerEnums;
using Player.Abilities.Base;
using System.Collections;
using UnityEngine;

public abstract class PassiveAbilityBase : AbilityBase
{
    private void Start()
    {
        Activate();
    }

    public override void Activate()
    {
        base.Activate();
    }
}
