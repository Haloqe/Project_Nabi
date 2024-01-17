using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 MinOffset;
    public Vector3 MaxOffset;
    public float Speed;

    private Vector3 _minPos;
    private Vector3 _maxPos;
    [SerializeField] private bool _startTowardsMin;

    private void Start()
    {
        _minPos = transform.position - MinOffset;
        _maxPos = transform.position + MaxOffset;
        StartCoroutine(MoveCoroutine(_startTowardsMin ? _minPos : _maxPos));
    }

    public void Update()
    {
        if (transform.position == _minPos)
        {
            StartCoroutine(MoveCoroutine(_maxPos));
        }
        else if (transform.position == _maxPos)
        {
            StartCoroutine(MoveCoroutine(_minPos));
        }
    }

    IEnumerator MoveCoroutine(Vector3 target)
    {
        Vector3 startPosition = transform.position;
        float time = 0f;

        while (transform.position != target)
        {
            transform.position = Vector3.Lerp(startPosition, target, (time / Vector3.Distance(startPosition, target)) * Speed);
            time += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var player = collision.gameObject;
        if (player.CompareTag("Player"))
        {
            player.transform.SetParent(null);
            player.transform.rotation = Quaternion.identity;
            player.transform.localScale = new Vector3(player.transform.localScale.x, 1, 1);
        }
    }
}
