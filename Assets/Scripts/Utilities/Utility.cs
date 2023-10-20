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
        if (damageInfo.Damages != null) 
        {
            foreach (var damage in damageInfo.Damages)
            {
                damages += damage.Type.ToString() + " (" + damage.Duration +
                    "s, " + damage.Amount + ")";
            }
        }
        
        string statusEffects = System.String.Empty;
        if (damageInfo.StatusEffects != null)
        {
            foreach (var effect in damageInfo.StatusEffects)
            {
                statusEffects += effect.Effect.ToString() + " (" + effect.Duration +
                    "s, " + effect.Strength + ")";
            }
        }

        Debug.Log("[Dealer ID] " + damageInfo.DamageSource +
            " [Receiver] " + receiver + " [Metal Type] " + damageInfo.AbilityMetalType.ToString() +
            " [Damage Types] " + damages + " [Status Effects] " + statusEffects);
    }

    public static float GetLongestParticleDuration(GameObject obj)
    {
        if (obj == null) return 0;

        // Try get particle systems
        var particleSystems = obj.GetComponents<ParticleSystem>();
        if (particleSystems.Length == 0) return 0;

        // Find the longest duration
        float duration = 0.0f;
        foreach (var particle in particleSystems)
        {
            if (particle.main.loop) continue;
            duration = Mathf.Max(duration, particle.main.duration + particle.main.startLifetime.constant);
        }

        return duration;
    }
}
