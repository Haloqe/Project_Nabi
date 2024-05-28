using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomGuideUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image panel;

    private void Start()
    {
        // Initially make them transparent
        roomNameText.color = new Color(roomNameText.color.r, roomNameText.color.g, roomNameText.color.b, 0);
        descriptionText.color = new Color(descriptionText.color.r, descriptionText.color.g, descriptionText.color.b, 0);
        panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, 0);
        
        StartCoroutine(FadeInOutCoroutine());
    }

    public void InitSecretRoomGuide(string roomName, string description)
    {
        roomNameText.text = roomName;
        descriptionText.text = description;
    }
    
    private IEnumerator FadeInOutCoroutine()
    {
        // Quickly decrease transparency
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.unscaledDeltaTime * 1.4f;
            roomNameText.color = new Color(roomNameText.color.r, roomNameText.color.g, roomNameText.color.b, alpha);
            descriptionText.color = new Color(descriptionText.color.r, descriptionText.color.g, descriptionText.color.b, alpha);
            panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, alpha);
            yield return null;
        }
        
        // Display ui for a while
        yield return new WaitForSecondsRealtime(2f);

        // Quickly increase transparency
        while (alpha > 0)
        {
            alpha -= Time.unscaledDeltaTime * 1.4f;
            roomNameText.color = new Color(roomNameText.color.r, roomNameText.color.g, roomNameText.color.b, alpha);
            descriptionText.color = new Color(descriptionText.color.r, descriptionText.color.g, descriptionText.color.b, alpha);
            panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, alpha);
            yield return null;
        }
        
        Destroy(gameObject);
    }
}