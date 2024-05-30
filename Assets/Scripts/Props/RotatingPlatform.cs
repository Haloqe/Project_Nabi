using System.Collections;
using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{
    public float FullRotationSpeed;
    private bool _isPlayerOnPlatform;


    public void Update()
    {
        transform.Rotate(Vector3.forward, FullRotationSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (_isPlayerOnPlatform)
        {
            Transform trans = PlayerController.Instance.transform;
            trans.RotateAround(transform.position, Vector3.forward, FullRotationSpeed * Time.deltaTime);
            trans.rotation = Quaternion.identity;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.gameObject;
        if (player.CompareTag("Player"))
        {
            var movementComp = player.GetComponent<PlayerMovement>();
            // Return if already in moving platfrom
            if (movementComp.isOnMovingPlatform) return;
            else
            {
                _isPlayerOnPlatform = true;
                movementComp.isOnMovingPlatform = true;
                movementComp.SetFriction(0.7f);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_isPlayerOnPlatform) return;

        var player = collision.gameObject;
        if (player.CompareTag("Player"))
        {
            _isPlayerOnPlatform = false;
            player.GetComponent<PlayerMovement>().isOnMovingPlatform = false;
            player.GetComponent<PlayerMovement>().ResetFriction();
            player.transform.rotation = Quaternion.identity;
        }
    }
}
