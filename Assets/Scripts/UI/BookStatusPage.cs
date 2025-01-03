using TMPro;
using UnityEngine;

public class BookStatusPage : BookPage
{
    // References
    private GameManager _gameManager;
    private PlayerController _playerController;
    private AudioSource _audioSource;
    
    // Left Page
    [SerializeField] private TextMeshProUGUI deathTMP;
    [SerializeField] private TextMeshProUGUI killTMP;
    [SerializeField] private TextMeshProUGUI goldTMP;
    [SerializeField] private TextMeshProUGUI soulTMP;
    
    // Right Page
    [SerializeField] private TextMeshProUGUI healthTMP;
    [SerializeField] private TextMeshProUGUI strengthTMP;
    [SerializeField] private TextMeshProUGUI criticalRateTMP;
    [SerializeField] private TextMeshProUGUI armourTMP;
    [SerializeField] private TextMeshProUGUI armourPenetrationTMP;
    [SerializeField] private TextMeshProUGUI evasionRateTMP;
    [SerializeField] private TextMeshProUGUI healEfficiencyTMP;
    
    [SerializeField] private TextMeshProUGUI[] attackTMPs;
    [SerializeField] private TextMeshProUGUI totalTMP;

    public override void Init(BookUIController baseUI)
    {
        BaseUI = baseUI;
        _gameManager = GameManager.Instance;
        _playerController = PlayerController.Instance;
        _audioSource = GetComponent<AudioSource>();
    }
    
    public override void OnBookOpen()
    {
        // Left Page
        deathTMP.text = "DEATHS: " + _gameManager.PlayerMetaData.numDeaths;
        killTMP.text = "KILLS: " + _gameManager.PlayerMetaData.numKills;
        goldTMP.text = "GOLD: " + _playerController.playerInventory.Gold;
        soulTMP.text = "SOULS: " + _playerController.playerInventory.SoulShard;
        
        // Right Page
        healthTMP.text = Utility.FormatFloat(_playerController.playerDamageReceiver.BaseHealth);
        strengthTMP.text = Utility.FormatFloat(_playerController.BaseStrength);
        criticalRateTMP.text = Utility.FormatPercentage(_playerController.BaseCriticalRate) + "%";
        armourTMP.text = Utility.FormatFloat(_playerController.BaseArmour);
        armourPenetrationTMP.text = Utility.FormatFloat(_playerController.BaseArmourPenetration);
        evasionRateTMP.text = Utility.FormatPercentage(_playerController.BaseEvasionRate) + "%";
        healEfficiencyTMP.text = Utility.FormatFloat(_playerController.BaseHealEfficiency);
        
        // Additions
        float healthDiff = _playerController.playerDamageReceiver.MaxHealth - _playerController.playerDamageReceiver.BaseHealth;
        float strengthDiff = _playerController.Strength - _playerController.BaseStrength;
        float critRateDiff = _playerController.CriticalRate - _playerController.BaseCriticalRate;
        float armourDiff = _playerController.Armour - _playerController.BaseArmour;
        float armourPenetrationDiff = _playerController.ArmourPenetration - _playerController.BaseArmourPenetration;
        float evasionRateDiff = _playerController.EvasionRate - _playerController.BaseEvasionRate;
        float healEfficiencyDiff = _playerController.HealEfficiency - _playerController.BaseHealEfficiency;
        
        // Addition texts
        if (healthDiff != 0) healthTMP.text += $"<color=#32a852> (+{Utility.FormatFloat(healthDiff)})</color>";
        if (strengthDiff != 0) strengthTMP.text += $"<color=#32a852> (+{Utility.FormatFloat(strengthDiff)})</color>";
        if (critRateDiff != 0) criticalRateTMP.text += $"<color=#32a852> (+{Utility.FormatPercentage(critRateDiff)}%)</color>";
        if (armourDiff != 0) armourTMP.text += $"<color=#32a852> (+{Utility.FormatFloat(armourDiff)})</color>";
        if (armourPenetrationDiff != 0) armourPenetrationTMP.text += $"<color=#32a852> (+{Utility.FormatFloat(armourPenetrationDiff)})</color>";
        if (evasionRateDiff != 0) evasionRateTMP.text += $"<color=#32a852> (+{Utility.FormatPercentage(evasionRateDiff)}%)</color>";
        if (healEfficiencyDiff != 0) healEfficiencyTMP.text += $"<color=#32a852> (+{Utility.FormatFloat(healEfficiencyDiff)})</color>";
        
        // Attack Additions
        var damageMultipliers = _playerController.playerDamageDealer.attackDamageMultipliers;
        for (int i = 0; i < 4; i++)
        {
            if (damageMultipliers[i] != 1) attackTMPs[i].text += $"<color=#00A7B7> (X{Utility.FormatFloat(damageMultipliers[i])})</color>";
        }
        if (_playerController.playerDamageDealer.totalDamageMultiplier != 1)
            totalTMP.text += $"<color=#00A7B7> (X{Utility.FormatFloat(_playerController.playerDamageDealer.totalDamageMultiplier)})</color>";
    }

    public override void OnPageOpen()
    {
        BaseUI.SelectOption(0);
    }
    
    public override void OnNavigate(Vector2 value)
    {
        if (value.x == 0) return;
        BaseUI.NavigateOptions(value.x);
        _audioSource.Play();
    }
    
    public override void OnSubmit()
    {
        if (BaseUI == null) return;
        BaseUI.OnSubmitOption();        
    }
}
