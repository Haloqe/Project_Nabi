//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.SceneManagement;

//public class Relic : InteractorHold
//{
//    private Canvas _priceCanvas;
//    private int _itemIdx;
//    private SRelicData _relicData;
//    private GeneralStore _ownerStore;

//    public void Init(GeneralStore owner, int itemIdx, SRelicData data)
//    {
//        // Set variables
//        _priceCanvas = GetComponentInChildren<Canvas>();
//        _ownerStore = owner;
//        _itemIdx = itemIdx;
//        _relicData = data;

//        // Set sprite
//        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
//        spriteRenderer.sprite = RelicManager.Instance.GetSprite(_relicData.SpriteIndex);

//        // Set price text
//        _priceCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "$" + Define.LegacyPriceByRarity[(int)_relicData.Rarity].ToString();

//        // Set position TEMP TODO
//        var halfHeight = spriteRenderer.sprite.textureRect.height / spriteRenderer.sprite.pixelsPerUnit / 2;
//        transform.position = new Vector3(transform.position.x, transform.position.y + halfHeight, transform.position.z);
//        //_priceCanvas.GetComponent<RectTransform>().position = new Vector3(0, halfHeight + 0.5f, 0);
//    }

//    protected override void OnEnterArea()
//    {
//        _ownerStore.DisplayItemUI(_itemIdx);
//    }

//    protected override void OnExitArea()
//    {
//        _ownerStore.CancelBuyInteract();
//        _ownerStore.HideItemUI();
//    }

//    protected override void OnInteractStarted(InputAction.CallbackContext obj)
//    {
//        _ownerStore.StartBuyInteract();
//    }

//    protected override void OnInteractPerformed(InputAction.CallbackContext obj)
//    {
//        _ownerStore.PerformBuyInteract();
//    }

//    protected override void OnInteractCanceled(InputAction.CallbackContext obj)
//    {
//        _ownerStore.CancelBuyInteract();
//    }
//}
