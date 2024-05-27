using System.Collections;
using TMPro;
using UnityEngine;

public class TextUI : MonoBehaviour
{
    private TextMeshProUGUI _tmp;
    public float lifeTime = 0.6f;
    private Transform parent;

    public void Init(Transform followParent, string text)
    {
        parent = followParent;
        _tmp.text = text;
    }

    private void Awake()
    {
        _tmp = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    private void Start()
    {
        StartCoroutine(LifeTimeCoroutine());
    }

    private void Update()
    {
        if (parent) transform.position = parent.position + new Vector3(0, 2.3f, 0);
    }
    
    private IEnumerator LifeTimeCoroutine()
    {
        yield return new WaitForSeconds(lifeTime);
        
        // Fade
        var changeAmount = new Color(0, 0, 0, 0.01f);
        while (_tmp.color.a >= 0.0f)
        {
            _tmp.color -= changeAmount;
            yield return null;
        }
        Destroy(gameObject);
    }
}
