using System;
using System.Collections.Generic;
using calculation;
using gardensettings;
using GridSystem;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Material = GridSystem.Material;
using Random = UnityEngine.Random;

namespace ai
{
    public class GridAgent : Agent
    {
        [SerializeField] private GridManager grid;
        [SerializeField] private GardenSettings gardenSettings;
        [SerializeField] private Material[] randomizedMaterials;
        [SerializeField] private Material buildingMaterial;

        private Calculator _calculator;

        private int _preFilled;

        private const int MaxWidth = 20;
        private const int MaxHeight = 20;

        private Dictionary<string, int> _materialAreaCounts = new();

        private void PreFillGridRandomly(Material[] material, Func<int, int> countFunction)
        {
            List<(int, int)> emptyTiles = new();

            grid.ForEachTile((tile, x, y) =>
            {
                if (tile.GetMaterial() is null)
                {
                    emptyTiles.Add((x, y));
                }
            });
            
            List<Material> pickableMaterials = new();

            foreach (var mat in material)
            {
                pickableMaterials.Add(mat);
            }

            foreach (var _ in material)
            {
                var pickedIndex = Random.Range(0, pickableMaterials.Count);
                var pickedMaterial = pickableMaterials[pickedIndex];
                var count = countFunction(emptyTiles.Count);
                int location = Random.Range(0, emptyTiles.Count);

                for (int i = 0; i < count; i++)
                {
                    grid.PlaceMaterial(pickedMaterial, emptyTiles[location]);
                    emptyTiles.RemoveAt(location);
                }

                pickableMaterials.RemoveAt(pickedIndex);
            }
        }

        private void PreFillBuildings()
        {
            PreFillGridRandomly(new[] { buildingMaterial }, _ => Random.Range(0, 50));
        }

        private void PreFillMaterials()
        {
            float bias = 5f;
            PreFillGridRandomly(randomizedMaterials, empty =>
                Mathf.FloorToInt(Mathf.Pow(Random.value, bias) * empty));
        }

        private void PickAreas()
        {
            float bias = 3f;
            var size = 0;

            grid.ForEachTile((tile, _, _) =>
            {
                if (tile.GetMaterial() is null)
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
                _materialAreaCounts.Add(pickableMaterials[materialIndex].materialName, count);
                pickableMaterials.RemoveAt(materialIndex);
            }
        }

        private void RandomizeSettings()
        {
            gardenSettings.Birds = Random.Range(0, 2) == 1;
            gardenSettings.FlyingInsects = Random.Range(0, 2) == 1;
            gardenSettings.Spiders = Random.Range(0, 2) == 1;
            gardenSettings.OtherAnimals = Random.Range(0, 2) == 1;

            gardenSettings.CompostCleanup = (CompostCleanup)Random.Range(0, 3);
            gardenSettings.PlantDiversity = (PlantDiversity)Random.Range(0, 5);
            gardenSettings.Fertilizer = (Fertilizer)Random.Range(0, 3);
        }

        public override void OnEpisodeBegin()
        {
            grid.Reset();
            PreFillBuildings();
            PreFillMaterials();
            PickAreas();
            RandomizeSettings();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            for (int i = 0; i < MaxWidth; i++)
            {
                for (int j = 0; j < MaxHeight; j++)
                {
                    switch (grid.GetMaterialName(i, j))
                    {
                        case "Building":
                            sensor.AddObservation(0);
                            break;
                        case "Bush":
                            sensor.AddObservation(1);
                            break;
                        case "Flower":
                            sensor.AddObservation(2);
                            break;
                        case "Grass":
                            sensor.AddObservation(3);
                            break;
                        case "Tree":
                            sensor.AddObservation(4);
                            break;
                        case "Water":
                            sensor.AddObservation(5);
                            break;
                    }
                }
            }

            foreach (var materialAreaCount in _materialAreaCounts)
            {
                sensor.AddObservation(materialAreaCount.Value);
            }

            sensor.AddObservation(gardenSettings.Birds);
            sensor.AddObservation(gardenSettings.FlyingInsects);
            sensor.AddObservation(gardenSettings.Spiders);
            sensor.AddObservation(gardenSettings.OtherAnimals);
            sensor.AddObservation((float)gardenSettings.CompostCleanup);
            sensor.AddObservation((float)gardenSettings.Fertilizer);
            sensor.AddObservation((float)gardenSettings.PlantDiversity);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            Dictionary<int, int> placements = new();
            
            for (int i = 0; i < MaxWidth; i++)
            {
                for (int j = 0; j < MaxHeight; j++)
                {
                    int index = i * MaxWidth + j;
                    var placementAction = actions.DiscreteActions[index];
                    if (placementAction == 0) continue;
                    grid.PlaceMaterial(randomizedMaterials[placementAction + 1], (i, j));
                }
            }

            
            
            var calculationResult = _calculator.Calculate().CalculationResult;

            AddReward(calculationResult.AnimalScore + calculationResult.PlantScore + calculationResult.SoilScore +
                      calculationResult.WaterScore);
        }

        protected override void Awake()
        {
            _calculator = new Calculator(BasicCalculationModel.Instance, grid, gardenSettings);
        }
    }
}