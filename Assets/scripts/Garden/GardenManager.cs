using System;
using System.Collections.Generic;
using GridSystem;
using TMPro;
using UI;
using UnityEngine;
using Material = GridSystem.Material;

namespace Data
{
    public class GardenManager : MonoBehaviour
    {
       
        [SerializeField] private GridManager gridManager;
        [SerializeField] private MaterialMenu menu;
        
        
     

        private Dictionary<Material.Category, Material> GardenMaterials;
        
        public void Start()
        {
            GardenMaterials  = new Dictionary<Material.Category, Material>();
            foreach (var material in menu.Materials)
            {
                GardenMaterials.Add(material.category, material);
            }
        }
        
        

        public void SaveGarden()
        {
            // All tiles
            Tile [,] allTiles = gridManager.GetAllTiles();
            GardenDataModel dataModel = new GardenDataModel("", gridManager.GetSize());
  
            for (int x = 0; x < allTiles.GetLength(0); x++)
            {
                for (int y = 0; y < allTiles.GetLength(1); y++)
                {
                    dataModel.StoreMaterials();
                }
            }
        }
        
        public void GetGarden()
        {
            // get garden data
            var GardenData = DataManager.GetGardenData("Garden");
            
            gridManager.CreateGrid(GardenData.GridSize.Width, GardenData.GridSize.Height);
            
            for (int x = 0; x < GardenData.Materials.GetLength(0); x++)
            {
                for (int y = 0; y < GardenData.Materials.GetLength(1); y++)
                {
                    Material.Category tile = GardenData.Materials[x, y];
                    Material material = GardenMaterials[tile];
                    var gridPos = (x, y);
                    gridManager.PlaceMaterial(material, gridPos);
                }
            }

        }
        
    }
}