using UnityEngine.InputSystem;

public class Flower : Interactor
{
    private int _maxNumberOfFlowers = 2;
    public EFlowerType flowerType;

    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        var playerController = PlayerController.Instance;
        int flowerIndex = (int)GetComponent<Flower>().flowerType;
        
        //NectarFlower heals immediately on collection
        if (flowerIndex == (int)EFlowerType.NectarFlower)
        {
            playerController.Heal(25);  
            Destroy(gameObject);
        }
        //add to inventory if the number of flower does not exceed 2
        else
        {
            if (playerController.playerInventory.GetNumberOfFlowers(flowerIndex) <= _maxNumberOfFlowers - 1)
            {
                playerController.playerInventory.AddFlower(flowerIndex);
                Destroy(gameObject);
            }
        }
    }   
}
