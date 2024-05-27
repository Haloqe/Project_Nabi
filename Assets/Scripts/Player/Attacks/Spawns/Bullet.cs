using UnityEngine;

public class Bullet : MonoBehaviour
{
    public AttackBase_Ranged Owner;
    private float _speed = 8f;
    private float _lifeTime = 4f;
    private float _timer = 0.0f;
    public float Direction { set; private get; }
    public AttackInfo attackInfo;
    private bool _toBeDestroyed;
    [SerializeField] private bool _shouldRotate;

    private void Start()
    {
        // Set velocity
        GetComponent<Rigidbody2D>().velocity = new Vector2(Direction * _speed, 0.0f);

        // Flip sprite to the flying direction
        transform.localScale = new Vector3(-Direction, 1, 1);
    }

    private void Update()
    {
        // Kill if lifeTime ended
        _timer += Time.deltaTime;
        if (_timer > _lifeTime) DestroySelf(null);
        
        // Animation
        if (_shouldRotate)
        {
            transform.Rotate(0,0,5f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_toBeDestroyed) return;
        _toBeDestroyed = true;
        IDamageable target = collision.gameObject.GetComponentInParent<IDamageable>();
        DestroySelf(target);
    }

    // Destroy bullet after hitting target; if target is null then nobody is hit
    private void DestroySelf(IDamageable target)
    {
        Destroy(gameObject);
        Owner.OnHit(target, transform.position, attackInfo);
    }
}