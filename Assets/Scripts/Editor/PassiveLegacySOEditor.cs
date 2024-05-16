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
            case EBuffType.EuphoriaEcstasyUpgrade:
                break;
            
            case EBuffType.StatUpgrade:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("StatUpgradeData"), true);
                break;
            
            case EBuffType.EnemyItemDropRate:
            case EBuffType.SpawnAreaIncrease:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BuffIncreaseMethod"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BuffIncreaseAmounts"), true);
                break;
            
            case EBuffType.NightShadeFastChase:
            case EBuffType.BindingSkillUpgrade:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Stats"), true);
                break;
            
            case EBuffType.EuphoriaEnemyGoldDropBuff:
            case EBuffType.SommerHypHallucination:
            case EBuffType.TurbelaMaxButterfly:
            case EBuffType.TurbelaDoubleSpawn:
            case EBuffType.TurbelaButterflyCrit:
            case EBuffType.NightShadeShadeBonusStats:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Stats"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SavedInDefine"), true);
                break;
            
            case EBuffType.AttackDamageMultiply:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AttackType"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Stats"), true);
                break;
        }

        // Apply changes to serialized properties
        serializedObject.ApplyModifiedProperties();
    }
}