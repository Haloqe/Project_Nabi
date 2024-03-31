using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
        yield return new WaitForSeconds(explodeDelay);

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
        if (collision.gameObject.CompareTag("Enemy") == false) return;
        if (Utility.IsObjectInList(collision.gameObject, _affectedEnemies)) return;

        Debug.Log("Enemy Hit!");

        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target != null)
        {
            Owner.DealDamage(target);
            _affectedEnemies.Add(collision.gameObject.GetInstanceID());
        }
    }
}