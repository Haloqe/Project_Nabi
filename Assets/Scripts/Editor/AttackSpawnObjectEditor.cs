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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("IsAttachedToPlayer"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoDestroy"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("DealInterval"));
        
        // Conditionally display additional fields
        EditorGUILayout.PropertyField(serializedObject.FindProperty("HasDamage"));
        if (obj.HasDamage)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("relativeDamages"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShouldUpdateTension"), true);
        }
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("HasStatusEffect"));
        if (obj.HasStatusEffect)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StatusEffectInfos"), true);
        }
        
        // Apply changes to serialized properties
        serializedObject.ApplyModifiedProperties();
    }       
}