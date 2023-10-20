using Player.Abilities.Base;
using System.Collections;
using UnityEngine;

public class PerfectPurification : ActiveAbilityBase
{
    // remove all debuffs
    // increase movement speed by 1.3 for 1.5 seconds
    protected override void ActivateAbility()
    {
        _owner.RemoveAllDebuffs();
        FindObjectOfType<PlayerMovement>().SetMoveSpeedForDuration(3f, 1.5f);
    }
}
