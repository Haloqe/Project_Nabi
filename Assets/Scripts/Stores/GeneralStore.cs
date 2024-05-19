using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// TODO GEMS !
public class GeneralStore : MonoBehaviour
{
    [SerializeField] private GameObject Stall;
    [SerializeField] private GameObject ItemUI;
    [SerializeField] private TextMeshProUGUI ItemNameText;
    [SerializeField] private TextMeshProUGUI ItemDescText;
    [SerializeField] private GameObject ItemBuyFill;
    private Image _activeItemImage;
    private Coroutine _activeFillRoutine;

    private int _minItemsToSell = 3;
    private int _maxItemsToSell = 5;
    private int _minRelicsToSell = 0;
    private int _maxRelicsToSell = 2;
    private int _activeRelicIdx = -1;

    //private List<SRelicData> _relicsToSellData;
    //private List<Object> _relicObjects;
    private Animator _UIAnimator;

    private void Start()
    {
        _UIAnimator = ItemUI.GetComponentInChildren<Animator>();
        //_relicObjects = new List<Object>();
        //InitStall();
    }

    private void InitStall()
    {
        //// Decide how many and which items to sell
        //var rand = new System.Random();
        //int itemCount = rand.Next(_minItemsToSell, _maxItemsToSell+1);
        //int relicCount = rand.Next(_minRelicsToSell+1, _maxRelicsToSell + 1);

        //// Decide on which relics to sell
        //_relicsToSellData = RelicManager.Instance.RandomChooseRelic_Store(relicCount);
        //relicCount = _relicsToSellData.Count;
        //int gemCount = itemCount - relicCount;

        //// Decide where to place items
        //Transform stallTransform = Stall.transform; 
        //float itemGap = stallTransform.localScale.x / (itemCount + 1);
        //float heightOffset = stallTransform.localScale.y / 2.0f;
        //float left = stallTransform.position.x - (stallTransform.localScale.x / 2) + itemGap;
        //float bottom = stallTransform.position.y + stallTransform.localScale.y / 2.0f;

        //// TODO gems
        //for (int i = 0; i < gemCount; i++)
        //{
        //    var item = Object.Instantiate(
        //        Utility.LoadObjectFromPath("Prefabs/Interact/Store_Gem"),
        //        new Vector3(left, bottom, 0), Quaternion.identity);
        //    left += itemGap;
        //}

        //for (int i = 0; i < relicCount; i++)
        //{
        //    var item = Object.Instantiate(
        //        Utility.LoadObjectFromPath("Prefabs/Interact/Store_Relic"), 
        //        new Vector3(left, bottom, 0), Quaternion.identity);
        //    item.GetComponent<Relic>().Init(this, i, _relicsToSellData[i]);
        //    left += itemGap;
        //    _relicObjects.Add(item);
        //}        
    }

    public void DisplayItemUI(int itemIdx)
    {
        //ItemNameText.text = _relicsToSellData[itemIdx].Name_KO;
        //ItemDescText.text = _relicsToSellData[itemIdx].Des_KO;
        //_activeItemImage = ItemBuyFill.GetComponent<Image>();
        //_activeItemImage.fillAmount = 0.0f;
        //_activeRelicIdx = itemIdx;
        //_UIAnimator.SetTrigger("ShowTrigger");
    }

    public void HideItemUI()
    {
        _activeRelicIdx = -1;
        if (_UIAnimator != null) _UIAnimator.SetTrigger("HideTrigger");
    }

    public void StartBuyInteract()
    {
        if (_activeFillRoutine == null)
            _activeFillRoutine = StartCoroutine(FillRoutine());
    }

    public void PerformBuyInteract()
    {
        //if (_activeFillRoutine != null)
        //{
        //    int activeIdx = _activeRelicIdx;
        //    PlayerAttackManager.Instance.CollectAbility(_relicsToSellData[activeIdx].AbilityId);
        //    if (_activeFillRoutine != null)
        //    {
        //        StopCoroutine(_activeFillRoutine);
        //        _activeFillRoutine = null;
        //    }
        //    Destroy(_relicObjects[activeIdx]);
        //    _relicObjects[activeIdx] = null;
        //}
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
        for (var time = 0f; time < Define.HoldInteractionTime; time += Time.unscaledDeltaTime)
        {
            _activeItemImage.fillAmount = time / Define.HoldInteractionTime;
            yield return null;
        }
    }
}