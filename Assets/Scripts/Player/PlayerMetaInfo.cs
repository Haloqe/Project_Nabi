public class PlayerMetaInfo
{
    public int NumDeaths;
    public int NumKills;
    public int NumSouls;
    public int[] MetaUpgradeLevels;

    public void Reset()
    {
        NumDeaths = 0;
        NumKills = 0;
        NumSouls = 0;
        MetaUpgradeLevels = new int[]{-1,-1,-1,-1,-1};
    }
}
