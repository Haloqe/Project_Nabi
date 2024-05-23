using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private MainMenuUIController _baseUI;
    private int _optionIdx;
    
    public void Init(MainMenuUIController baseUI, int optionIdx)
    {
        _baseUI = baseUI;
        _optionIdx = optionIdx;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _baseUI.OnPointerEnter(_optionIdx);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        _baseUI.OnPointerClick();
    }
}