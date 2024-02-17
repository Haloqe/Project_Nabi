using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LegacyData_Ranged", menuName = "LegacyData/LegacyData_Ranged")]
public class Legacy_Ranged : ActiveLegacySO
{
    [Header("Attack Type Specific Settings")]
    // 플레이어의 위치를 원점으로, 총알이 나갈 때 생성할 오브젝트
    public GameObject AttackSpawnObject;
    
    // 총알의 위치를 원점으로, 총알이 사라질 때 생성할 오브젝트
    public GameObject BulletDestroySpawnObject;
    
    public float BulletSpeedMultiplier = 1.0f;

    public override void Init(Transform playerTransform)
    {
        _spawnScaleMultiplier = 1.0f;
        _playerTransform = playerTransform;
        var player = _playerTransform.gameObject.GetComponent<PlayerDamageDealer>();
        if (AttackSpawnObject)
            AttackSpawnObject.GetComponent<AttackSpawnObject>().PlayerDamageDealer = player;
        if (BulletDestroySpawnObject)
            BulletDestroySpawnObject.GetComponent<AttackSpawnObject>().PlayerDamageDealer = player;
    }
    
    public override void OnUpdateStatusEffect(EStatusEffect newEffect)
    {
        if (AttackSpawnObject)
            AttackSpawnObject.GetComponent<AttackSpawnObject>().StatusEffect = newEffect;
        if (BulletDestroySpawnObject)
            BulletDestroySpawnObject.GetComponent<AttackSpawnObject>().StatusEffect = newEffect;
    }
    
    private void OnAttack()
    {
        
    }

    private void OnHit()
    {

    }

    public void OnBulletDestroy(Vector3 bulletPos)
    {
        if (BulletDestroySpawnObject == null) return;
        var obj = Instantiate(BulletDestroySpawnObject, bulletPos, Quaternion.identity);
        var localScale = obj.transform.localScale;
        obj.transform.localScale = new Vector3(localScale.x * _spawnScaleMultiplier, localScale.y * _spawnScaleMultiplier, localScale.z);
    }
}