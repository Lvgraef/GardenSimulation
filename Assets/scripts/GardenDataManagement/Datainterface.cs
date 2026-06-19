using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace GardenDataManagement
{
    public class Datainterface : MonoBehaviour
    {
        
        private const string GardenFolderName = "GardenData";

        private static string GetGardenFolder()
        {
            string folder = Path.Combine(Application.persistentDataPath, GardenFolderName);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder; 
        }
        
        private static string GetGardenPath(string gardenName)
        {
            return Path.Combine(GetGardenFolder(), $"{gardenName}.json");
        }
        
        public static bool GardenExists(string gardenName)
        {
            return File.Exists(GetGardenPath(gardenName));
        }
            
            
        public static bool SaveGardenData(GardenDataModel gardenData)
        {
            string path = GetGardenPath(gardenData.GardenName);
            try
            {
                var data = JsonConvert.SerializeObject(gardenData, Formatting.Indented);
                File.WriteAllText(path, data);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            
        }

        public static GardenDataModel GetGardenData(string gardenName)
        {
            string path = GetGardenPath(gardenName);
            if (!File.Exists(path)) return null;

            try
            {
                string file = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<GardenDataModel>(file);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            
        }
        
        public static string[] GetAllGardenNames()
        {
            string folder = GetGardenFolder();
            string[] files = Directory.GetFiles(folder, "*.json");
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(files[i]);
            }
            return files;
        }
    }

}