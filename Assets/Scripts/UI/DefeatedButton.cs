using UnityEngine;
using UnityEngine.EventSystems;

public class DefeatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private DefeatedUIController _baseUI;
    private bool _isRestartBtn;
    
    public void Init(DefeatedUIController baseUI, bool isRestartBtn)
    {
        _baseUI = baseUI;
        _isRestartBtn = isRestartBtn;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse enter");
        _baseUI.OnPointerEnter(_isRestartBtn);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse exit");
        _baseUI.OnPointerExit(_isRestartBtn);
    }
}