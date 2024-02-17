using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class Utility
{
    public static string GetLocalisationString(int index)
    {
        return ((ELocalisation)index).ToString();
    }

    public static bool LoadSceneByNameSafe(string sceneName)
    {
        int sceneIdx = SceneUtility.GetBuildIndexByScenePath(sceneName);
        var scene = SceneManager.GetSceneByBuildIndex(sceneIdx);
        if (scene.IsValid())
        {
            SceneManager.LoadScene(sceneIdx);
            return true;
        }
        return false;
    }

    public static Object LoadObjectFromPath(string path)
    {
        var loadedObject = Resources.Load(path);
        if (loadedObject == null)
        {
            //throw new FileNotFoundException("Object Load Failed " + path);
        }
        return loadedObject;
    }

    public static GameObject LoadGameObjectFromPath(string path)
    {
        Object obj = LoadObjectFromPath(path);
        if (obj != null)
        {
            return obj.GameObject();
        }
        return null;
    }

    public static Object[] LoadAllObjectsFromPath(string path)
    {
        var resources = Resources.LoadAll(path);
        Debug.AssertFormat(resources.Length > 0, "Objects not found");
        return resources;
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

    public static TileBase LoadTileFromPath(string path)
    {
        TileBase tile = (TileBase)LoadObjectFromPath("Tiles/" + path);
        Debug.Assert(tile != null, "Invalid tile path");
        return tile;
    }

    public static void PrintDamageInfo(string receiver, SDamageInfo damageInfo)
    {
        string damages = System.String.Empty;
        if (damageInfo.Damages != null) 
        {
            foreach (var damage in damageInfo.Damages)
            {
                if (damage.Duration == 0)
                {
                    damages += damage.Type.ToString() + " (" + damage.TotalAmount + ")";
                }
                else
                {
                    float perTick = damage.TotalAmount / (damage.Duration / damage.Tick + 1);
                    damages += damage.Type.ToString() + " (" + perTick +
                        " / " + damage.Tick + "s for " + damage.Duration + "s)";
                }
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

        Debug.Log("[Damage Types] " + damages + " [Status Effects] " + statusEffects);
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
    
    public static float GetChangedValue(float prevValue, float changeAmount, EIncreaseMethod changeMethod)
    {
        switch (changeMethod)
        {
            case EIncreaseMethod.Constant:
                return prevValue + changeAmount;
            
            case EIncreaseMethod.Percent:
                return prevValue + prevValue * changeAmount;
            
            case EIncreaseMethod.PercentPoint:
                return prevValue + changeAmount;
            
            default:
                return prevValue;
        }
    }
}
