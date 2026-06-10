using System.Collections.Generic;
using gardensettings;
using GridSystem;
using Unity.MLAgents;
using UnityEngine;
using Material = GridSystem.Material;

namespace ai
{
    public class GridAgent : Agent
    {
        [SerializeField] private GridManager grid;
        [SerializeField] private GardenSettings gardenSettings;
        [SerializeField] private Material[] randomizedMaterials;
        [SerializeField] private Material buildingMaterial;
        
        private int _preFilled;

        private const int MaxWidth = 20;
        private const int MaxHeight = 20;
        
        Dictionary<string, int> materialAreaCounts = new();
        
        private void PreFillGridRandomly(Material material, int count)
        {
            List<(int, int)> emptyTiles = new();
            
            grid.ForEachTile((tile, x, y) =>
            {
                if (tile is null)
                {
                    emptyTiles.Add((x, y));
                }
            });
            
            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, emptyTiles.Count);
                grid.PlaceMaterial(material, emptyTiles[index]);
            }
        }

        private void PreFillBuildings()
        {
            PreFillGridRandomly(buildingMaterial, Random.Range(0, 50));
        }
        
        private void PreFillMaterials()
        {
            int materialIndex = Random.Range(0, randomizedMaterials.Length);
            float bias = 3f;
            var gridSize = MaxHeight * MaxWidth;
            _preFilled = Mathf.FloorToInt(Mathf.Pow(Random.value, bias) * gridSize);
            PreFillGridRandomly(randomizedMaterials[materialIndex], _preFilled);
        }

        private void PickAreas()
        {
            float bias = 3f;
            int size = 0;
            
            grid.ForEachTile((tile, x, y) =>
            {
                if (tile is null)
                {
                    size++;
                }
            });
            
            List<Material> pickableMaterials = new();
            
            foreach (Material mat in randomizedMaterials)
            {
                pickableMaterials.Add(mat);
            }
        
            for (int i = 0; i < randomizedMaterials.Length; i++)
            {
                int materialIndex = Random.Range(0, pickableMaterials.Count);
                int count = Mathf.FloorToInt(Mathf.Pow(Random.value, bias) * size);
                size -= count;
                materialAreaCounts.Add(pickableMaterials[materialIndex].materialName, count);
                pickableMaterials.RemoveAt(materialIndex);
            }
        }

        private void RandomizeSettings()
        {
            gardenSettings.Birds = Random.Range(0, 2) == 1;
            gardenSettings.FlyingInsects = Random.Range(0, 2) == 1;
            gardenSettings.Spiders = Random.Range(0, 2) == 1;
            gardenSettings.OtherAnimals = Random.Range(0, 2) == 1;
            
            gardenSettings.CompostCleanup = (CompostCleanup) Random.Range(0, 3);
            gardenSettings.PlantDiversity = (PlantDiversity) Random.Range(0, 5);
            gardenSettings.Fertilizer = (Fertilizer) Random.Range(0, 3);
        }
        
        public override void OnEpisodeBegin()
        {
            grid.Reset();
            PreFillBuildings();
            PreFillMaterials();
            PickAreas();
            RandomizeSettings();
        }
    }
}