using Player.Abilities.Base;
using System.Collections;
using UnityEngine;

public class TemplateActiveAbility : ActiveAbilityBase
{
    protected override void Initialise()
    {
        // Do initialisation for the binding of the ability
    }

    protected override void ActivateAbility()
    {
        // Called upon activation of the ability
        // Do initialisation of the ability
    }

    protected override void EndAbility()
    {
        // Called during the termination of the ability
        // Do some jobs after the ability is finished
    }
}
