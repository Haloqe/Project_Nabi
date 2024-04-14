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

        followUpVFX();
    }

    public void followUpVFX()
    {
        string GravityVfxAddress = "Prefabs/Player/BombVFX/GravityField";
        GameObject GravityVFXObject = Utility.LoadGameObjectFromPath(GravityVfxAddress);

        float dir = Mathf.Sign(gameObject.transform.localScale.x);
        Vector3 playerPos = gameObject.transform.position;
        Vector3 vfxPos = GravityVFXObject.transform.position;
        Vector3 position = new Vector3(playerPos.x + dir * (vfxPos.x), playerPos.y + vfxPos.y, playerPos.z + vfxPos.z);

        GravityVFXObject.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
        var vfx = Instantiate(GravityVFXObject, position, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") == false) return;
        if (Utility.IsObjectInList(collision.gameObject, _affectedEnemies)) return;

        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target != null)
        {
            Owner.DealDamage(target);
            _affectedEnemies.Add(collision.gameObject.GetInstanceID());


        }
    }
}