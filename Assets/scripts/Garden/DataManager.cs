using System.IO;
using Data;
using UnityEngine;

namespace Data
{
    public class DataManager : MonoBehaviour
    {
        public static bool SaveGardenData(GardenDataModel gardenData)
        {
            // Check if data entry exist (Create new if it doesn't
            string json = JsonUtility.ToJson(gardenData);
            string PATH = Application.persistentDataPath + $"/{gardenData.GardenName}.json";
            File.WriteAllText(PATH, json);
            
            return true;
        }

        public static GardenDataModel GetGardenData(string gardenName)
        {
            // parse garden data (check if not null)
            string path = Application.persistentDataPath + $"/{gardenName}.json";
            if (!File.Exists(path)) return null;
            
            string json = File.ReadAllText(path);
            GardenDataModel gardenData = JsonUtility.FromJson<GardenDataModel>(json);
            
            return gardenData;
        }
    }

}