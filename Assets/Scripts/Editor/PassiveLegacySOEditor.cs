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

        EditorGUILayout.PropertyField(serializedObject.FindProperty("BuffType"), true);
        // Conditionally display additional fields based on BuffType
        switch (legacySO.BuffType)
        {
            case EBuffType.None:
                break;
            
            case EBuffType.StatusEffectUpgrade:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("StatusEffectUpgrades"), true);
                break;
            
            case EBuffType.StatUpgrade:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("StatUpgrades"), true);
                break;
            
            case EBuffType.EnemyItemDropRate:
            case EBuffType.EnemyGoldDropRate:
            case EBuffType.SpawnAreaIncrease:
            case EBuffType.HealEfficiency_Food:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BuffIncreaseMethod"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BuffIncreaseAmounts"), true);
                break;
        }

        // Apply changes to serialized properties
        serializedObject.ApplyModifiedProperties();
    }
}