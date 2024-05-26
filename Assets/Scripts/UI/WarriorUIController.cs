using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WarriorUIController : MonoBehaviour
{
    private PlayerController _player;
    private int _numUsedPanels;
    private int _hoveredPanelIdx = -1;
    private int _selectedPanelIdx;
    private ELegacyPreservation[] _legacyPreservations;
    [SerializeField] private EWarrior _warrior;
    private String[] _legacyFullNames;
    private Color _hoveredColor;
    private Color _selectedColor;
    private SLegacyData[] _legacies;

    private GameObject _confirmPanelObject;
    private Transform _confirmContentPanel;
    private TextMeshProUGUI _confirmOldText;
    private TextMeshProUGUI _confirmNewText;
    private Outline[] _panelOutlines;
    [SerializeField] private GameObject[] _legacyPanels;
    private int _legacyChangePrice;
    private Coroutine _activeShakeCoroutine;
    private Vector2 _confirmPanelInitialPos;
    
    public void Initialise(EWarrior warrior)
    {
        // Initialise variables
        _legacyChangePrice = 600;
        _player = PlayerController.Instance;
        _warrior = warrior;
        _legacyPreservations = new ELegacyPreservation[3];
        _panelOutlines = new Outline[4];
        _legacyFullNames = new string[3];
        _hoveredPanelIdx = -1;
        _confirmPanelObject = transform.Find("ConfirmPanel").gameObject;
        _confirmContentPanel = _confirmPanelObject.transform.Find("Panel");
        var names = _confirmContentPanel.Find("Names");
        _confirmOldText = names.Find("OldText").Find("Name").GetComponent<TextMeshProUGUI>();
        _confirmNewText = names.Find("NewText").Find("Name").GetComponent<TextMeshProUGUI>();
        _confirmPanelInitialPos = _confirmContentPanel.GetComponent<RectTransform>().anchoredPosition;
        
        // Retrieve a list of legacies that are not collected yet
        var possibleLegacies = PlayerAttackManager.Instance.GetBindableLegaciesByWarrior(_warrior);
        _numUsedPanels = Mathf.Min(possibleLegacies.Count, 3);
        
        // Select random legacies to display
        // Has meta upgrade?
        float[] legacyApperanceByPreserv = Define.LegacyAppearanceByPreservation;
        int metaLev = GameManager.Instance.PlayerMetaInfo.MetaUpgradeLevels[(int)EMetaUpgrade.BetterLegacyPreserv];
        if (metaLev != -1) legacyApperanceByPreserv = Define.MetaLegacyAppearanceByPreservation[metaLev];
        
        // Select legacies
        _legacies = possibleLegacies.OrderBy(_ => Random.value).Take(_numUsedPanels).ToArray();
        for (int i = 0; i < _numUsedPanels; i++)
        {
            _legacyPreservations[i] = (ELegacyPreservation)Array.FindIndex
                (legacyApperanceByPreserv, possibility => possibility >= Random.value);
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
        var guideText = _confirmContentPanel.Find("ChangeGuideText").GetComponent<TextMeshProUGUI>();
        guideText.text = _legacyChangePrice + guideText.text;
        
        
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
        if (string.IsNullOrEmpty(boundLegacyName))
        {
            CollectLegacy(panelIndex); 
        }
        else
        {
            DisplayConfirmPanel(panelIndex);
        }
    }

    private void DisplayConfirmPanel(int panelIndex)
    {
        // Change old and new legacy names
        ELegacyType attackType = _legacies[panelIndex].Type;
        AttackBase attackBase = _player.playerDamageDealer.AttackBases[(int)attackType];
        string oldText = Utility.GetColouredWarriorText(attackBase.ActiveLegacy.warrior) + " " +
            Utility.GetColouredPreservationText(attackBase.ActiveLegacy.preservation) + "\n" +
            PlayerAttackManager.Instance.GetBoundActiveLegacyName(attackType);
        _confirmOldText.text = oldText;
        
        string newText = Utility.GetColouredWarriorText(_legacies[panelIndex].Warrior) + " " +
            Utility.GetColouredPreservationText(_legacyPreservations[panelIndex]) + "\n" +
            _legacies[panelIndex].Names[(int)Define.Localisation];
        _confirmNewText.text = newText;
        
        // Header
        var headerTMP = _confirmContentPanel.Find("TitleText").GetComponent<TextMeshProUGUI>();
        headerTMP.text = Define.AttackTypeNames[(int)Define.Localisation, (int)attackType] +
            "이 다음과 같이 변경됩니다.";
        
        // Display UI
        _confirmPanelObject.SetActive(true);
    }
    
    private void CollectLegacy(int panelIndex)
    {
        PlayerAttackManager.Instance.CollectLegacy(_legacies[panelIndex].ID, _legacyPreservations[panelIndex]);
        UIManager.Instance.CloseFocusedUI();   
    }
    
    public void OnSubmit()
    {
        if (_confirmPanelObject.activeSelf)
        {
            var res = _player.playerInventory.TryBuyWithGold(_legacyChangePrice);
            if (!res)
            {
                if (_activeShakeCoroutine != null) StopCoroutine(_activeShakeCoroutine);
                _activeShakeCoroutine = StartCoroutine(ShakeCoroutine());
            }
            else
            {
                if (_activeShakeCoroutine != null) StopCoroutine(_activeShakeCoroutine);
                _confirmPanelObject.SetActive(false);
                CollectLegacy(_selectedPanelIdx);
            }
        }
        else
        {
            OnPointerClickPanel(_selectedPanelIdx);
        }
    }
    
    private IEnumerator ShakeCoroutine()
    {
        var rectTransform = _confirmContentPanel.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = _confirmPanelInitialPos;
        float elapsed = 0.0f;

        while (elapsed < 0.5f)
        {
            float x = Random.Range(-1f, 1f) * 3f;
            x *= 1.0f - (elapsed / 0.5f);
            rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = _confirmPanelInitialPos;
    }

    public void OnCancel()
    {
        if (!_confirmPanelObject.activeSelf) return;
        _confirmPanelObject.SetActive(false);
    }
    
    public void OnNavigate(Vector2 value)
    {
        if (_confirmPanelObject.activeSelf) return;
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
