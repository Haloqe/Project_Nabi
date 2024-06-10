using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public AttackBase_Area Owner;
    private float _speed;
    private float _explodeDelay;

    // Collider
    private List<int> _affectedEnemies;
    private CircleCollider2D _collider;
    public int ExplodeStartSpriteIndex;
    public int ExplodeEndSpriteIndex;
    public float ExplodeMaxRadius;

    private void Start()
    {
        _affectedEnemies = new List<int>();
        _collider = GetComponent<CircleCollider2D>();
        var ps = gameObject.GetComponent<ParticleSystem>();
        int spriteCount = ps.textureSheetAnimation.numTilesX * ps.textureSheetAnimation.numTilesY;
        var explodeDelay = ps.main.startDelay.constant
            + ps.main.startLifetime.constant / spriteCount * ExplodeStartSpriteIndex;
        var explodeDuration = ps.main.startDelay.constant
            + ps.main.startLifetime.constant / spriteCount * ExplodeEndSpriteIndex - explodeDelay;
        StartCoroutine(ExplodeCoroutine(explodeDelay, explodeDuration));
    }

    private IEnumerator ExplodeCoroutine(float explodeDelay, float explodeDuration)
    {
        yield return new WaitForSeconds(explodeDelay - 0.3f);
        Owner.PlayExplosionSound();
        yield return new WaitForSeconds(0.3f);

        _collider.enabled = true;
        for (float time = 0; time < explodeDuration; time += Time.deltaTime)
        {
            float radius = Mathf.Lerp(0.2f, ExplodeMaxRadius, time / explodeDuration);
            _collider.radius = radius;
            yield return null;
        }
        _collider.enabled = false;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the hit target is valid
        var rootEnemyDamageable = collision.GetComponentInParent<IDamageable>();
        if (rootEnemyDamageable == null || Utility.IsObjectInList(rootEnemyDamageable.GetGameObject(), _affectedEnemies)) return;
        
        // Do damage
        Owner.DealDamage(rootEnemyDamageable, false);
        _affectedEnemies.Add(rootEnemyDamageable.GetGameObject().GetInstanceID());
    }
}
