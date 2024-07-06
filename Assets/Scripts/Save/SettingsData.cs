[System.Serializable]
public class SettingsData_Sound
{ 
    public bool isMuted;
    public float masterVolume;
    public float musicVolume;
    public float enemyVolume;
    public float othersVolume;
    public float uiVolume;
    
    public SettingsData_Sound Clone()
    {
        return (SettingsData_Sound)MemberwiseClone();
    }
}

public class SettingsData_General
{
    public bool IsFullscreen;
    public int ResolutionIndex;
    public ELocalisation Localisation;
    
    public SettingsData_General Clone()
    {
        return (SettingsData_General)MemberwiseClone();
    }
}