using Enums.PlayerEnums;
using System.Collections;
using UnityEngine;

namespace Player.Abilities.Base
{
    public abstract class ActiveAbilityBase : AbilityBase
    {
        public new EAbilityType Type = EAbilityType.Active;

        public override void Activate()
        {
            
        }
    }
}