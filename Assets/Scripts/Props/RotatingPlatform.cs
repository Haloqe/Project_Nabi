using System.Collections;
using UnityEngine;

// TODO Incomplete
public class RotatingPlatform : MonoBehaviour
{
    public bool ShouldPingPong;
    public Vector3 MinRotation;
    public Vector3 MaxRotation;
    public float Duration;
    public Vector3 FullRotation;
    
    public bool IsMovingPlatform;
    private bool IsPlayerOnPlatform;


    public void Start()
    {
        if (Duration > 0 && ShouldPingPong) 
            StartCoroutine(RotationCoroutine());
    }

    public void Update()
    {
        var prev = transform.localEulerAngles;

        if (!ShouldPingPong)
            transform.Rotate(FullRotation * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (IsPlayerOnPlatform)
        {
            Transform trans = PlayerController.Instance.transform;
            trans.RotateAround(transform.position, Vector3.forward, FullRotation.z * Time.deltaTime);
            trans.rotation = Quaternion.identity;
        }
    }

    private IEnumerator RotationCoroutine()
    {
        while (true)
        {
            for (float time = 0; time < Duration * 2; time += Time.deltaTime)
            {
                float progress = Mathf.PingPong(time, Duration) / Duration;
                transform.eulerAngles = Vector3.Lerp(MinRotation, MaxRotation, progress);
                yield return null;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsMovingPlatform) return;

        var player = collision.gameObject;
        if (player.CompareTag("Player"))
        {
            var movementComp = player.GetComponent<PlayerMovement>();
            // Return if already in moving platfrom
            if (movementComp.IsOnMovingPlatform) return;
            else
            {
                IsPlayerOnPlatform = true;
                movementComp.IsOnMovingPlatform = true;
                movementComp.SetFriction(0.7f);
                //player.transform.SetParent(transform);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsMovingPlatform || !IsPlayerOnPlatform) return;

        var player = collision.gameObject;
        if (player.CompareTag("Player"))
        {
            IsPlayerOnPlatform = false;
            player.GetComponent<PlayerMovement>().IsOnMovingPlatform = false;
            player.GetComponent<PlayerMovement>().ResetFriction();

            //player.transform.SetParent(null);
            player.transform.rotation = Quaternion.identity;
            //player.transform.localScale = new Vector3(player.transform.localScale.x, 1, 1);
        }
    }
}
