using Enums.PlayerEnums;
using System.Collections;
using UnityEngine;

namespace Player.Abilities.Base
{
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
}