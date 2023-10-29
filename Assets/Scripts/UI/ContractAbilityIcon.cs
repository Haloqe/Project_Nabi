using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContractAbilityIcon : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private int index = 0;
    private MetalContractUI _baseUI;

    private void Start()
    {
        _baseUI = FindObjectOfType<MetalContractUI>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        _baseUI.OnMouseEnterIcon(index);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _baseUI.OnMouseClickIcon(index);
    }
}