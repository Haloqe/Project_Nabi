using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttackSpawnObject))]
public class AttackSpawnObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get a reference to the target 
        AttackSpawnObject obj = target as AttackSpawnObject;
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("IsAttached"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ShouldManuallyDestroy"));
        
        // Conditionally display additional fields
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ShouldInflictDamage"));
        if (obj.ShouldInflictDamage)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageInfo"), true);
        }
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ShouldInflictStatusEffect"));
        if (obj.ShouldInflictDamage)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StatusEffectInfos"), true);
        }
        
        // Apply changes to serialized properties
        serializedObject.ApplyModifiedProperties();
    }       
}