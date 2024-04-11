using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    private int _gold = 0;

    //(Variables for FlowerBomb)
    private int[] _flowerNumbers = new int[5];
    private int _currentSelectedFlower = 1;
    [SerializeField] TextMeshProUGUI[] flowerText = new TextMeshProUGUI[5];

    public void ChangeGoldByAmount(int amount)
    {
        _gold += amount;
        PlayerEvents.goldChanged.Invoke(_gold);
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

    //Selection of Bombs
    void OnBomb0_Select(InputValue value)
    {
        SelectFlowers(0);
    }
    void OnBomb1_Select(InputValue value)
    {
        SelectFlowers(1);
    }

    void OnBomb2_Select(InputValue value)
    {
        SelectFlowers(2);
    }
    void OnBomb3_Select(InputValue value)
    {
        SelectFlowers(3);
    }
    void OnBomb4_Select(InputValue value)
    {
        SelectFlowers(4);
    }


    // Store the number of flower bombs the player owns
    public void AddToFlower(int flowerIndex)
    {
        _flowerNumbers[flowerIndex]++;
        flowerText[flowerIndex].text = _flowerNumbers[flowerIndex].ToString();
    }

    // Decrease the number of flower bombs the player owns
    public void DecreaseToFlower(int flowerIndex)
    {
        _flowerNumbers[flowerIndex]--;
        flowerText[flowerIndex].text = _flowerNumbers[flowerIndex].ToString();
    }

    // Return the number of flower bombs currently stored
    public int GetNumberOfFlowers(int flowerIndex)
    {
        return _flowerNumbers[flowerIndex];
    }

    //Select a certain flower ready to be used
    public void SelectFlowers(int flowerIndex)
    {
        //if the number of flowers in stock is not 0, select.
        if (_flowerNumbers[flowerIndex] != 0)
        {
            _currentSelectedFlower = flowerIndex;
            Debug.Log("Flower Number" + flowerIndex + "is selected!");
            FindObjectOfType<AttackBase_Area>().SwitchVFX();
        }
    }

    //Return the current selected flower
    public int GetCurrentSelectedFlower()
    {
        return _currentSelectedFlower;
    }
}
