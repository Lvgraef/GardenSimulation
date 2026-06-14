using System;
using System.Collections.Generic;
using gardensettings;
using GridSystem;
using TMPro;
using UI;
using UnityEngine;
using Material = GridSystem.Material;

namespace Data
{
    public class GardenDataManager : MonoBehaviour
    {
       
        [SerializeField] private GridManager gridManager;
        [SerializeField] private MaterialMenu menu;
        [SerializeField] private GardenSettings gardenSettings;
        
        private string gardenName =  "Garden";

        private Dictionary<Material.Category, Material> GardenMaterials;
        
        public void Awake()
        {
            GardenMaterials  = new Dictionary<Material.Category, Material>();
            foreach (var material in menu.Materials)
            {
                GardenMaterials.Add(material.category, material);
            }
        }
        
        
        //TODO: ADD PROMPT WHEN TRYING TO SAVE
        public void SaveGardenData()
        {
            // All tiles
            Tile [,] allTiles = gridManager.GetAllTiles();
            GardenSettingsDataModel gardenSettingsDataModel = new GardenSettingsDataModel(
                gardenSettings.Fertilizer,
                gardenSettings.CompostCleanup,
                gardenSettings.PlantDiversity,
                gardenSettings.FlyingInsects,
                gardenSettings.Birds,
                gardenSettings.Spiders,
                gardenSettings.OtherAnimals
            );

            // Have to link the name to whatever we are planning to get the name from
            GardenDataModel dataModel = new GardenDataModel(
                gardenName,
                gridManager.GetSize(),
                gardenSettingsDataModel
            );
  
            for (int x = 0; x < allTiles.GetLength(0); x++)
            {
                for (int y = 0; y < allTiles.GetLength(1); y++)
                {
                    Material material = allTiles[x, y].GetMaterial();
                    
                    if (material == null)
                    {
                               
                        dataModel.StoreMaterials(-1, (x, y));
                    }
                    else
                    {
                               
                        dataModel.StoreMaterials((int)material.category, (x, y));
                    }
             
                }
            }

            if (!Datainterface.SaveGardenData(dataModel))
            {
                Debug.LogError("Failed to save garden!");
            }
        }
        
        //TODO: ADD PROMPT WHEN TRYING TO READ
        public void GetGardenData()
        {
            // Have to link the name to whatever we are planning to get the name from
            var gardenData = Datainterface.GetGardenData(gardenName);

            if (gardenData == null)
            {
                Debug.LogError("Failed to find garden!");
                return;
            }

            
            Debug.Log(gardenData);
            
       

            if (gardenData.Materials == null)
            {
                Debug.LogError("Failed to find garden materials!");
                return;
            }
            

            gridManager.CreateGrid(gardenData.GridSize.Width, gardenData.GridSize.Height);
            gardenSettings.LoadFrom(gardenData.GardenSettings);
            
            for (int x = 0; x < gardenData.Materials.GetLength(0); x++)
            {
                for (int y = 0; y < gardenData.Materials.GetLength(1); y++)
                {
                    int tile = gardenData.Materials[x, y];
                    if(tile == -1) continue;
                    Material material = GardenMaterials[(Material.Category)tile];
                    var gridPos = (x, y);
                    gridManager.PlaceMaterial(material, gridPos);
                }
            }
        }


        
    }
}