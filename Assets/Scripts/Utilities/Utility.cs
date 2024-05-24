using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using ColorUtility = UnityEngine.ColorUtility;
using Object = UnityEngine.Object;

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

    public static void PrintDamageInfo(string receiver, AttackInfo attackInfo)
    {
        string damageInfo = string.Empty;
        if (attackInfo.Damage != null)
        {
            var damage = attackInfo.Damage;
            if (damage.Duration == 0)
            {
                damageInfo += damage.Type + " (" + damage.TotalAmount + ")";
            }
            else
            {
                float perTick = damage.TotalAmount / (damage.Duration / damage.Tick + 1);
                damageInfo += damage.Type + " (" + perTick + " / " + damage.Tick + "s for " + damage.Duration + "s)";
            }
        }
        
        string statusEffectInfo = string.Empty;
        if (attackInfo.StatusEffects != null)
        {
            foreach (var effect in attackInfo.StatusEffects)
            {
                statusEffectInfo += effect.Effect + " (" + effect.Duration + "s, " + effect.Strength + ") ";
            }
        }
        //Debug.Log("[Damage] " + damageInfo + " [Status Effects] " + statusEffectInfo);
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

    public static bool Compare(float left, EComparator comparator, float right)
    {
        switch (comparator)
        {
            case EComparator.IsLessThan:
                return left < right;
            case EComparator.IsLessOrEqualTo:
                return left <= right;
            case EComparator.IsEqualTo:
                return Math.Abs(left - right) < 0.0001f;
            case EComparator.IsGreaterOrEqualTo:
                return left >= right;
            case EComparator.IsGreaterThan:
                return left >= right;
            case EComparator.IncreasesBy:
                return Math.Abs(left - right) < 0.0001f;
        }
        return false;
    }

    public static string GetColouredPreservationText(ELegacyPreservation preserv)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGBA(Define.LegacyPreservationColors[(int)preserv])
            + ">[" + Define.LegacyPreservationNames[(int)Define.Localisation, (int)preserv] + "]</color>";
    }
    
    public static string GetColouredWarriorText(EWarrior warrior)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGBA(Define.WarriorMainColours[(int)warrior])
            + ">[" + Define.WarriorNames[(int)Define.Localisation, (int)warrior] + "]</color>";
    }
    
    public static string FormatFloat(float number)
    {
        if (number % 1 == 0)
        {
            // If the number has no decimal place
            return number.ToString("0");
        }
        else
        {
            // If the number has a decimal place, cut to first decimal place
            return number.ToString("0.0");
        }
    }
}
