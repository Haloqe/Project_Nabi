using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class Flower : MonoBehaviour
{
    private int _maxNumberOfFlowers = 2;
    private Sprite[] _flowerSprites;
    private SFlowerInfo _flowerInfo;
    private List<SFlowerInfo> _flowerIdentification = new List<SFlowerInfo>
        {
            new SFlowerInfo {Name = "NectarFlower", Description = "체력 회복.", SpriteIndex = 0},
            new SFlowerInfo {Name = "IncendiaryFlower", Description = "적을 불태운다.", SpriteIndex = 1},
            new SFlowerInfo {Name = "StickyFlower", Description = "슬로우 제공.", SpriteIndex = 2},
            new SFlowerInfo {Name = "BlizzardFlower", Description = "스턴 제공.", SpriteIndex = 3},
            new SFlowerInfo {Name = "GravityFlower", Description = "끌어당김.", SpriteIndex = 4}};

    public void Init(int itemIdx, SFlowerInfo info)
    {
        //
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        //What happens when the player decides to pick up the flower by pressing "C"
        if (other.CompareTag("Player"))
        {
            for(int i=0; i<5; i++)
            {
                if(gameObject.name == _flowerIdentification[i].Name)
                {
                    int check = FindObjectOfType<PlayerController>().NumberOfFlowers(_flowerIdentification[i].SpriteIndex);

                    if (check <= _maxNumberOfFlowers - 1)
                    {
                        FindObjectOfType<PlayerController>().AddToFlower(_flowerIdentification[i].SpriteIndex);
                        Destroy(gameObject);
                    }
                        
                }
            }
            
            
            //increase the number of a certain flower bomb in the UI
        }
    }
}