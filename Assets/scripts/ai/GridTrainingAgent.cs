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
    public class GridTrainingAgent : Agent
    {
        [SerializeField] private GridManager grid;
        [SerializeField] private GardenSettings gardenSettings;
        private IMaterial[] _randomizedMaterials;
        private IMaterial _buildingMaterial;

        [SerializeField] private Material[] randomizedMaterialsObject;
        [SerializeField] private Material buildingMaterialObject;

        private Calculator _calculator;

        private int _preFilled;
        private int _empty;

        [SerializeField] private int maxWidth = 5;
        [SerializeField] private int maxHeight = 5;

        private float _previousCalculationResult;
        private float _firstScore;

        private int _gridIndex;

        private List<int> _materialAreaCounts = new(6);
        private List<(int, int)> _emptyTiles = new();
        private List<IMaterial> _pickableMaterials = new();

        private void PreFillGridRandomly(IMaterial[] material, Func<int, int> countFunction)
        {
            _emptyTiles.Clear();
            _pickableMaterials.Clear();

            grid.ForEachTile((tile, x, y) =>
            {
                if (tile.GetMaterial() is null)
                {
                    _emptyTiles.Add((x, y));
                }
            });


            foreach (var mat in material)
            {
                _pickableMaterials.Add(mat);
            }

            foreach (var _ in material)
            {
                var pickedIndex = Random.Range(0, _pickableMaterials.Count);
                var pickedMaterial = _pickableMaterials[pickedIndex];
                var count = countFunction(_emptyTiles.Count);

                for (int i = 0; i < count; i++)
                {
                    int location = Random.Range(0, _emptyTiles.Count);
                    grid.PlaceMaterial(pickedMaterial, _emptyTiles[location]);
                    _emptyTiles.RemoveAt(location);
                }

                _pickableMaterials.RemoveAt(pickedIndex);
            }
        }

        private void PreFillBuildings()
        {
            PreFillGridRandomly(new[] { _buildingMaterial }, _ => Random.Range(0, (maxWidth * maxHeight) / 8));
        }

        private void PreFillMaterials()
        {
            PreFillGridRandomly(_randomizedMaterials, empty => Mathf.FloorToInt(Random.Range(0, empty * 0.07f)));
        }

        private void PickAreas()
        {
            _materialAreaCounts.Clear();
            for (int i = 0; i < _randomizedMaterials.Length; i++)
            {
                _materialAreaCounts.Add(0);
            }

            int size = 0;
            grid.ForEachTile((tile, _, _) =>
            {
                if (tile.GetMaterial() is null)
                {
                    size++;
                }
            });

            List<IMaterial> activeMaterials = new List<IMaterial>();
            for (int i = 0; i < _randomizedMaterials.Length; i++)
            {
                if (Random.Range(0, 6) == 0)
                {
                    _materialAreaCounts[i] = int.MinValue;
                }
                else
                {
                    activeMaterials.Add(_randomizedMaterials[i]);
                }
            }

            
            float[] weights = new float[activeMaterials.Count];
            float totalWeight = 0;
            for (int i = 0; i < activeMaterials.Count; i++)
            {
                weights[i] = Random.value;
                totalWeight += weights[i];
            }

            int totalDistributed = 0;
            for (int i = 0; i < activeMaterials.Count - 1; i++)
            {
                int count = Mathf.RoundToInt((weights[i] / totalWeight) * size);
                _materialAreaCounts[activeMaterials[i].ID] = count;
                totalDistributed += count;
            }

            if (activeMaterials.Count == 0) return;
            
            int lastMaterialID = activeMaterials[^1].ID;
            _materialAreaCounts[lastMaterialID] = size - totalDistributed;
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
            _gridIndex = 0;
            _previousCalculationResult = 0;
            _materialAreaCounts.Clear();
            grid.Reset();
            PreFillBuildings();
            PreFillMaterials();
            PickAreas();
            RandomizeSettings();

            _emptyTiles.Clear();

            grid.ForEachTile((tile, x, y) =>
            {
                if (tile.GetMaterial() is null)
                {
                    _emptyTiles.Add((x, y));
                }
            });

            _empty = _emptyTiles.Count;
            
            var calculationResult = _calculator.Calculate(true).CalculationResult;
            var result = (calculationResult.AnimalScore + calculationResult.PlantScore + calculationResult.SoilScore +
                          calculationResult.WaterScore);
            _previousCalculationResult = result;
            _firstScore = result;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            for (int i = 0; i < maxWidth; i++)
            {
                for (int j = 0; j < maxHeight; j++)
                {
                    int id = grid.GetMaterial(i, j)?.ID ?? -1;
                    sensor.AddOneHotObservation(id + 1, _randomizedMaterials.Length + 1);
                }
            }

            foreach (var materialAreaCount in _materialAreaCounts)
            {
                float norm = materialAreaCount switch
                {
                    int.MinValue => -1f,
                    _ => (float)materialAreaCount / (maxWidth * maxHeight)
                };
                
                sensor.AddObservation(norm);
            }
            
            int currentX = _gridIndex % maxWidth;
            int currentY = _gridIndex / maxWidth;
            sensor.AddObservation((float)currentX / maxWidth);
            sensor.AddObservation((float)currentY / maxHeight);

            sensor.AddObservation(gardenSettings.Birds);
            sensor.AddObservation(gardenSettings.FlyingInsects);
            sensor.AddObservation(gardenSettings.Spiders);
            sensor.AddObservation(gardenSettings.OtherAnimals);
            sensor.AddOneHotObservation((int)gardenSettings.CompostCleanup, 3);
            sensor.AddOneHotObservation((int)gardenSettings.Fertilizer, 3);
            sensor.AddOneHotObservation((int)gardenSettings.PlantDiversity, 5);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            bool constraint = true;
            
            if (_gridIndex >= maxWidth * maxHeight)
            {
                EndEpisode();
                return;
            }

            int? currentMaterial = grid.GetMaterialId(_gridIndex, maxWidth, maxHeight);

            while (_gridIndex < maxWidth * maxHeight && currentMaterial is not null)
            {
                _gridIndex++;
                if (_gridIndex >= maxWidth * maxHeight)
                {
                    EndEpisode();
                    return;
                };
                currentMaterial = grid.GetMaterialId(_gridIndex, maxWidth, maxHeight);
            }

            var placementAction = actions.DiscreteActions[0];

            if (currentMaterial is null)
            {
                int currentBudget = _materialAreaCounts[placementAction];

                if (currentBudget > 0)
                {
                    grid.PlaceMaterial(_buildingMaterial, _gridIndex, maxWidth, maxHeight);
                    AddReward(0.1f);
                }
                else if (currentBudget != int.MinValue)
                {
                    grid.PlaceMaterial(_buildingMaterial, _gridIndex, maxWidth, maxHeight);
                    AddReward(-0.2f);
                    constraint = false;
                }
                else
                {
                    grid.PlaceMaterial(_randomizedMaterials[placementAction], _gridIndex, maxWidth, maxHeight);
                }

                _empty--;

                if (_materialAreaCounts[placementAction] != int.MinValue)
                {
                    _materialAreaCounts[placementAction] -= 1;
                }
            }


            _gridIndex++;

            var calculationResult = _calculator.Calculate(true).CalculationResult;
            
            var result = (calculationResult.AnimalScore + calculationResult.PlantScore + calculationResult.SoilScore +
                          calculationResult.WaterScore);
            if (constraint)
            {
                AddReward(0.5f * Math.Clamp(result - _previousCalculationResult, -0.2f, 0.2f));
            }
            _previousCalculationResult = result;

            if (_empty > 0) return;

            foreach (var value in _materialAreaCounts)
            {
                if (value == int.MinValue) continue;

                AddReward(Math.Abs(value) * (25f / (maxHeight * maxWidth)) * -0.05f);

                if (value == 0)
                {
                    AddReward(1f);
                }
            }
            
            var finalResult = _calculator.Calculate(true).CalculationResult;
            
            float totalEcoScore = finalResult.WaterScore + finalResult.SoilScore + finalResult.AnimalScore + finalResult.PlantScore - _firstScore;
            
            AddReward(0.1f * totalEcoScore); 

            EndEpisode();
        }

        protected override void Awake()
        {
            _calculator = new Calculator(BasicCalculationModel.Instance, grid, gardenSettings);

            _randomizedMaterials = new IMaterial[]
            {
                new VirtualMaterial("Water", MaterialCategory.NonPermeable, 0),
                new VirtualMaterial("Tile", MaterialCategory.NonPermeable, 1),
                new VirtualMaterial("Flowers", MaterialCategory.Flowers, 2),
                new VirtualMaterial("Grass", MaterialCategory.Grass, 3),
                new VirtualMaterial("Tree", MaterialCategory.Tree, 4),
                new VirtualMaterial("Bush", MaterialCategory.Shrubs, 5),
            };

            _buildingMaterial = new VirtualMaterial("Building", MaterialCategory.Building, -1);

            // _randomizedMaterials = new IMaterial[randomizedMaterialsObject.Length];
            //
            // for (var index = 0; index < randomizedMaterialsObject.Length; index++)
            // {
            //     _randomizedMaterials[index] = randomizedMaterialsObject[index];
            // }
            //
            // _buildingMaterial = buildingMaterialObject;
        }
    }
}