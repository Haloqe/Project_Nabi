using System.Collections;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Vector3 MinRotation;
    public Vector3 MaxRotation;
    public float Duration;

    public void Start()
    {
        if (Duration > 0) StartCoroutine(RotationCoroutine());
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
}
