using Enums.PlayerEnums;
using Player.Abilities.Base;
using System.Collections;
using UnityEngine;

namespace Player.Abilities
{
    public class TemplatePassiveAbility : PassiveAbilityBase
    {
        public override void Activate()
        {
            base.Activate();

            Debug.Log(Name_EN + " (p)");
        }
    }
}