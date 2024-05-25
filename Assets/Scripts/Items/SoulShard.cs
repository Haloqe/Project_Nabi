using UnityEngine;

public class SoulShard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController.Instance.playerInventory.CollectSoulShard();
        Destroy(gameObject);
    }
}