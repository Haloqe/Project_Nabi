using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 orignalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.position = orignalPosition + new Vector3(x, y, -0.5f);
            elapsed += Time.deltaTime;
            yield return 0;
        }
        //Shake
        transform.position = orignalPosition;
        if(TestEffects.Instance.index>=15 && TestEffects.Instance.index <= 21)
            StartCoroutine(Shake(1f, 0.1f));
    }
}
