using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Food : InteractorHold
{
    private Sprite[] _foodSprites;
    private Canvas _priceCanvas;
    private int _itemIdx;
    private FoodStore.SFoodInfo _foodInfo;
    private FoodStore _ownerStore;
    
    public void Init(FoodStore owner, int itemIdx, FoodStore.SFoodInfo info)
    {
        //set variables
        _priceCanvas = GetComponentInChildren<Canvas>();
        _ownerStore = owner;
        _itemIdx = itemIdx;
        _foodInfo = info; 

        //set sprite
        _foodSprites = Resources.LoadAll<Sprite>("Sprites/Food/FoodTempSpriteSheet");
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = _foodSprites[_foodInfo.SpriteIndex];

        //set price text
        _priceCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "$" + _foodInfo.Price.ToString();

        // Set position TEMP TODO
        var halfHeight = spriteRenderer.sprite.textureRect.height / spriteRenderer.sprite.pixelsPerUnit / 2;
        transform.position = new Vector3(transform.position.x, transform.position.y + halfHeight, transform.position.z);
    }


    protected override void OnEnterArea()
    {
        _ownerStore.DisplayItemUI(_itemIdx);
    }

    protected override void OnExitArea()
    {
        _ownerStore.CancelBuyInteract();
        _ownerStore.HideItemUI();
    }

    protected override void OnInteractStarted(InputAction.CallbackContext obj)
    {
        _ownerStore.StartBuyInteract();
    }

    protected override void OnInteractPerformed(InputAction.CallbackContext obj)
    {
        _ownerStore.PerformBuyInteract();
    }

    protected override void OnInteractCanceled(InputAction.CallbackContext obj)
    {
        _ownerStore.CancelBuyInteract();
    }
}