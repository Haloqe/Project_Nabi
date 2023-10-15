using Enums.PlayerEnums;
using Structs.PlayerStructs;
using UnityEngine;

namespace Player.Abilities.Base
{
    public abstract class AbilityBase : MonoBehaviour
    {
        public SAbilityData _data { get; set; }
        protected EAbilityState _state;

        public virtual void Init()
        {
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
    }
}
