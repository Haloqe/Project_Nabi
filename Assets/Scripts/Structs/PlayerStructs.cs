using Enums.PlayerEnums;

namespace Structs.PlayerStructs
{
    // 생성될 클래스명 보관하는 변수 따로 만들지 고민. 그게 나을지도
    public struct SAbilityData
    {
        public string Name_EN;
        public string Name_KO;
        public string Des_EN;
        public string Des_KO;
        public string IconPath;
        public EAbilityType Type;
        public EAbilityMetalType MetalType;
        public float CoolDownTime;
        public float LifeTime;

        SAbilityData(string name_en, string name_ko, string des_en, string des_ko, string iconPath,
            EAbilityType type, EAbilityMetalType metalType, float cdTime, float lTime)
        {
            Name_EN         = name_en;
            Name_KO         = name_ko;
            Des_EN          = des_en;
            Des_KO          = des_ko;
            IconPath        = iconPath;
            Type            = type;
            MetalType       = metalType;
            CoolDownTime    = cdTime;
            LifeTime        = lTime;
        }
    }
}