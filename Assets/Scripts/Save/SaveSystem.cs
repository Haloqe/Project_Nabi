using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
        public static void SaveMetaData()
        {
                BinaryFormatter formatter = new BinaryFormatter();
                string path = Application.persistentDataPath + "/save";
                FileStream stream = new FileStream(path, FileMode.Create);
                formatter.Serialize(stream, GameManager.Instance.PlayerMetaData);
                stream.Close();
        }

        public static PlayerMetaData LoadMetaData()
        {
                string path = Application.persistentDataPath + "/save";
                if (File.Exists(path))
                {
                        BinaryFormatter formatter = new BinaryFormatter();
                        FileStream stream = new FileStream(path, FileMode.Open);
                        PlayerMetaData data = formatter.Deserialize(stream) as PlayerMetaData;
                        stream.Close();
                        return data;
                }
                else
                {
                        return new PlayerMetaData();
                }
        }

        public static void RemoveSaveData()
        {
                string path = Application.persistentDataPath + "/save";
                File.Delete(path);
        }
}