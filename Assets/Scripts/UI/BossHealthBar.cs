using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Image _hpBarImage;

    public void OnBossHPChanged(float percentDamage)
    {
        _hpBarImage.fillAmount += percentDamage;
        Debug.Log("took damage " + percentDamage);
    }
}
