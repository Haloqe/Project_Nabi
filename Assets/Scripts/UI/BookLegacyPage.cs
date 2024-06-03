using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookLegacyPage : BookPage
{
    // References
    private PlayerController _playerController;
    private PlayerAttackManager _playerAttackManager;
    private AudioSource _audioSource;
    
    // Left Page
    private int _newestSelectedIdx;
    private Color _unselectedColour;
    private string[] _attackBaseDescriptions;
    private bool[] _hasActiveBounds;
    private string[] _activeNames;
    private string[] _activeDescs;
    private string[] _activeWarriors; 
    private string[] _activePreservations;
    
    [SerializeField] private Image[] activeIcons;
    [SerializeField] private GameObject activeNoBoundObj;
    [SerializeField] private TextMeshProUGUI attackBaseDescription;
    [SerializeField] private GameObject activeBoundDescObj;
    [SerializeField] private TextMeshProUGUI activeBoundNameText;
    [SerializeField] private TextMeshProUGUI activeBoundDescText;
    [SerializeField] private TextMeshProUGUI activeBoundExtraText;
    
    // Right Page
    private int _numBoundPassives;
    private int _numCols = 6;
    private string[] _passiveNames;
    private string[] _passiveDescs;
    private string[] _passiveWarriors; 
    private string[] _passivePreservations;
    private int[] _passiveIDs;

    private GameObject[] _passiveHighlights;
    private Image[] _passiveIcons;
    [SerializeField] private GameObject passiveNoBoundObj;
    [SerializeField] private GameObject passiveBoundDescObj;
    [SerializeField] private TextMeshProUGUI passiveBoundNameText;
    [SerializeField] private TextMeshProUGUI passiveBoundDescText;
    [SerializeField] private TextMeshProUGUI passiveBoundExtraText;

    public override void Init(BookUIController baseUI)
    {
        BaseUI = baseUI;
        _audioSource = GetComponent<AudioSource>();
        _playerController = PlayerController.Instance;
        _playerAttackManager = PlayerAttackManager.Instance;
        _unselectedColour = new Color(1, 1, 1, 0.15f);
        foreach (var activeIcon in activeIcons)
        {
            activeIcon.color = _unselectedColour;
        }
        _attackBaseDescriptions = new string[]
        {
            "태엽 열쇠를 전방으로 가볍게 휘둘러 맞은 모든 적에게 데미지를 입힌다. 세번째 연속된 공격마다 열쇠를 바닥에 내려쳐 더 강한 데미지를 입힌다.",
            "전방으로 총알을 발사해 처음으로 맞은 적에게 데미지를 입힌다. 총알은 벽이나 몹에 맞으면 사라진다.",
            "전방으로 짧은 거리를 빠르게 이동한다. 바인딩된 유산이 없을 시 아무런 데미지를 입히지 않는다.",
            "전방으로 폭탄꽃을 던져 맞은 모든 적에게 피해를 입힌다.",
        };
        _passiveHighlights = new GameObject[24];
        var passivesRoot = transform.Find("Page_R").Find("Passives");
        _passiveIcons = passivesRoot.GetComponentsInChildren<Image>();
        for (int i = 0; i < 24; i++)
        {
            _passiveHighlights[i] = _passiveIcons[i].transform.Find("Highlighter").gameObject;
        }
    }
    
    public override void OnBookOpen()
    {
        GetBoundLegacyInfo();
    }

    public override void OnPageOpen()
    {
        if (BaseUI)
        {
            _isNavigatingOptions = false;
            BaseUI.UnselectSelectedOption();
        }
        
        // Deselect old
        if (_newestSelectedIdx > 3) _passiveHighlights[ToPassiveIndex(_newestSelectedIdx)].SetActive(false);
        for (int i = 1; i < 4; i++) activeIcons[i].color = _unselectedColour;
        
        // Select new
        if (_numBoundPassives > 0) SelectPassiveIcon(0);
        SelectActiveIcon(0);
    }
    
    // TODO 나중에 시간되면 바인딩할때만 추가하도록하기 일단야매
    private void GetBoundLegacyInfo()
    {
        // Retrieve active legacy information
        bool hasActive = false;
        _hasActiveBounds = new bool[4];
        _activeNames = new string[4];
        _activeDescs = new string[4];
        _activeWarriors = new string[4];
        _activePreservations = new string[4];
        for (int i = 0; i < 4; i++)
        {
            var legacy = _playerController.playerDamageDealer.AttackBases[i].ActiveLegacy;
            if (legacy == null) continue;

            hasActive = true;
            _hasActiveBounds[i] = true;
            activeIcons[i].sprite = _playerAttackManager.GetLegacyIcon(legacy.id);
            _activeNames[i] = _playerAttackManager.GetBoundActiveLegacyName((ELegacyType)i);
            _activeDescs[i] = _playerAttackManager.GetBoundActiveLegacyDesc((ELegacyType)i);
            _activeWarriors[i] = Utility.GetColouredWarriorText(legacy.warrior);
            _activePreservations[i] = Utility.GetColouredPreservationText(legacy.preservation);
        }
        activeBoundDescObj.SetActive(hasActive);
        
        // Retrieve passive legacy information
        var passives = _playerAttackManager.GetAllBoundPassiveLegacyData();
        var passivePreservs = _playerAttackManager.GetAllBoundPassiveLegacyPreserv();
        int localisation = (int)Define.Localisation;
        _numBoundPassives = passives.Count;
        _passiveNames = new string[_numBoundPassives];
        _passiveDescs = new string[_numBoundPassives];
        _passiveWarriors = new string[_numBoundPassives];
        _passivePreservations = new string[_numBoundPassives];
        _passiveIDs = new int[_numBoundPassives];
        for (int i = 0; i < _numBoundPassives; i++)
        {
            _passiveIDs[i] = passives[i].ID;
            _passiveIcons[i].sprite = _playerAttackManager.GetLegacyIcon(passives[i].ID);
            _passiveIcons[i].color = Color.white;
            _passiveNames[i] = passives[i].Names[localisation];
            _passiveDescs[i] = passives[i].Descs[localisation];
            _passiveWarriors[i] = Utility.GetColouredWarriorText(passives[i].Warrior);
            _passivePreservations[i] = Utility.GetColouredPreservationText(passivePreservs[i]);
        }
        
        passiveBoundDescObj.SetActive(_numBoundPassives != 0);
        passiveNoBoundObj.SetActive(_numBoundPassives == 0);
    }

    private void SelectActiveIcon(int slotIdx)
    {
        // Change previous
        if (_newestSelectedIdx > 3) 
            _passiveHighlights[0].SetActive(false);
        else 
            activeIcons[_newestSelectedIdx].color = _unselectedColour;
        
        // Change new
        activeIcons[slotIdx].color = Color.white;
        
        // Base description
        attackBaseDescription.text = _attackBaseDescriptions[slotIdx];
        
        // Has bound legacy
        if (_hasActiveBounds[slotIdx])
        {
            activeBoundDescObj.SetActive(true);
            activeNoBoundObj.SetActive(false);
            activeBoundNameText.text = _activeNames[slotIdx];
            activeBoundDescText.text = _activeDescs[slotIdx];
            activeBoundExtraText.text = _activeWarriors[slotIdx] + " " + _activePreservations[slotIdx];
        }
        // No bound legacy
        else
        {
            activeBoundDescObj.SetActive(false);
            activeNoBoundObj.SetActive(true);
        }
        
        // Update selected index
        _newestSelectedIdx = slotIdx;
    }
    
    private void SelectPassiveIcon(int passiveSlotIdx)
    {
        // Can I select the icon?
        if (passiveSlotIdx >= _numBoundPassives) return;
        
        // Change highlighter of old & new 
        if (_newestSelectedIdx > 3) 
            _passiveHighlights[ToPassiveIndex(_newestSelectedIdx)].SetActive(false);
        _passiveHighlights[passiveSlotIdx].SetActive(true);
        
        // Update description
        passiveBoundNameText.text = _passiveNames[passiveSlotIdx];
        passiveBoundDescText.text = _passiveDescs[passiveSlotIdx];
        passiveBoundExtraText.text = _passiveWarriors[passiveSlotIdx] + " " + _passivePreservations[passiveSlotIdx];
        
        // Update selected index
        _newestSelectedIdx = FromPassiveIndex(passiveSlotIdx);
    }

    private bool _isNavigatingOptions;
    private int _prevNavigatingIdx;
    
    public override void OnNavigate(Vector2 value)
    {
        if (value.x < 0) NavigateLeft();
        else if (value.x > 0) NavigateRight();
        else if (value.y > 0) NavigateUp();
        else if (value.y < 0) NavigateDown();
    }

    private void NavigateLeft()
    {
        if (_isNavigatingOptions)
        {
            BaseUI.NavigateOptions(-1);
            _audioSource.Play();
            return;
        }
        
        switch (_newestSelectedIdx)
        {
            // first active
            case 0:
                {
                    // Do nothing
                    break;
                }
            // actives except the first (1,2,3)
            case > 0 and < 4:
                {
                    SelectActiveIcon(_newestSelectedIdx - 1);
                    _audioSource.Play();
                    break;
                }
            default:
                {
                    // first passive
                    if (_newestSelectedIdx == FromPassiveIndex(0))
                    {
                        SelectActiveIcon(3);
                    }
                    // other passives
                    else 
                    {
                        SelectPassiveIcon(ToPassiveIndex(_newestSelectedIdx - 1));
                    }
                    _audioSource.Play();
                    break;
                }
        }
    }

    private void NavigateRight()
    {
        if (_isNavigatingOptions)
        {
            BaseUI.NavigateOptions(1);
            _audioSource.Play();
            return;
        }
        
        switch (_newestSelectedIdx)
        {
            // actives except the last (0,1,2)
            case < 3:
                {
                    SelectActiveIcon(_newestSelectedIdx + 1);
                    _audioSource.Play();
                    break;
                }
            // last active
            case 3:
                {
                    if (_numBoundPassives > 0) SelectPassiveIcon(0);
                    _audioSource.Play();
                    break;
                }
            default:
                {
                    // last passive
                    int currSelectedPassiveIdx = ToPassiveIndex(_newestSelectedIdx); 
                    if (currSelectedPassiveIdx == _numBoundPassives - 1) 
                    {
                        // Do nothing
                    }
                    // other passives
                    else 
                    {
                        SelectPassiveIcon(currSelectedPassiveIdx + 1);
                        _audioSource.Play();
                    }
                    break;
                }
        }
    }

    private void NavigateUp()
    {
        // Options -> legacy
        if (_isNavigatingOptions)
        {
            _isNavigatingOptions = false;
            BaseUI.UnselectSelectedOption();
            if (_prevNavigatingIdx < 4) SelectActiveIcon(_prevNavigatingIdx);
            else SelectPassiveIcon(ToPassiveIndex(_prevNavigatingIdx));
            _audioSource.Play();
            return;
        }
        
        // Actives
        if (_newestSelectedIdx < 4) return;
        
        // The first passive row
        if (_newestSelectedIdx < FromPassiveIndex(_numCols)) return;
        
        // Other passives
        SelectPassiveIcon(ToPassiveIndex(_newestSelectedIdx - _numCols));
        _audioSource.Play();
    }

    private void NavigateDown()
    {
        if (_isNavigatingOptions) return;
        
        // Actives
        if (_newestSelectedIdx < 4 && BaseUI != null)
        {
            StartNavigatingOptions();
            _audioSource.Play();
            return;
        }
        
        // The last passive row
        int currSelectedPassiveIdx = ToPassiveIndex(_newestSelectedIdx);
        if (currSelectedPassiveIdx / _numCols == (_numBoundPassives - 1) / _numCols)
        {
            StartNavigatingOptions();
            _audioSource.Play();
            return;
        }
        
        // Other passives
        SelectPassiveIcon(currSelectedPassiveIdx + _numCols);
        _audioSource.Play();
    }

    private void StartNavigatingOptions()
    {
        if (BaseUI == null) return;
        _prevNavigatingIdx = _newestSelectedIdx;
        _isNavigatingOptions = true;
        BaseUI.SelectOption(0);
    }

    public override void OnSubmit()
    {
        if (BaseUI == null) return;
        if (!_isNavigatingOptions) return;
        BaseUI.OnSubmitOption();        
    }

    private static int ToPassiveIndex(int totalIndex) => totalIndex - 4;
    private static int FromPassiveIndex(int passiveIndex) => passiveIndex + 4;

    public int GetSelectedLegacyID()
    {
        if (_newestSelectedIdx == 3) return -1;
        if (_newestSelectedIdx < 3)
        {
            var legacy = _playerController.playerDamageDealer.AttackBases[_newestSelectedIdx].ActiveLegacy;
            return (legacy == null ? -1 : legacy.id);
        }
        else
        {
            return _passiveIDs[ToPassiveIndex(_newestSelectedIdx)];
        }
    }
}
