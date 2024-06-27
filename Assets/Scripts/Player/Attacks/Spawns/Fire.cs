using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Fire : MonoBehaviour
{
    public AttackBase_Area Owner;
    public int burningDuration;
    private List<int> _affectedEnemies;
    private FireChild[] _fireChildren;
    private AttackInfo _attackInfo;

    private void Start()
    {
        _affectedEnemies = new List<int>();
        _fireChildren = GetComponentsInChildren<FireChild>();
        _attackInfo = new AttackInfo
        {
            Damage = new DamageInfo(EDamageType.Base, 5),
            ShouldUpdateTension = false,
        };
        StartCoroutine(BurningCoroutine());
    }

    private IEnumerator BurningCoroutine()
    {
        StartCoroutine(nameof(BurnToggleCoroutine));
        yield return new WaitForSecondsRealtime(burningDuration);
        StopCoroutine(nameof(BurnToggleCoroutine));
        Destroy(gameObject);
    }

    private IEnumerator BurnToggleCoroutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);
            _affectedEnemies.Clear();
            foreach (var fire in _fireChildren)
            {
                fire.ToggleCollider();
            }
        }
    }

    public void OnEnemyEnter(Collider2D collision)
    {
        // Check if the hit target is valid
        var rootEnemyDamageable = collision.GetComponentInParent<IDamageable>();
        if (rootEnemyDamageable == null || Utility.IsObjectInList(rootEnemyDamageable.GetGameObject(), _affectedEnemies)) return;

        // Do damage
        Owner.DealDamage(rootEnemyDamageable, _attackInfo);
        _affectedEnemies.Add(rootEnemyDamageable.GetGameObject().GetInstanceID()); 
    }
    
}
