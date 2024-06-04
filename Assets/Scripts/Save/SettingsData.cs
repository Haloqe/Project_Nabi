[System.Serializable]
public class SettingsData_Sound
{ 
    public bool isMuted;
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public float uiVolume;
    
    public SettingsData_Sound Clone()
    {
        return (SettingsData_Sound)MemberwiseClone();
    }
}