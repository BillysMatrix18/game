using System.IO;
using UnityEngine;

namespace StarboundSprint.Systems
{
    [System.Serializable]
    public class SaveData
    {
        public int coins;
        public int lives;
        public int remainingTime;
    }

    public static class SaveSystem
    {
        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        public static void SaveProgress(SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }

        public static SaveData LoadProgress()
        {
            if (!File.Exists(SavePath))
            {
                return new SaveData();
            }

            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
    }
}
