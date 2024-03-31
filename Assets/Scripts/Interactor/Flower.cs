using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Flower : Interactor
{
    private int _maxNumberOfFlowers = 2;

    [SerializeField] public EFlowerType flowerType;

    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        int flowerIndex = (int)GetComponent<Flower>().flowerType;

        if (FindObjectOfType<PlayerInventory>().GetNumberOfFlowers(flowerIndex) <= _maxNumberOfFlowers - 1)
        {
            FindObjectOfType<PlayerInventory>().AddToFlower(flowerIndex);
            Destroy(gameObject);
        }
    }   
}