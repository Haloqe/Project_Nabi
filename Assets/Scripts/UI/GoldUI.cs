using System;
using System.Globalization;
using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    private TextMeshProUGUI _goldText;
    private Animator _goldIconAnimator;
    private readonly static int Emphasis = Animator.StringToHash("Emphasis");

    private void Awake()
    {
        _goldText = GetComponentInChildren<TextMeshProUGUI>();
        _goldIconAnimator = GetComponentInChildren<Animator>();

        _goldText.text = "0";
        GameEvents.restarted += () => OnPlayerGoldChanged(0);
        PlayerEvents.goldChanged += OnPlayerGoldChanged;
    }
    
    private void OnPlayerGoldChanged(float gold)
    {
        _goldText.text = gold.ToString(CultureInfo.CurrentCulture);
        if (_goldIconAnimator) _goldIconAnimator.SetTrigger(Emphasis);
    }
}