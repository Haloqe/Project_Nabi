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
        yield return new WaitForSecondsRealtime(burningDelay);
        StartCoroutine(nameof(BurnToggleCoroutine));
        yield return new WaitForSecondsRealtime(burningDuration);
        StopCoroutine(nameof(BurnToggleCoroutine));
        Destroy(gameObject);
    }

    private IEnumerator BurnToggleCoroutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1.5f);
            _affectedEnemies.Clear();
            foreach (var fire in _fireChildren)
            {
                fire.Toggle();
            }
        }
    }

    public void OnEnemyEnter(Collider2D collision)
    {
        // Check if the hit target is valid
        var rootEnemyDamageable = collision.GetComponentInParent<IDamageable>();
        if (rootEnemyDamageable == null || Utility.IsObjectInList(rootEnemyDamageable.GetGameObject(), _affectedEnemies)) return;

        // Do damage
        Owner.DealDamage(rootEnemyDamageable, true);
        _affectedEnemies.Add(rootEnemyDamageable.GetGameObject().GetInstanceID()); 
    }
    
}
