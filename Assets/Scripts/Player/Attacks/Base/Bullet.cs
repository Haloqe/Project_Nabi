using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Legacy_Range _owner;
    private float _speed = 8.5f;
    private float _lifeTime = 4f;
    private float _timer = 0.0f;
    public float Direction { set; private get; }

    private void Start()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(Direction * _speed, 0.0f);
    }

    private void Update()
    {
        // Kill if lifeTime ended
        _timer += Time.deltaTime;
        if (_timer > _lifeTime) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Disable object
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;

        // TODO if enemy,

        // Play hit effect
        GetComponentInChildren<ParticleSystem>().Play();
    }
}