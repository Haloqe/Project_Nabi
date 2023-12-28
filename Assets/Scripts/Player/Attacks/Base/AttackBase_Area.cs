using UnityEngine;

public class AttackBase_Area : AttackBase
{
    private Legacy_Area _activeLegacy;
    private float _areaRadius;
    private float _areaRadiusMultiplier;
    [SerializeField] private GameObject _bomb;

    public override void Initialise()
    {
        base.Initialise();
        _isAttached = false;
        _vfxObject = Utility.LoadGameObjectFromPath("Prefabs/Player/AttackVFXs/Area_Default");
    }

    public override void Reset()
    {
        base.Reset();
        _areaRadiusMultiplier = 1f;
        _activeLegacy = null;
        _attackPostDelay = 0.0f;
    }

    public override void Attack()
    {
        Debug.Log("AttackBase_Area");
        _animator.SetInteger("AttackIndex", (int)ELegacyType.Area);

        // Play VFX
        float dir = Mathf.Sign(gameObject.transform.localScale.x);
        Vector3 playerPos = gameObject.transform.position;
        Vector3 vfxPos = _vfxObject.transform.position;
        Vector3 position = new Vector3(playerPos.x + dir * (vfxPos.x), playerPos.y + vfxPos.y, playerPos.z + vfxPos.z);

        _vfxObject.transform.localScale = new Vector3(dir, 1.0f, 1.0f);
        Instantiate(_vfxObject, position, Quaternion.identity);
    }
}