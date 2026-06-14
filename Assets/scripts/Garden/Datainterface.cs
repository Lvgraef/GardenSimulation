using System;
using System.IO;
using Data;
using UnityEngine;
using Newtonsoft.Json;

namespace Data
{
    public class Datainterface : MonoBehaviour
    {
        public static bool SaveGardenData(GardenDataModel gardenData)
        {
            string path = Application.persistentDataPath + $"/{gardenData.GardenName}.json";
            try
            {
                
                using (var sw = new StreamWriter(path))
                {

                    var data = JsonConvert.SerializeObject(gardenData, Newtonsoft.Json.Formatting.Indented);
                    sw.Write(data);
                    sw.Flush();
                    sw.Close();

                    return true;
                }
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
                using (var reader = new StreamReader(path))
                {
                    string file = reader.ReadToEnd();
                    reader.Close();
                    return JsonConvert.DeserializeObject<GardenDataModel>(file);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }

}