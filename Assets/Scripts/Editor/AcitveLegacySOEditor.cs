using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

[CustomEditor(typeof(ActiveLegacySO), true)]
public class ActiveLegacySOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        List<string> variables = new List<string>();
        foreach(FieldInfo field in typeof(LegacySO).GetFields())
        {
            variables.Add(field.Name);
        }
        DrawPropertiesExcluding(serializedObject, variables.ToArray());
        serializedObject.ApplyModifiedProperties();
    }
}