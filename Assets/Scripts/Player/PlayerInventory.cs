using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int _gold = 0;
    
    public void ChangeGoldByAmount(int amount)
    {
        _gold += amount;
        PlayerEvents.goldChanged.Invoke(_gold);
    }

    public bool TryBuyItem(int price)
    {
        if (_gold >= price)
        {
            ChangeGoldByAmount(-price);
            return true;
        }
        else return false;
    }
}
