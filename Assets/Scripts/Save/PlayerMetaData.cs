[System.Serializable]
public class PlayerMetaData
{
    public bool isDirty = false;
    public int numDeaths = 0;
    public int numKills = 0;
    public int numSouls = 0;
    public int[] metaUpgradeLevels = new int[]{-1,-1,-1,-1,-1};
}
