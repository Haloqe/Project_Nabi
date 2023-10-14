using Enums.PlayerEnums;
using UnityEngine;

namespace Player.Abilities.Base
{
    public abstract class AbilityBase : MonoBehaviour
    {
        // 이후 abilityInfo라는 struct로 합쳐버릴수도 있음
        public int Id                       = 0;
        public string Name_EN               = string.Empty;
        public string Name_KO               = string.Empty;
        public string Description_EN        = string.Empty;
        public string Description_KO        = string.Empty;
        public string Icon                  = string.Empty;

        public EAbilityMetalType MetalType  = EAbilityMetalType.Undefined;
        public EAbilityType Type            = EAbilityType.Undefined;
        public EAbilityState State          = EAbilityState.Ready;

        // 패시브 스킬에서도 사용할 수 있다고 판단하여 일단 baseClass에 포함 (e.g. n초마다 회복)
        // 이후 액티브 스킬에 국한된 변수일시 activeBase로 이동
        public float CoolDownTime = 0.0f;
        public float Lifetime = 0.0f;

        // status effects
        // vfx

        void Update()
        {
            if (Type == EAbilityType.Passive) return;
            switch (State)
            {
                case EAbilityState.Ready:
                
                    break;
                case EAbilityState.Active: 
                                
                    break;
                case EAbilityState.Cooldown:
                
                    break;
            }
        }

        public abstract void Activate();
    }
}
