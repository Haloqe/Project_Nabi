using Enums.PlayerEnums;
using System.Collections;
using UnityEngine;

namespace Player.Abilities.Base
{
    public abstract class PassiveAbilityBase : AbilityBase
    {
        public new EAbilityType Type = EAbilityType.Passive;

        public override void Activate()
        {

        }
    }
}