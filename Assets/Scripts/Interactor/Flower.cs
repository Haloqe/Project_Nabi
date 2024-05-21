using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Flower : Interactor
{
    protected PlayerController _playerController;
    private int _maxNumberOfFlowers = 2;

    [SerializeField] public EFlowerType flowerType;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        int flowerIndex = (int)GetComponent<Flower>().flowerType;
        
        //NectarFlower heals immediately on collection
        if (flowerIndex == (int)EFlowerType.NectarFlower)
        {
            FindObjectOfType<PlayerController>().Heal(10);  
            Destroy(gameObject);
        }

        //add to inventory if the number of flower does not exceed 2
        else
        {
            if (FindObjectOfType<PlayerInventory>().GetNumberOfFlowers(flowerIndex) <= _maxNumberOfFlowers - 1)
            {
                FindObjectOfType<PlayerInventory>().AddFlower(flowerIndex);
                Destroy(gameObject);
            }
        }
        
    }   
}