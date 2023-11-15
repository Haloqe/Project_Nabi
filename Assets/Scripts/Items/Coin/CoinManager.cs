using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{  
    public static CoinManager instance;
    private int coins;
    [SerializeField] private TMP_Text _coinsDisplay;
    
    private void Awake()
    {
        
        if (!instance)
        {
            instance = this;
        }
        
    }
    private void OnGUI()
    {
        _coinsDisplay.text = coins.ToString();
    }

    public void ChangeCoins(int amount)
    {
        
        coins += amount;
    }

}
