using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Utility
{
    public static UnityEngine.Object LoadObjectFromFile(string path)
    {
        var loadedObject = Resources.Load(path);
        if (loadedObject == null)
        {
            throw new FileNotFoundException("...no file found - please check the configuration");
        }
        return loadedObject;
    }

    public static bool IsObjectInList(GameObject obj, List<int> list)
    {
        int id = obj.GetInstanceID();
        foreach (int i in list)
        {
            if (i == id) return true;
        }
        return false;
    }

    public static void PrintDamageInfo(string receiver, SDamageInfo damageInfo)
    {
        string damages = System.String.Empty;
        foreach (var damage in damageInfo.Damages)
        {
            damages += damage.Type.ToString() + " (" + damage.Duration +
                "s, " + damage.Amount + ")";
        }
        string statusEffects = System.String.Empty;
        foreach (var effect in damageInfo.StatusEffects)
        {
            statusEffects += effect.Effect.ToString() + " (" + effect.Duration +
                "s, " + effect.Strength + ")";
        }

        Debug.Log("[Dealer ID] " + damageInfo.DamageSource +
            " [Receiver] " + receiver +
            "\n[Metal Type] " + damageInfo.AbilityMetalType.ToString() +
            " [Damage Types] " + damages +
            "\n[Status Effects] " + statusEffects);
    }
}
