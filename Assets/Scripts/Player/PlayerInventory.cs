using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // References
    private AttackBase_Area _areaAttack;
    private UIManager _uiManager;
    
    // Variables
    public int Gold { get; private set; }
    private int[] _numFlowers = new int[5];
    private int _currentSelectedFlower = 0;
    
    // VFXs
    public GameObject noFlowerVFX;

    private void Awake()
    {
        _uiManager = UIManager.Instance;
        // Reset gold upon restart
        GameEvents.restarted += () =>
        {
            ChangeGoldByAmount(-Gold);
            _currentSelectedFlower = 0;
            _uiManager.ChangeFlowerBomb(false);
        };
        _areaAttack = transform.GetComponentInChildren<AttackBase_Area>();
    }

    private void Start()
    {
        _currentSelectedFlower = 0;
        UIManager.Instance.ChangeFlowerBomb(false);
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
        
        // Instant UI update?
        if (_currentSelectedFlower == flowerIndex)
        {
            _uiManager.UpdateFlowerCount(flowerIndex);
        }
    }

    // Decrease the number of flower bombs the player owns
    public void RemoveFlower(int flowerIndex)
    {
        _numFlowers[flowerIndex]--;
        
        // Instant UI update?
        if (_currentSelectedFlower == flowerIndex)
        {
            _uiManager.UpdateFlowerCount(flowerIndex);
        }
    }

    // Return the number of flower bombs currently stored
    public int GetNumberOfFlowers(int flowerIndex)
    {
        return _numFlowers[flowerIndex];
    }

    //Select a certain flower ready to be used
    public void SelectFlower(int flowerIndex)
    {
        _currentSelectedFlower = flowerIndex;
        _areaAttack.SwitchVFX(flowerIndex);
    }

    //Return the current selected flower
    public int GetCurrentSelectedFlower()
    {
        return _currentSelectedFlower;
    }
}
