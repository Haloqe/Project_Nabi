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
}
