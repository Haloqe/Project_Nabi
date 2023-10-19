using UnityEngine;

public class PlayerCombat : MonoBehaviour, IDamageDealer, IDamageable
{
    // attributes
    private int health = 15;

#region Damage Dealing and Receiving
    public void DealDamage(IDamageable target, SDamageInfo damageInfo)
    {
        target.TakeDamage(AdjustOutgoingDamage(damageInfo));
    }

    public void TakeDamage(SDamageInfo damageInfo)
    {
        // TEMP CODE
        Utility.PrintDamageInfo("player", damageInfo);
        int damage = (int)IDamageable.CalculateRoughDamage(damageInfo);
        health -= damage;
        Debug.Log("Damaged by " +  damage + " | Current health: " + health);
        if (health <= 0) Die();
    }

    private SDamageInfo AdjustOutgoingDamage(SDamageInfo damageInfo)
    {
        // make changes to the damage dealt based on attributes
        return damageInfo;
    }

    private void Die()
    {
        // TEMP CODE
        Debug.Log("Player died.");
        // should save info somewhere, do progressive updates
        Destroy(gameObject);
    }
    #endregion Damage Dealing and Receiving
}
