using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LegacyPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private int _index = 0;
    private WarriorUI _baseUI;

    public void Init(WarriorUI baseUI, int index)
    {
        _index = index;
        _baseUI = baseUI;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _baseUI.OnPointerEnterPanel(_index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _baseUI.OnPointerExitPanel(_index);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        _baseUI.OnPointerClickPanel(_index);
    }
}