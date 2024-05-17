using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int Gold { get; private set; }
    public GameObject noFlowerVFX; 

    // FlowerBomb
    private int[] _numFlowers = new int[5];
    private int _currentSelectedFlower = (int)EFlowerType.IncendiaryFlower;

    private void Awake()
    {
        // Reset gold upon restart
        GameEvents.restarted += () => ChangeGoldByAmount(-Gold);
    }
    
    public void ChangeGoldByAmount(int amount)
    {
        Gold += amount;
        PlayerEvents.goldChanged.Invoke();
    }

    public bool TryBuyItem(int price)
    {
        if (Gold >= price)
        {
            ChangeGoldByAmount(-price);
            return true;
        }
        return false;
    }

    // Store the number of flower bombs the player owns
    public void AddFlower(int flowerIndex)
    {
        _numFlowers[flowerIndex]++;
    }

    // Decrease the number of flower bombs the player owns
    public void RemoveFlower(int flowerIndex)
    {
        _numFlowers[flowerIndex]--;
    }

    // Return the number of flower bombs currently stored
    public int GetNumberOfFlowers(int flowerIndex)
    {
        return _numFlowers[flowerIndex];
    }

    //Select a certain flower ready to be used
    public void SelectFlower(int flowerIndex)
    {
        //if the number of flowers in stock is not 0, select.
        if (_numFlowers[flowerIndex] != 0)
        {
            _currentSelectedFlower = flowerIndex;
            FindObjectOfType<AttackBase_Area>().SwitchVFX();
            Debug.Log((EFlowerType)flowerIndex + " selected!");
        }
    }

    //Return the current selected flower
    public int GetCurrentSelectedFlower()
    {
        return _currentSelectedFlower;
    }
}
