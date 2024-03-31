using UnityEditor;

[CustomEditor(typeof(PassiveLegacySO))]
public class PassiveLegacySOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get a reference to the target Scriptable Object
        PassiveLegacySO legacySO = target as PassiveLegacySO;
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("BuffType"), true);
        // Conditionally display additional fields based on BuffType
        switch (legacySO.BuffType)
        {
            case EBuffType.None:
                break;
            
            case EBuffType.StatUpgrade:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("StatUpgradeData"), true);
                break;
            
            case EBuffType.EnemyItemDropRate:
            case EBuffType.SpawnAreaIncrease:
            case EBuffType.FoodHealEfficiency:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BuffIncreaseMethod"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BuffIncreaseAmounts"), true);
                break;
            
            case EBuffType.EnemyGoldDropBuff:
            case EBuffType.DruggedEffectBuff:
            case EBuffType.HypHallucination:
                break;
            
            case EBuffType.TurbelaMaxButterfly:
            case EBuffType.TurbelaDoubleButterfly:
            case EBuffType.TurbelaButterflyCrit:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BuffIncreaseAmounts"), true);
                break;
        }

        // Apply changes to serialized properties
        serializedObject.ApplyModifiedProperties();
    }
}