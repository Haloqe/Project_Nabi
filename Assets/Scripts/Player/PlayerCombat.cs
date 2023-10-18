using UnityEngine;

public class PlayerCombat : MonoBehaviour, IDamageDealer, IDamageable
{
    // attributes
    //int health = 100;

#region Damage Dealing and Receiving
    public void DealDamage(IDamageable target, SDamageInfo damageInfo)
    {
        target.TakeDamage(AdjustOutgoingDamage(damageInfo));
    }

    public void TakeDamage(SDamageInfo damageInfo)
    {
        Utility.PrintDamageInfo("player", damageInfo);
    }

    private SDamageInfo AdjustOutgoingDamage(SDamageInfo damageInfo)
    {
        // make changes to the damage dealt based on attributes
        return damageInfo;
    }
    #endregion Damage Dealing and Receiving
}
