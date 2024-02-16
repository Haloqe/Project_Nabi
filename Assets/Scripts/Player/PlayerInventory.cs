using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int _gold = 0;
    
    public void ChangeGoldByAmount(int amount)
    {
        _gold += amount;
        Debug.Log("Current gold: " + _gold + " (" + amount + ")");
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