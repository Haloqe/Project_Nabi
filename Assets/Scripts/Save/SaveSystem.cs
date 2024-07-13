using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
        public static void SaveMetaData()
        {
                var data = GameManager.Instance.PlayerMetaData;
                BinaryFormatter formatter = new BinaryFormatter();
                string path = Application.persistentDataPath + "/meta";
                FileStream stream = new FileStream(path, FileMode.Create);
                formatter.Serialize(stream, data);
                stream.Close();
                Debug.Log("Save: kills " + data.numKills + " deaths " + data.numDeaths + " souls: " + data.numSouls);
        }

        public static PlayerMetaData LoadMetaData()
        {
                string path = Application.persistentDataPath + "/meta";
                if (File.Exists(path))
                {
                        BinaryFormatter formatter = new BinaryFormatter();
                        FileStream stream = new FileStream(path, FileMode.Open);
                        PlayerMetaData data = formatter.Deserialize(stream) as PlayerMetaData;
                        stream.Close();
                        data.metaUpgradeLevelsTemporary = data.metaUpgradeLevels;
                        Debug.Log("Load: kills " + data.numKills + " deaths " + data.numDeaths + " souls: " + data.numSouls);
                        return data;
                }
                else
                {
                        Debug.Log("Load: no data");
                        return new PlayerMetaData();
                }
        }

        public static void RemoveMetaData()
        {
                string path = Application.persistentDataPath + "/meta";
                File.Delete(path);
        }
        
        public static void SaveSoundSettingsData()
        {
                BinaryFormatter formatter = new BinaryFormatter();
                string path = Application.persistentDataPath + "/sound";
                FileStream stream = new FileStream(path, FileMode.Create);
                formatter.Serialize(stream, AudioManager.Instance.SoundSettingsData);
                stream.Close();
        }

        public static SettingsData_Sound LoadSoundSettingsData()
        {
                string path = Application.persistentDataPath + "/sound";
                if (File.Exists(path))
                {
                        BinaryFormatter formatter = new BinaryFormatter();
                        FileStream stream = new FileStream(path, FileMode.Open);
                        SettingsData_Sound data = formatter.Deserialize(stream) as SettingsData_Sound;
                        stream.Close();
                        return data;
                }
                else
                {
                        return null;
                }
        }

        public static void SaveGeneralSettingsData()
        {
                BinaryFormatter formatter = new BinaryFormatter();
                string path = Application.persistentDataPath + "/general";
                FileStream stream = new FileStream(path, FileMode.Create);
                formatter.Serialize(stream, GameManager.Instance.GeneralSettingsData);
                stream.Close();
        }
        
        public static SettingsData_General LoadGeneralSettingsData()
        {
                string path = Application.persistentDataPath + "/general";
                if (File.Exists(path))
                {
                        BinaryFormatter formatter = new BinaryFormatter();
                        FileStream stream = new FileStream(path, FileMode.Open);
                        SettingsData_General data = formatter.Deserialize(stream) as SettingsData_General;
                        stream.Close();
                        return data;
                }
                else
                {
                        return null;
                }
        }
}