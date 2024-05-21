using TMPro;
using UnityEngine;

public class MainMenuUIController : MonoBehaviour
{
    private TextMeshProUGUI[] _options;

    
    private void Awake()
    {
        _options = transform.Find("Panel").Find("Options").GetComponents<TextMeshProUGUI>();
    }
    
    
}