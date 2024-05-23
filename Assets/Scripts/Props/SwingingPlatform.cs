using System.Collections;
using UnityEngine;

public class SwingingPlatform : MonoBehaviour
{
    public float MinRotation;
    public float MaxRotation;
    public float Duration;

    private bool _isPlayerOnPlatform;
    private float _deltaRotation;
    private float _prevRotation;


    public void Start()
    {
        if (Duration > 0)
            StartCoroutine(RotationCoroutine());
    }

    private void LateUpdate()
    {
        if (_isPlayerOnPlatform)
        {
            Transform trans = PlayerController.Instance.transform;
            trans.RotateAround(transform.position, Vector3.forward, _deltaRotation);
            trans.rotation = Quaternion.identity;
        }
    }

    private IEnumerator RotationCoroutine()
    {
        while (true)
        {
            for (float time = 0; time < Duration * 2; time += Time.deltaTime)
            {
                _prevRotation = transform.eulerAngles.z;
                float progress = Mathf.PingPong(time, Duration) / Duration;
                transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(MinRotation, MaxRotation, progress));
                _deltaRotation = transform.eulerAngles.z - _prevRotation;
                yield return null;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.gameObject;
        if (player.CompareTag("Player"))
        {
            var movementComp = player.GetComponent<PlayerMovement>();
            // Return if already in moving platform
            if (!movementComp.IsOnMovingPlatform)
            {
                _isPlayerOnPlatform = true;
                movementComp.IsOnMovingPlatform = true;
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
            player.GetComponent<PlayerMovement>().IsOnMovingPlatform = false;
            player.GetComponent<PlayerMovement>().ResetFriction();
            player.transform.rotation = Quaternion.identity;
        }
    }
}
