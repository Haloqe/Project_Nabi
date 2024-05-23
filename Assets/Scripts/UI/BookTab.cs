using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BookTab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private BookUIController _baseUI;
    private Image _image;
    private int _tabIdx;
    public Sprite selectedSprite;
    private bool _isPointerOver;
    public bool _isActiveTab;
    
    public void Init(BookUIController baseUI, int tabIdx)
    {
        _baseUI = baseUI;
        _tabIdx = tabIdx;
        _image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        _isPointerOver = false;
        _isActiveTab = false;
    }

    private void LateUpdate()
    {
        if (_isPointerOver || _isActiveTab) _image.sprite = selectedSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerOver = true;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        _isPointerOver = true;
        _baseUI.OnPointerClickTab(_tabIdx);
    }
}
