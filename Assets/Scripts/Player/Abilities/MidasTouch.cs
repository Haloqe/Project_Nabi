using Player.Abilities.Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidasTouch : ActiveAbilityBase
{
    private List<int> _affectedEnemies;
    private int _maxTargetNum;
    private PlayerMovement _playerMovement;

    protected override void Initialise()
    {
        _affectedEnemies = new List<int>();
        _maxTargetNum = 1;
    }

    protected override void ActivateAbility()
    {
        _affectedEnemies.Clear();
    }

    protected override void EndAbility()
    {
        // Called during the termination of the ability
        // Do some jobs after the ability is finished
    }

    protected override void TogglePrefab(bool isActive)
    {
        gameObject.SetActive(isActive);
        if (isActive && !_data.IsAttached)
        {
            //these three lines of code flip the skill VFX prefab depending on the side that the player is facing
            Vector3 currentScale = gameObject.transform.localScale;
            currentScale.x *= -1;
            gameObject.transform.localScale = currentScale;

            gameObject.transform.position = _owner.transform.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") == false)
        {
            Debug.LogError(_data.Name_EN + " trigger with non-enemy");
            return;
        }
        if (_affectedEnemies.Count >= _maxTargetNum) return;
        if (Utility.IsObjectInList(collision.gameObject, _affectedEnemies)) return;

        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target == null) return;

        _affectedEnemies.Add(collision.gameObject.GetInstanceID());

        //TO-DO: Gold 상태이상 구현
        _owner.DealDamage(target, _data.DamageInfo);

    }

}