using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PassiveLegacySO))]
public class PassiveLegacySOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get a reference to the target Scriptable Object
        PassiveLegacySO legacySO = target as PassiveLegacySO;
        serializedObject.Update();

        // Conditionally display additional fields based on BuffType
        switch (legacySO.BuffType)
        {
            case EBuffType.None:
                DrawPropertiesExcluding(serializedObject, "StatUpgrades", "StatusEffectUpgrades", "IncreaseMethod", "IncreaseAmounts");
                break;
            
            case EBuffType.StatusEffectUpgrade:
                DrawPropertiesExcluding(serializedObject, "StatUpgrades", "IncreaseMethod", "IncreaseAmounts");
                break;
            
            case EBuffType.StatUpgrade:
                DrawPropertiesExcluding(serializedObject, "StatusEffectUpgrades", "IncreaseMethod", "IncreaseAmounts");
                break;
            
            case EBuffType.EnemyItemDropRate:
            case EBuffType.EnemyGoldDropRate:
            case EBuffType.SpawnAreaIncrease:
            case EBuffType.HealEfficiency_Food:
                DrawPropertiesExcluding(serializedObject, "StatUpgrades", "StatusEffectUpgrades");
                break;
        }

        // Apply changes to serialized properties
        serializedObject.ApplyModifiedProperties();
    }
}