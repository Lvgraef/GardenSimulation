using System;
using System.IO;
using Data;
using UnityEngine;

namespace Data
{
    public class DataManager : MonoBehaviour
    {
        public static bool SaveGardenData(GardenDataModel gardenData)
        {
            string path = Application.persistentDataPath + $"/{gardenData.GardenName}.json";
            try
            {
                string json = JsonUtility.ToJson(gardenData);
            
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
                // throw;
            }
            
        }

        public static GardenDataModel GetGardenData(string gardenName)
        {
            // parse garden data (check if not null)
            string path = Application.persistentDataPath + $"/{gardenName}.json";
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                GardenDataModel gardenData = JsonUtility.FromJson<GardenDataModel>(json);
                return gardenData;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }

}