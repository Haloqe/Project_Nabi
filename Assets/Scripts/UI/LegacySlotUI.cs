using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LegacySlotUI : LanguageChangeHandlerBase, IPointerEnterHandler, IPointerExitHandler
{
    private EWarrior _warrior;
    private List<string> _names;
    private List<string> _descs;
    private ELegacyPreservation _preserv;
    private GameObject _descPopUp;
    private TextMeshProUGUI _descNameText;
    private TextMeshProUGUI _descDescText;
    private TextMeshProUGUI _descWarriorText;

    protected override void Awake()
    {
        base.Awake();
        _descPopUp = transform.Find("DescPopUp").gameObject;
        _descNameText = _descPopUp.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        _descDescText = _descPopUp.transform.Find("DescText").GetComponent<TextMeshProUGUI>();
        _descWarriorText = _descPopUp.transform.Find("WarriorText").GetComponent<TextMeshProUGUI>();
    }
    
    public override void UpdateText()
    {
        int localisation = (int)Define.Localisation;
        _descNameText.text = _names[localisation];
        _descDescText.text = _descs[localisation];
        _descWarriorText.text = Utility.GetColouredWarriorText(_warrior);
    }

    public void Init(EWarrior warrior, List<string> names, List<string> descs, ELegacyPreservation preservation)
    {
        _warrior = warrior;
        _names = names;
        _preserv = preservation;
        _descs = new List<string>();
        foreach (var desc in descs)
        {
            _descs.Add(desc.Replace("<br>", " "));
        }
        _descNameText.color = Define.LegacyPreservationColors[(int)_preserv];
        _descWarriorText.color = Define.WarriorMainColours[(int)_warrior];
        UpdateText();
    }

    public void OnUpdatePreservation()
    {
        _preserv++;
        _descNameText.color = Define.LegacyPreservationColors[(int)_preserv];
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_names == null) return;
        _descPopUp.SetActive(true);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_names == null) return;
        _descPopUp.SetActive(false);
    }
}