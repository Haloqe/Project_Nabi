using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    private float _flipRotationTime = 1f;
    private Transform _playerTransform;

    private void Start()
    {
        _playerTransform = PlayerController.Instance.transform;
    }
    
    private void Update()
    {
        transform.position = _playerTransform.position;
    }

    public void TurnCamera()
    {
        StartCoroutine(FlipYLerp());
    }

    private IEnumerator FlipYLerp()
    {
        float startRotation = transform.localEulerAngles.y;
        float playerDirection = Mathf.Sign(_playerTransform.localScale.x);
        float endRotation = playerDirection > 0 ? 180f : 0f;
        float yRotation = 0f;

        float timer = 0f;
        while (timer < _flipRotationTime)
        {
            yRotation = Mathf.Lerp(startRotation, endRotation, (timer / _flipRotationTime));
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

            if (Mathf.Sign(_playerTransform.localScale.x) != playerDirection) yield break;
            yield return null;
            timer += Time.deltaTime;
        }
    }
}
