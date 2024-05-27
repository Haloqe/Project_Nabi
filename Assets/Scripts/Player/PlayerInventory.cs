using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // References
    private AttackBase_Area _areaAttack;
    private UIManager _uiManager;
    
    // Variables
    public int Gold { get; private set; }
    public int SoulShard { get; private set; }
    private int[] _numFlowers = new int[5];
    private int _currentSelectedFlower = 0;
    
    // VFXs
    public GameObject noFlowerVFX;

    private void Start()
    {
        _uiManager = UIManager.Instance;
        _areaAttack = FindObjectOfType<AttackBase_Area>();
        _currentSelectedFlower = 0;
        _uiManager.ChangeFlowerBomb(false);
        GameEvents.Restarted += OnRestarted;
    }

    private void OnDestroy()
    {
        GameEvents.Restarted -= OnRestarted;
    }

    private void OnRestarted()
    {
        Gold = 0;
        _currentSelectedFlower = 0;
        for (int i = 0; i < _numFlowers.Length; i++) _numFlowers[i] = 0;
        _uiManager.ChangeFlowerBomb(false);
    }

    public void ChangeGoldByAmount(int amount)
    {
        Gold += amount;
        PlayerEvents.GoldChanged.Invoke();
    }
    
    public void ChangeSoulShardByAmount(int amount)
    {
        SoulShard += amount;
        PlayerEvents.SoulShardChanged.Invoke();
    }

    public bool TryBuyWithGold(int price)
    {
        if (Gold < price) return false;
        ChangeGoldByAmount(-price);
        _uiManager.DisplayGoldPopUp(-price);
        return true;
    }

    public bool TryBuyWithSoulShard(int price)
    {
        if (SoulShard < price) return false;
        ChangeSoulShardByAmount(-price);
        _uiManager.DisplaySoulPopUp(-price);
        return true;
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
        _areaAttack.UpdateVFX(flowerIndex);
    }

    //Return the current selected flower
    public int GetCurrentSelectedFlower()
    {
        return _currentSelectedFlower;
    }
}
