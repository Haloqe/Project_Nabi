using System.Collections;
using UnityEngine;

public class SoulShard : MonoBehaviour
{
    private bool _isInteracting;
    
    private void Start()
    {
        StartCoroutine(BounceCoroutine());
    }

    private IEnumerator BounceCoroutine()
    {
        Vector3 lowestPos = transform.position;
        Vector3 highestPos = transform.position + new Vector3(0, 0.2f, 0);
        
        while (true)
        {
            for (float time = 0; time < 2 * 2; time += Time.unscaledDeltaTime)
            {
                float progress = Mathf.PingPong(time, 2) / 2;
                transform.position = Vector3.Lerp(lowestPos, highestPos, progress);
                yield return null;
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isInteracting) return;
        _isInteracting = true;
        PlayerController.Instance.playerInventory.CollectSoulShard();
        Destroy(gameObject);
    }
}