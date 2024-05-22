using UnityEngine;
using UnityEngine.EventSystems;

public class BookIcon : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        UIManager.Instance.OpenBook();
    }
}