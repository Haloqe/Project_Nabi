using Enums.PlayerEnums;

namespace Structs.PlayerStructs
{
    public struct SAbilityData
    {
        public int Id;
        public string ClassName;
        public string Name_EN;
        public string Name_KO;
        public string Des_EN;
        public string Des_KO;
        public string IconPath;
        public string PrefabPath;
        public bool IsAttached;
        public EInteraction Interaction; // 이후 slowtap duration, hold duration 등 필요해지면 string 또는 separated field 변경

        public EAbilityType Type;
        public EAbilityMetalType MetalType;
        public float CoolDownTime;
        public float LifeTime;

        SAbilityData(int id, string className, string name_en, string name_ko, string des_en, string des_ko, 
            string iconPath, string prefabPath, bool isAttached, EInteraction interaction, 
            EAbilityType type, EAbilityMetalType metalType, float cdTime, float lTime)
        {
            Id              = id;
            ClassName       = className;
            Name_EN         = name_en;
            Name_KO         = name_ko;
            Des_EN          = des_en;
            Des_KO          = des_ko;
            IconPath        = iconPath;
            PrefabPath      = prefabPath;
            IsAttached      = isAttached;
            Interaction     = interaction;
            Type            = type;
            MetalType       = metalType;
            CoolDownTime    = cdTime;
            LifeTime        = lTime;
        }
    }
}