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
    
    // SFXs
    private AudioSource _audioSource;
    [SerializeField] private AudioClip _soulShardPickupSFX;

    private void Start()
    {
        Debug.Log("PlayerInven - Start");
        _audioSource = GetComponent<AudioSource>();
        _uiManager = UIManager.Instance;
        _areaAttack = FindObjectOfType<AttackBase_Area>();
        _currentSelectedFlower = 0;
        //_uiManager.ChangeFlowerBomb(false);
        GameEvents.Restarted += OnRestarted;
        GameEvents.CombatSceneChanged += OnCombatSceneChanged;
    }

    private void OnDestroy()
    {
        if (GetComponent<PlayerController>().IsToBeDestroyed) return;
        GameEvents.Restarted -= OnRestarted;
        GameEvents.CombatSceneChanged -= OnCombatSceneChanged;
    }

    private void OnRestarted()
    {
        Gold = 0;
        _currentSelectedFlower = 0;
        for (int i = 0; i < _numFlowers.Length; i++) _numFlowers[i] = 0;
        //_uiManager.ChangeFlowerBomb(false);
    }

    private void OnCombatSceneChanged()
    {
        // ChangeGoldByAmount(0);
        // ChangeSoulShardByAmount(0);
        // SelectFlower(1);
    }

    public void ChangeGoldByAmount(int amount)
    {
        Gold += amount;
        PlayerEvents.GoldChanged.Invoke();
    }
    
    public void ChangeSoulShardByAmount(int amount)
    {
        SoulShard += amount;
        _uiManager.DisplaySoulPopUp(amount);
        _audioSource.PlayOneShot(_soulShardPickupSFX, 0.5f);
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
