using System;
using UnityEngine;

namespace Player.Abilities.Base
{
    public abstract class AbilityBase : MonoBehaviour
    {
        protected PlayerCombat _owner;
        public SAbilityData _data { get; set; }
        protected EAbilityState _state;

        protected virtual void Start()
        {
            _owner = FindObjectOfType<PlayerCombat>();
            _state = EAbilityState.Ready;
        }

        public virtual void Activate()
        {
            if (_state != EAbilityState.Ready)
            {
                Debug.Log("[" + _data.Name_EN + "] under cooldown");
                return;
            }
            Debug.Log("[" + _data.Name_EN + "] activated");
            _state = EAbilityState.Active;
        }

        protected virtual void TogglePrefab(bool isActive)
        {
            gameObject.SetActive(isActive);
            if (isActive && !_data.IsAttached)
            {
                gameObject.transform.position = _owner.transform.position;
            }
        }
    }
}
