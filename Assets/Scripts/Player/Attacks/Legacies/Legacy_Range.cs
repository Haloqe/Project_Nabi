using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LegacyData_Range", menuName = "LegacyData/LegacyData_Range")]
public class Legacy_Range : LegacySO
{
    // 플레이어의 위치를 원점으로, 총알이 나갈 때 생성할 오브젝트
    public GameObject AttackSpawnObject;
    
    // 총알의 위치를 원점으로, 총알이 사라질 때 생성할 오브젝트
    public GameObject BulletDestroySpawnObject;
    
    public float BulletSpeedMultiplier = 1.0f;

    private void OnAttack()
    {
        
    }

    private void OnHit()
    {

    }

    public void OnBulletDestroy(Vector3 bulletPos)
    {
        if (BulletDestroySpawnObject)
            Instantiate(BulletDestroySpawnObject, bulletPos, Quaternion.identity);
    }
}