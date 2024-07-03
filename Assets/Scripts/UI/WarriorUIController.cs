using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WarriorUIController : UIControllerBase
{
    // Base
    private PlayerAttackManager _playerAttackManager;
    private PlayerController _player;
    
    [SerializeField] private EWarrior _warrior;
    [SerializeField] private GameObject[] _legacyPanels;
    private int _numUsedPanels;
    private int _hoveredPanelIdx = -1;
    private int _selectedPanelIdx;
    private ELegacyPreservation[] _legacyPreservations;
    private Outline[] _panelOutlines;
    private String[] _legacyFullNames;
    private Color _hoveredColor;
    private Color _selectedColor;
    private SLegacyData[] _legacies;

    // Confirm
    private GameObject _confirmPanelObject;
    private Transform _confirmContentPanel;
    private TextMeshProUGUI _confirmOldText;
    private TextMeshProUGUI _confirmNewText;
    private int _legacyChangePrice;
    private Coroutine _activeShakeCoroutine;
    private RectTransform _confirmRect;
    private Vector2 _confirmRectInitialPos;
    
    // Legacy Upgrade
    private Vector2 _upgradeRectInitialPos;
    private ELegacyPreservation _highestAppearedPreserv;
    private GameObject _legacyUpgradeCanvas;
    private BookLegacyPage _legacyUpgradeController;
    private GameObject _upgradeFailCanvas;
    private GameObject _upgradeSucceedCanvas;
    private float[][] _upgradeSuccessChances;
    private RectTransform _upgradeRect;
    
    // SFX
    private AudioSource _audioSource;
    [SerializeField] private AudioClip clockworkCollectSound;
    [SerializeField] private AudioClip clockworkBindSound;
    
    public void Initialise(EWarrior warrior, bool isPristineClockwork)
    {
        _playerAttackManager = PlayerAttackManager.Instance;
        _player = PlayerController.Instance;
        _warrior = warrior;
        
        // Play sound
        _audioSource = GetComponent<AudioSource>();
        _audioSource.PlayOneShot(clockworkCollectSound);
        
        // Initialise UI
        InitialiseConfirmUI();
        InitialiseBaseUI(isPristineClockwork);
        InitialiseUpgradeUI();
        
        // Highlight the first panel by default
        SelectPanel(0);
    }
    
    private void InitialiseBaseUI(bool isPristineClockwork)
    {
        // Initialise variables
        _legacyChangePrice = 600;
        _legacyPreservations = new ELegacyPreservation[3];
        _panelOutlines = new Outline[4];
        _legacyFullNames = new string[3];
        _hoveredPanelIdx = -1;

        // Retrieve a list of legacies that are not collected yet
        var possibleLegacies = _playerAttackManager.GetBindableLegaciesByWarrior(_warrior);
        _numUsedPanels = Mathf.Min(possibleLegacies.Count, 3);

        // Select random legacies to display
        // Has meta upgrade?
        float[] legacyApperanceByPreserv = Define.LegacyAppearanceByPreservation;
        int metaLev = GameManager.Instance.PlayerMetaData.metaUpgradeLevels[(int)EMetaUpgrade.BetterLegacyPreserv];
        if (metaLev != -1) legacyApperanceByPreserv = Define.MetaLegacyAppearanceByPreservation[metaLev];

        // Select legacies
        _legacies = possibleLegacies.OrderBy(_ => Random.value).Take(_numUsedPanels).ToArray();
        for (int i = 0; i < _numUsedPanels; i++)
        {
            if (isPristineClockwork)
            {
                _legacyPreservations[i] = ELegacyPreservation.Pristine;
            }
            else
            {
                _legacyPreservations[i] = (ELegacyPreservation)Array.FindIndex
                    (legacyApperanceByPreserv, possibility => possibility >= Random.value);
            }
            _legacyPanels[i].AddComponent<LegacyPanel>().Init(this, i);
        }
        _legacyPanels[3].AddComponent<LegacyPanel>().Init(this, 3);
        _highestAppearedPreserv = _legacyPreservations.Max();

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
                _playerAttackManager.GetLegacyIcon(_legacies[i].ID);
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
        _hoveredColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.15f);
        _selectedColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.65f);
    }

    private void InitialiseUpgradeUI()
    {
        _legacyUpgradeCanvas = transform.Find("LegacyUpgradeCanvas").gameObject;
        _legacyUpgradeController = _legacyUpgradeCanvas.GetComponentInChildren<BookLegacyPage>();
        _upgradeFailCanvas = _legacyUpgradeCanvas.transform.Find("UpgradeFail").gameObject;
        _upgradeSucceedCanvas = _legacyUpgradeCanvas.transform.Find("UpgradeSucceed").gameObject;
        _upgradeRect = _legacyUpgradeCanvas.transform.Find("Content").GetComponent<RectTransform>();
        _upgradeRectInitialPos = _upgradeRect.anchoredPosition;
        
        _upgradeSuccessChances = new float[][]
        {
            new [] {0.50f, 0.15f, 0.00f},
            new [] {0.75f, 0.25f, 0.05f},
            new [] {0.90f, 0.35f, 0.15f},
            new [] {1.00f, 0.50f, 0.30f},
        };
        _legacyUpgradeController.Init(null);
        _legacyUpgradeController.OnBookOpen();
        _legacyUpgradeCanvas.transform.Find("Content").Find("Page_1").Find("Desc").GetComponentInChildren<TextMeshProUGUI>().text
            = $"{Utility.GetColouredPreservationText(_highestAppearedPreserv)} 태엽을 제물로 바쳐 이미 보유한 유산의 보존도를 한 단계 높입니다." +
            $"\n성공 확률: [닳은] {Utility.FormatPercentage(_upgradeSuccessChances[(int)_highestAppearedPreserv][0])}% " +
            $"[빛바랜] {Utility.FormatPercentage(_upgradeSuccessChances[(int)_highestAppearedPreserv][1])}% " +
            $"[온전한] {Utility.FormatPercentage(_upgradeSuccessChances[(int)_highestAppearedPreserv][2])}%";
    }

    private void InitialiseConfirmUI()
    {
        _confirmPanelObject = transform.Find("ConfirmPanel").gameObject;
        _confirmContentPanel = _confirmPanelObject.transform.Find("Panel");
        var names = _confirmContentPanel.Find("Names");
        _confirmOldText = names.Find("OldText").Find("Name").GetComponent<TextMeshProUGUI>();
        _confirmNewText = names.Find("NewText").Find("Name").GetComponent<TextMeshProUGUI>();
        _confirmRect = _confirmContentPanel.GetComponent<RectTransform>();
        _confirmRectInitialPos = _confirmRect.anchoredPosition;
    }

    public void OnPointerEnterPanel(int index)
    {
        if (_legacyUpgradeCanvas.activeSelf || _confirmPanelObject.activeSelf) return;
        if (index == _selectedPanelIdx) return;
        _panelOutlines[index].enabled = true;
        _panelOutlines[index].effectColor = _hoveredColor;
        _hoveredPanelIdx = index;
    }
    
    public void OnPointerExitPanel(int index)
    {
        if (_legacyUpgradeCanvas.activeSelf || _confirmPanelObject.activeSelf) return;
        if (index == _selectedPanelIdx) return;
        _panelOutlines[index].enabled = false;
        _hoveredPanelIdx = -1;
    }

    public void OnPointerClickPanel(int panelIndex)
    {
        if (_legacyUpgradeCanvas.activeSelf || _confirmPanelObject.activeSelf) return;
        SelectPanel(panelIndex);
        
        // 다른 태엽 보존도 높이기 옵션
        if (panelIndex == 3)
        {
            _legacyUpgradeController.OnPageOpen();
            _legacyUpgradeCanvas.SetActive(true);
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
        string boundLegacyName = _playerAttackManager.GetBoundActiveLegacyName(_legacies[panelIndex].Type);
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
            _playerAttackManager.GetBoundActiveLegacyName(attackType);
        _confirmOldText.text = oldText;
        
        string newText = Utility.GetColouredWarriorText(_legacies[panelIndex].Warrior) + " " +
            Utility.GetColouredPreservationText(_legacyPreservations[panelIndex]) + "\n" +
            _legacies[panelIndex].Names[(int)Define.Localisation];
        _confirmNewText.text = newText;
        
        // Header
        var headerTMP = _confirmContentPanel.Find("TitleText").GetComponent<TextMeshProUGUI>();
        headerTMP.text = Define.AttackTypeNames[(int)Define.Localisation, (int)attackType] + "이 다음과 같이 변경됩니다.";
        
        // Display UI
        _confirmPanelObject.SetActive(true);
    }
    
    private void CollectLegacy(int panelIndex)
    {
        AudioManager.Instance.PlayOneShotClip(clockworkBindSound, 1f, EAudioType.Others);
        _playerAttackManager.CollectLegacy(_legacies[panelIndex].ID, _legacyPreservations[panelIndex]);
        UIManager.Instance.CloseFocusedUI();   
    }
    
    public override void OnSubmit()
    {
        if (_upgradeFailCanvas.activeSelf || _upgradeSucceedCanvas.activeSelf)
        {
            return;
        }
        if (_legacyUpgradeCanvas.activeSelf)
        {
            TryUpgradeLegacy();
        }
        else if (_confirmPanelObject.activeSelf)
        {
            var res = _player.playerInventory.TryBuyWithGold(_legacyChangePrice);
            if (!res)
            {
                // Not enough gold to change legacy
                if (_activeShakeCoroutine != null) StopCoroutine(_activeShakeCoroutine);
                _activeShakeCoroutine = StartCoroutine(ShakeCoroutine());
            }
            else
            {
                // Change legacy
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
    public override void OnClose()
    {
        OnCancel();
    }
    
    public override void OnTab()
    {
        return;
    }

    private void TryUpgradeLegacy()
    {
        int id = _legacyUpgradeController.GetSelectedLegacyID();
        if (id == -1 || _playerAttackManager.GetLegacyPreservation(id) == ELegacyPreservation.Pristine)
        {
            if (_activeShakeCoroutine != null) StopCoroutine(_activeShakeCoroutine);
            _activeShakeCoroutine = StartCoroutine(ShakeCoroutine());
        }
        else
        {
            if (_activeShakeCoroutine != null) StopCoroutine(_activeShakeCoroutine);
            
            // Try upgrade
            int targetPreserv = (int)_playerAttackManager.GetLegacyPreservation(id);
            if (Random.value <= _upgradeSuccessChances[(int)_highestAppearedPreserv][targetPreserv])
            {
                // Upgrade success
                _upgradeSucceedCanvas.transform.Find("DetailText").GetComponent<TextMeshProUGUI>().text =
                    $"{Utility.GetColouredPreservationText((ELegacyPreservation)targetPreserv + 1)} {_playerAttackManager.GetLegacyName(id)} 획득";
                _upgradeSucceedCanvas.SetActive(true);
                _playerAttackManager.UpdateLegacyPreservation(id);
            }
            else
            {
                // Upgrade fail
                _upgradeFailCanvas.SetActive(true);
            }
            _legacyUpgradeCanvas.GetComponent<Animator>().SetTrigger("Show");
            StartCoroutine(UpgradeResultDelayCoroutine());
        }
    }

    private IEnumerator UpgradeResultDelayCoroutine()
    {
        yield return new WaitForSecondsRealtime(1.8f);
        UIManager.Instance.CloseFocusedUI();
    }
    
    private IEnumerator ShakeCoroutine()
    {
        bool confirmPanelShake = _confirmPanelObject.activeSelf; 
        RectTransform rect = confirmPanelShake ? _confirmRect : _upgradeRect;
        rect.anchoredPosition = confirmPanelShake ? _confirmRectInitialPos : _upgradeRectInitialPos;
        float elapsed = 0.0f;

        while (elapsed < 0.5f)
        {
            float x = Random.Range(-1f, 1f) * 3f;
            x *= 1.0f - (elapsed / 0.5f);
            rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = confirmPanelShake ? _confirmRectInitialPos : _upgradeRectInitialPos;
    }

    public void OnCancel()
    {
        if (_upgradeFailCanvas.activeSelf || _upgradeSucceedCanvas.activeSelf)
        {
            return;
        }
        if (_legacyUpgradeCanvas.activeSelf)
        {
            _legacyUpgradeCanvas.SetActive(false);
        }
        else if (_confirmPanelObject.activeSelf)
        {
            _confirmPanelObject.SetActive(false);
        }
    }
    
    public override void OnNavigate(Vector2 value)
    {
        if (_upgradeFailCanvas.activeSelf || _upgradeSucceedCanvas.activeSelf)
        {
            return;
        }
        if (_legacyUpgradeCanvas.activeSelf)
        {
            _legacyUpgradeController.OnNavigate(value);
        }
        else if (_confirmPanelObject.activeSelf)
        {
            return;
        }
        else
        {
            switch (value.y)
            {
                // Left or Right
                case 0:
                    return;
                // Up
                case > 0:
                    {
                        AudioManager.Instance.PlayUINavigateSound();
                        if (_selectedPanelIdx == 0) SelectPanel(3);
                        else if (_selectedPanelIdx == 3) SelectPanel(_numUsedPanels - 1);
                        else SelectPanel(_selectedPanelIdx - 1);
                        break;
                    }
                // Down
                case < 0:
                    {
                        AudioManager.Instance.PlayUINavigateSound();
                        if (_selectedPanelIdx == 3) SelectPanel(0);
                        else if (_selectedPanelIdx == _numUsedPanels - 1) SelectPanel(3);
                        else SelectPanel(_selectedPanelIdx + 1);
                        break;
                    }
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
