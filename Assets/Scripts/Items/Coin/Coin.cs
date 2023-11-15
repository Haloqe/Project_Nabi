using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private bool hasTriggered;
    TestEnemyDropGold testEnemyDropGold;
    private CoinManager _coinManager;

    public static event Action OnCoinCollected;
    public void Start()
    {
        testEnemyDropGold = FindObjectOfType<TestEnemyDropGold>();
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
            _coinManager.ChangeCoins(CoinValue());
            Destroy(gameObject);
            CoinGainEffect();
        }
    }

    private int CoinValue()
    {
        int value = UnityEngine.Random.Range(testEnemyDropGold.MinCoinRange(), testEnemyDropGold.MaxCoinRange());
        return value;
    }
    
    //TO-DO: Coin gain Effect()
    public void CoinGainEffect()
    {
        
    }
}
