using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Fire : MonoBehaviour
{
    public AttackBase_Area Owner;
    public int burningDelay = 0;
    public int burningDuration;
    public bool readyToCollide;
    // Collider
    private List<int> _affectedEnemies;
    private CircleCollider2D _collider;

    private void Start()
    {
        _affectedEnemies = new List<int>();
        _collider = GetComponent<CircleCollider2D>();
        //var ps = gameObject.GetComponent<ParticleSystem>();
        StartCoroutine(BurningCoroutine(burningDelay, burningDuration));
    }

    private IEnumerator BurningCoroutine(float burningDelay, float burningDuration)
    {
        yield return new WaitForSeconds(burningDelay);

        _collider.enabled = true;

        for (float time = 0; time < burningDuration; time += Time.deltaTime)
        {
            _collider.enabled = !_collider.enabled;
            Debug.Log(_collider.enabled);
            _affectedEnemies.Clear();
            // clear list
            Debug.Log("List cleared");
            yield return new WaitForSeconds(3f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.CompareTag("Enemy") == false) return;
        Debug.Log(collision.gameObject.GetInstanceID());
        if (Utility.IsObjectInList(collision.gameObject, _affectedEnemies)) return;
        Debug.Log("Not in List");

        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target != null)
        {
            Owner.DealDamage(target);
            _affectedEnemies.Add(collision.gameObject.GetInstanceID());

        }   
    }

}