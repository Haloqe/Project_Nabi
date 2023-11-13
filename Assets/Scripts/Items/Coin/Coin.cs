using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    //TO-DO: the coin moving up and down 
    [SerializeField] private int value;
    private bool hasTriggered;

    private CoinManager _coinManager;
    public void Start()
    {
        _coinManager = CoinManager.instance;
    }
    public void Update()
    { 
        transform.Rotate(0, 50.0f * Time.deltaTime, 0);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && !hasTriggered)
        {
            hasTriggered = true;
            _coinManager.ChangeCoins(value);
            Destroy(gameObject);
            CoinGainEffect();
        }
        
    }

    

    //TO-DO: Coin gain Effect()
    public void CoinGainEffect()
    {
        
    }
}
