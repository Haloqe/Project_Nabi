using System.Globalization;
using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    private PlayerInventory _playerInventory;
    private TextMeshProUGUI _goldText;
    private Animator _goldIconAnimator;
    private readonly static int Emphasis = Animator.StringToHash("Emphasis");

    private void Awake()
    {
        _goldText = GetComponentInChildren<TextMeshProUGUI>();
        _goldIconAnimator = GetComponentInChildren<Animator>();
        _goldText.text = "0";
        PlayerEvents.GoldChanged += OnPlayerGoldChanged;
    }

    private void OnDestroy()
    {
        PlayerEvents.GoldChanged -= OnPlayerGoldChanged;
    }

    private void Start()
    {
        _playerInventory = PlayerController.Instance.playerInventory;
        _goldText.text = _playerInventory.Gold.ToString(CultureInfo.CurrentCulture);
    }

    private void OnPlayerGoldChanged()
    {
        _goldText.text = _playerInventory.Gold.ToString(CultureInfo.CurrentCulture);
        if (_goldIconAnimator) _goldIconAnimator.SetTrigger(Emphasis);
    }
}