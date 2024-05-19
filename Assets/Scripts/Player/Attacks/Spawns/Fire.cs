using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Fire : MonoBehaviour
{
    public AttackBase_Area Owner;
    public int burningDelay = 0;
    public int burningDuration;
    public bool readyToCollide;
    // Collider
    private List<int> _affectedEnemies;
    private FireChild[] _fireChildren;

    private void Start()
    {
        _affectedEnemies = new List<int>();
        _fireChildren = GetComponentsInChildren<FireChild>();
        StartCoroutine(BurningCoroutine());
    }

    private IEnumerator BurningCoroutine()
    {
        yield return new WaitForSeconds(burningDelay);
        StartCoroutine(nameof(BurnToggleCoroutine));
        yield return new WaitForSeconds(burningDuration);
        StopCoroutine(nameof(BurnToggleCoroutine));
        Destroy(gameObject);
    }

    private IEnumerator BurnToggleCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.5f);
            _affectedEnemies.Clear();
            foreach (var fire in _fireChildren)
            {
                fire.Toggle();
            }
        }
    }

    public void OnGroundContact(Collider2D collision)
    {
        collision.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
    }
    public void OnEnemyEnter(Collider2D collision)
    {
        if (Utility.IsObjectInList(collision.gameObject, _affectedEnemies)) return;
        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target != null)
        {
            Owner.DealDamage(target);
            _affectedEnemies.Add(collision.gameObject.GetInstanceID());
        }   
    }
    
}
