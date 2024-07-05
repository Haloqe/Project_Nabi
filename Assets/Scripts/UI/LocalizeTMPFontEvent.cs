using UnityEngine;
using UnityEngine.Events;
using System;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

[Serializable]
public class LocalizedTMPFont : LocalizedAsset<TMP_FontAsset> { }
[Serializable]
public class UnityEventTMPFont : UnityEvent<TMP_FontAsset> { }
 
[AddComponentMenu("Localization/Asset/Localize TMP Font Event")]
public class LocalizeTMPFontEvent : LocalizedAssetEvent<TMP_FontAsset, LocalizedTMPFont, UnityEventTMPFont>
{
#if UNITY_EDITOR
    void OnValidate()
    {
        if(OnUpdateAsset.GetPersistentEventCount()>0) return;
        TextMeshProUGUI target = gameObject.GetComponent<TextMeshProUGUI>();
        var setStringMethod = target.GetType().GetProperty("font").GetSetMethod();
        var methodDelegate = Delegate.CreateDelegate(typeof(UnityAction<TMP_FontAsset>), target, setStringMethod) as UnityAction<TMP_FontAsset>;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(OnUpdateAsset, methodDelegate);
        OnUpdateAsset.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
    }
#endif
}
 
[Serializable]
internal class UnityEventMaterial : UnityEvent<Material>
{
}
internal class LocalizeFontMaterialEvent
    : LocalizedAssetEvent<Material, LocalizedMaterial, UnityEventMaterial>
{
}