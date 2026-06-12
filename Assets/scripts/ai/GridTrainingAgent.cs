using System;
using System.Collections.Generic;
using calculation;
using gardensettings;
using GridSystem;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ai
{
    public class GridTrainingAgent : Agent
    {
        [SerializeField] private GridManager grid;
        [SerializeField] private GardenSettings gardenSettings;
        private IMaterial[] _randomizedMaterials;
        private IMaterial _buildingMaterial;

        private Calculator _calculator;

        private int _preFilled;
        private int _empty;

        private const int MaxWidth = 20;
        private const int MaxHeight = 20;

        private Dictionary<string, int> _stringToIntID = new()
        {
            { "Bush", 0 },
            { "Flowers", 1 },
            { "Grass", 2 },
            { "Tree", 3 },
            { "Water", 4 }
        };

        private Dictionary<int, int> _materialAreaCounts = new();

        private void PreFillGridRandomly(IMaterial[] material, Func<int, int> countFunction)
        {
            List<(int, int)> emptyTiles = new();

            grid.ForEachTile((tile, x, y) =>
            {
                if (tile is null)
                {
                    emptyTiles.Add((x, y));
                }
            });

            List<IMaterial> pickableMaterials = new();

            foreach (var mat in material)
            {
                pickableMaterials.Add(mat);
            }

            foreach (var _ in material)
            {
                var pickedIndex = Random.Range(0, pickableMaterials.Count);
                var pickedMaterial = pickableMaterials[pickedIndex];
                var count = countFunction(emptyTiles.Count);

                for (int i = 0; i < count; i++)
                {
                    int location = Random.Range(0, emptyTiles.Count);
                    grid.PlaceMaterial(pickedMaterial, emptyTiles[location]);
                    emptyTiles.RemoveAt(location);
                }

                pickableMaterials.RemoveAt(pickedIndex);
            }
        }

        private void PreFillBuildings()
        {
            PreFillGridRandomly(new[] { _buildingMaterial }, _ => Random.Range(0, 50));
        }

        private void PreFillMaterials()
        {
            PreFillGridRandomly(_randomizedMaterials, empty => Mathf.FloorToInt(Random.Range(0, empty * 0.07f)));
        }

        private void PickAreas()
        {
            _materialAreaCounts.Clear();

            float bias = 3f;
            var size = 0;

            grid.ForEachTile((tile, _, _) =>
            {
                if (tile is null)
                {
                    size++;
                }
            });

            List<IMaterial> pickableMaterials = new();

            foreach (var mat in _randomizedMaterials)
            {
                pickableMaterials.Add(mat);
            }

            for (int i = 0; i < _randomizedMaterials.Length; i++)
            {
                int materialIndex = Random.Range(0, pickableMaterials.Count);
                int count = Mathf.FloorToInt(Mathf.Pow(Random.value, bias) * size);
                size -= count;
                _materialAreaCounts.Add(_stringToIntID[pickableMaterials[materialIndex].MaterialName], count);
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

            List<(int, int)> emptyTiles = new();

            grid.ForEachTile((tile, x, y) =>
            {
                if (tile is null)
                {
                    emptyTiles.Add((x, y));
                }
            });

            _empty = emptyTiles.Count;
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
                            sensor.AddObservation(1);
                            break;
                        case "Bush":
                            sensor.AddObservation(2);
                            break;
                        case "Flowers":
                            sensor.AddObservation(3);
                            break;
                        case "Grass":
                            sensor.AddObservation(4);
                            break;
                        case "Tree":
                            sensor.AddObservation(5);
                            break;
                        case "Water":
                            sensor.AddObservation(6);
                            break;
                        case null:
                            sensor.AddObservation(0);
                            break;
                        default:
                            throw new ArgumentException("Unknown material name: " + grid.GetMaterialName(i, j));
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

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            base.Heuristic(in actionsOut);
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
                    grid.PlaceMaterial(_randomizedMaterials[placementAction - 1], (i, j));
                    placements[placementAction - 1] = placements.GetValueOrDefault(placementAction - 1, 0) + 1;
                }
            }

            foreach (var keyValuePair in placements)
            {
                // todo maybe look at these rewards if they are efficient or if it should guide it more towards the 0 point.
                if (_materialAreaCounts[keyValuePair.Key] == keyValuePair.Value)
                {
                    AddReward(0.5f);
                }
                else
                {
                    AddReward(-0.5f);
                }

                break;
            }

            AddReward(Math.Abs(placements.Count - _empty) * -0.01f);

            var calculationResult = _calculator.Calculate().CalculationResult;

            AddReward(calculationResult.AnimalScore + calculationResult.PlantScore + calculationResult.SoilScore +
                      calculationResult.WaterScore);
        }

        protected override void Awake()
        {
            _calculator = new Calculator(BasicCalculationModel.Instance, grid, gardenSettings);

            _randomizedMaterials = new IMaterial[]
            {
                new VirtualMaterial("Bush", MaterialCategory.Shrubs),
                new VirtualMaterial("Flowers", MaterialCategory.Flowers),
                new VirtualMaterial("Grass", MaterialCategory.Grass),
                new VirtualMaterial("Tree", MaterialCategory.Tree),
                new VirtualMaterial("Water", MaterialCategory.NonPermeable),
            };

            _buildingMaterial = new VirtualMaterial("Building", MaterialCategory.Building);
        }
    }
}