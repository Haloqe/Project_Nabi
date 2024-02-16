using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WarriorUI : MonoBehaviour
{
    private int _numUsedPanels;
    private int _hoveredPanelIdx = -1;
    private int _selectedPanelIdx;
    private ELegacyPreservation[] _legacyPreservations;
    [SerializeField] private EWarrior _warrior;
    private String[] _legacyFullNames;
    private Color _hoveredColor;
    private Color _selectedColor;
    private SLegacyData[] _legacies;

    private GameObject _confirmPanel;
    private Outline[] _panelOutlines;
    [SerializeField] private GameObject[] _legacyPanels;
    
    public void Initialise(EWarrior warrior)
    {
        // Initialise variables
        _warrior = warrior;
        _legacyPreservations = new ELegacyPreservation[3];
        _panelOutlines = new Outline[4];
        _legacyFullNames = new string[3];
        _hoveredPanelIdx = -1;
        _confirmPanel = transform.Find("ConfirmPanel").gameObject;
        
        // Retrieve a list of legacies that are not collected yet
        var possibleLegacies = PlayerAttackManager.Instance.GetBindableLegaciesByWarrior(_warrior);
        _numUsedPanels = Mathf.Min(possibleLegacies.Count, 3);
        
        // Select random legacies to display
        _legacies = possibleLegacies.OrderBy(_ => Random.value).Take(_numUsedPanels).ToArray();
        for (int i = 0; i < _numUsedPanels; i++)
        {
            _legacyPreservations[i] = (ELegacyPreservation)Array.FindIndex
                (Define.LegacyAppearanceByPreservation, possibility => possibility >= Random.value);
            _legacyPanels[i].AddComponent<LegacyPanel>().Init(this, i);
        }
        _legacyPanels[3].AddComponent<LegacyPanel>().Init(this, 3);
        
        // Hide unused panels
        for (int i = _numUsedPanels; i < 3; i++)
        {
            _legacyPanels[i].SetActive(false);
        }

        // Fill in UI respectively
        for (int i = 0; i < _numUsedPanels; i++)
        {
            _panelOutlines[i] = _legacyPanels[i].GetComponent<Outline>();
            _legacyPanels[i].transform.Find("Icon").GetComponent<Image>().sprite =
                PlayerAttackManager.Instance.GetLegacyIcon(_legacies[i].ID);
            var nameTmp = _legacyPanels[i].transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            nameTmp.text = "[" + Define.LegacyPreservationNames[(int)Define.Localisation, (int)_legacyPreservations[i]] + "] " 
                + _legacies[i].Names[(int)Define.Localisation];
            nameTmp.color = Define.LegacyPreservationColors[(int)_legacyPreservations[i]];
            var descTmp = _legacyPanels[i].transform.Find("DescText").GetComponent<TextMeshProUGUI>();
            descTmp.text = _legacies[i].Descs[(int)Define.Localisation];

            // Save the name
            _legacyFullNames[i] = "<color=#" + ColorUtility.ToHtmlStringRGBA(Define.LegacyPreservationColors[(int)_legacyPreservations[i]])
                + ">" + nameTmp.text + "</color>";
        }
        _panelOutlines[3] = _legacyPanels[3].GetComponent<Outline>();
        
        // Initialise color
        var baseColor = _panelOutlines[0].effectColor;
        _hoveredColor = new Color(baseColor.r, baseColor.g, baseColor.b , 0.08f);
        _selectedColor = new Color(baseColor.r, baseColor.g, baseColor.b , 0.3f);
        
        // Highlight the first panel by default
        SelectPanel(0);
    }

    public void OnPointerEnterPanel(int index)
    {
        if (index == _selectedPanelIdx) return;
        _panelOutlines[index].enabled = true;
        _panelOutlines[index].effectColor = _hoveredColor;
        _hoveredPanelIdx = index;
    }
    
    public void OnPointerExitPanel(int index)
    {
        if (index == _selectedPanelIdx) return;
        _panelOutlines[index].enabled = false;
        _hoveredPanelIdx = -1;
    }

    public void OnPointerClickPanel(int panelIndex)
    {
        SelectPanel(panelIndex);
        
        // 다른 태엽 보존도 높이기 옵션
        if (panelIndex == 3)
        {
            Debug.Log("태엽 보존도 높이기: NOT IMPLEMENTED");
            UIManager.Instance.CloseFocusedUI();
            return;
        }
        
        // Legacy selected
        // Passive
        if (_legacies[panelIndex].Type == ELegacyType.Passive)
        {
            CollectLegacy(panelIndex);
            return;
        }
        
        // Active
        string boundLegacyName = PlayerAttackManager.Instance.GetBoundActiveLegacyName(_legacies[panelIndex].Type);
        if (boundLegacyName.Equals(String.Empty))
        {
            CollectLegacy(panelIndex); 
        }
        else
        {
            // TODO
            _confirmPanel.SetActive(true);
        }
    }

    private void CollectLegacy(int panelIndex)
    {
        PlayerAttackManager.Instance.CollectLegacy(_legacies[panelIndex].ID, _legacyPreservations[panelIndex]);
        UIManager.Instance.CloseFocusedUI();   
    }
    
    public void OnSubmit()
    {
        if (_confirmPanel.activeSelf)
        {
            _confirmPanel.SetActive(false);
            CollectLegacy(_selectedPanelIdx);
        }
        else
        {
            OnPointerClickPanel(_selectedPanelIdx);
        }
    }

    public void OnCancel()
    {
        if (!_confirmPanel.activeSelf) return;
        _confirmPanel.SetActive(false);
    }
    
    public void OnNavigate(Vector2 value)
    {
        if (_confirmPanel.activeSelf) return;
        switch (value.y)
        {
            // Left or Right
            case 0:
                return;
            // Up
            case > 0:
                {
                    if (_selectedPanelIdx == 0) SelectPanel(3);
                    else if (_selectedPanelIdx == 3) SelectPanel(_numUsedPanels - 1);
                    else SelectPanel(_selectedPanelIdx - 1);
                    break;
                }
            // Down
            case < 0:
                {
                    if (_selectedPanelIdx == 3) SelectPanel(0);
                    else if (_selectedPanelIdx == _numUsedPanels - 1) SelectPanel(3);
                    else SelectPanel(_selectedPanelIdx + 1);
                    break;
                }
        }
    }

    private void SelectPanel(int index)
    {
        // Clean up previous selected panel
        if (_selectedPanelIdx != -1)
        {
            if (_hoveredPanelIdx != _selectedPanelIdx)
                _panelOutlines[_selectedPanelIdx].enabled = false;
            else
                _panelOutlines[_selectedPanelIdx].effectColor = _hoveredColor;
        }
        // Update selected panel
        _selectedPanelIdx = index;
        _panelOutlines[index].enabled = true;
        _panelOutlines[index].effectColor = _selectedColor;
    }
}