using System.Globalization;
using TMPro;
using UnityEngine;

public class SoulShardUI : MonoBehaviour
{
    private PlayerInventory _playerInventory;
    private TextMeshProUGUI _valueText;
    private Animator _iconAnimator;
    private readonly static int Emphasis = Animator.StringToHash("Emphasis");

    private void Awake()
    {
        _valueText = GetComponentInChildren<TextMeshProUGUI>();
        _iconAnimator = GetComponentInChildren<Animator>();
        PlayerEvents.Spawned += Initialise;
        PlayerEvents.SoulShardChanged += OnPlayerSoulShardChanged;
    }

    private void OnDestroy()
    {
        PlayerEvents.Spawned -= Initialise;
        PlayerEvents.SoulShardChanged -= OnPlayerSoulShardChanged;
    }

    private void Initialise()
    {
        _playerInventory = PlayerController.Instance.playerInventory;
        _valueText.text = _playerInventory.SoulShard.ToString(CultureInfo.CurrentCulture);
    }

    private void OnPlayerSoulShardChanged()
    {
        _valueText.text = _playerInventory.SoulShard.ToString(CultureInfo.CurrentCulture);
        if (_iconAnimator) _iconAnimator.SetTrigger(Emphasis);
    }
}
