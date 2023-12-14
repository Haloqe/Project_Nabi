using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FoodStore : MonoBehaviour
{
    private List<SFoodInfo> _foodsToSell = new List<SFoodInfo> 
        {new SFoodInfo {Name = "음식 (소)", Description = "작은 음식이다.", HealthPoint = 25, Price = 100, SpriteIndex = 51}, 
        new SFoodInfo {Name = "음식 (중)", Description = "중간 음식이다.", HealthPoint = 50, Price = 200, SpriteIndex = 53}, 
        new SFoodInfo {Name = "음식 (대)", Description = "큰 음식이다.", HealthPoint = 75, Price = 300, SpriteIndex = 54}};

    private List<Object> _foodObjects;

    [SerializeField] private GameObject Stall;
    [SerializeField] private GameObject ItemUI;
    [SerializeField] private TextMeshProUGUI FoodNameText;
    [SerializeField] private TextMeshProUGUI FoodDescText;
    [SerializeField] private GameObject ItemBuyFill;

    private Image _activeItemImage;
    private Coroutine _activeFillRoutine;

    private int _numberOfFoodsToSell = 3;
    private int _activeFoodIdx = -1;

    private Animator _UIAnimator;

    void Start()
    {
        _UIAnimator = ItemUI.GetComponentInChildren<Animator>();
        _foodObjects = new List<Object>();
        InitStall();
    }

    private void InitStall()
    {
        Transform stallTransform = Stall.transform; 
        float itemGap = stallTransform.localScale.x / (_numberOfFoodsToSell + 1);
        float heightOffset = stallTransform.localScale.y / 2.0f;
        float left = stallTransform.position.x - (stallTransform.localScale.x / 2) + itemGap;
        float bottom = stallTransform.position.y + stallTransform.localScale.y / 1.5f;

        for (int i = 0; i < _numberOfFoodsToSell; i++)
        {
            var item = Object.Instantiate(
                Utility.LoadObjectFromPath("Prefabs/Interact/Store_Food"),
                new Vector3(left, bottom, 0), Quaternion.identity);
            item.GetComponent<Food>().Init(this, i, _foodsToSell[i]);
            left += itemGap;
            _foodObjects.Add(item);
        }   
    }

    public void DisplayItemUI(int itemIdx)
    {
        FoodNameText.text = _foodsToSell[itemIdx].Name;
        FoodDescText.text = _foodsToSell[itemIdx].Description;
        _activeItemImage = ItemBuyFill.GetComponent<Image>();
        _activeItemImage.fillAmount = 0.0f;
        _activeFoodIdx = itemIdx;
        _UIAnimator.SetTrigger("ShowTrigger");
    }

    public void HideItemUI()
    {
        _activeFoodIdx = -1;
        if (_UIAnimator != null) _UIAnimator.SetTrigger("HideTrigger");
    }

    public void StartBuyInteract()
    {
        if (_activeFillRoutine == null)
            _activeFillRoutine = StartCoroutine(FillRoutine());
    }

    public void PerformBuyInteract()
    {
        if (_activeFillRoutine != null)
        {
            int activeIdx = _activeFoodIdx;
            Debug.Log("Bought item");
            if (_activeFillRoutine != null)
            {
                StopCoroutine(_activeFillRoutine);
                _activeFillRoutine = null;
            }
            Destroy(_foodObjects[activeIdx]);
            _foodObjects[activeIdx] = null;
        }
    }

    public void CancelBuyInteract()
    {
        if (_activeFillRoutine != null)
        {
            StopCoroutine(_activeFillRoutine);
            _activeFillRoutine = null;
        }
        _activeItemImage.fillAmount = 0.0f;
    }

    private IEnumerator FillRoutine()
    {
        for (var time = 0f; time < Define.HoldInteractionTime; time += Time.deltaTime)
        {
            _activeItemImage.fillAmount = time / Define.HoldInteractionTime;
            yield return null;
        }
    }
}
