// using UnityEditor;
//
// [CustomEditor(typeof(ActiveLegacySO), true)]
// public class ActiveLegacySOEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         // Get a reference to the target Scriptable Object
//         ActiveLegacySO legacySO = (ActiveLegacySO)target;
//         serializedObject.Update();
//
//         // Conditionally display additional fields if has additional status effect
//         if (legacySO.ExtraStatusEffect == EStatusEffect.None)
//             DrawPropertiesExcluding(serializedObject, "ExtraStatusEffectDurations", "ExtraStatusEffectStrengths", "ExtraStatusEffectProbabilites");
//         else
//             DrawDefaultInspector();
//
//         // Apply changes to serialized properties
//         serializedObject.ApplyModifiedProperties();
//     }
// }