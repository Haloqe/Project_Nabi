using UnityEngine;
using UnityEngine.Serialization;

public class Bullet : MonoBehaviour
{
    public AttackBase_Ranged Owner;
    private Rigidbody2D _rigidbody2D;
    
    private float _speed = 14f;
    private float _lifeTime = 4f;
    private float _timer = 0.0f;
    public float Direction { set; private get; }
    public AttackInfo attackInfo;
    private bool _toBeDestroyed;
    [SerializeField] private bool shouldRotate;

    private void Start()
    {
        // Set velocity
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.velocity = new Vector2(Direction * _speed / Time.timeScale, 0.0f);

        // Flip sprite to the flying direction
        transform.localScale = new Vector3(-Direction, 1, 1);
    }

    private void Update()
    {
        // Kill if lifeTime ended
        _timer += Time.unscaledDeltaTime;
        if (_timer > _lifeTime) DestroySelf(null);
        
        // Animation
        if (shouldRotate)
        {
            transform.Rotate(0,0,5f);
        }
    }

    public void UpdateVelocityOnTimeScaleChange()
    {
        _rigidbody2D.velocity = new Vector2(Direction * _speed / Time.timeScale, 0.0f);
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
        Owner.OnHit(this, target, transform.position, attackInfo);
    }
}
