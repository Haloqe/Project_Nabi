// 그룹별로 안 나누고 Enums namespace로 통일도 고려중
namespace Enums.PlayerEnums
{
    #region Ability
    public enum EAbilityMetalType
    {
        Copper,
        Gold,
        Undefined,
    }
    public enum EAbilityType // AbilityType이 좀 모호한듯도 한데 괜찮은 이름 있으면 바꿔줘.. 괜찮으면 냅두고.
    {
        Passive,
        Active,
        Undefined,
    }
    public enum EAbilityState
    {
        Ready,
        Active,
        Cooldown,
    } 
    #endregion Ability
}