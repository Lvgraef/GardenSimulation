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

        [SerializeField]
        private int maxWidth = 10;
        [SerializeField]
        private int maxHeight = 10;

        private float _previousCalculationResult;

        private int _gridIndex;

        private List<int> _materialAreaCounts = new(5);
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

            _pickableMaterials.Clear();

            float bias = 3f;
            var size = 0;

            grid.ForEachTile((tile, _, _) =>
            {
                if (tile.GetMaterial() is null)
                {
                    size++;
                }
            });

            foreach (var mat in _randomizedMaterials)
            {
                _pickableMaterials.Add(mat);
            }

            for (int i = 0; i < _randomizedMaterials.Length; i++)
            {
                int materialIndex = Random.Range(0, _pickableMaterials.Count);

                if (Random.Range(0, 10) == 0)
                {
                    _materialAreaCounts[materialIndex] = int.MinValue;
                    _pickableMaterials.RemoveAt(materialIndex);
                    continue;
                }

                int count = Mathf.FloorToInt(Mathf.Pow(Random.value, bias) * size);
                size -= count;
                _materialAreaCounts[_pickableMaterials[materialIndex].ID] = count;
                _pickableMaterials.RemoveAt(materialIndex);
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
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            for (int i = 0; i < maxWidth; i++)
            {
                for (int j = 0; j < maxHeight; j++)
                {
                    sensor.AddObservation(grid.GetMaterial(i, j)?.ID ?? -1);
                }
            }

            foreach (var materialAreaCount in _materialAreaCounts)
            {
                sensor.AddObservation(materialAreaCount);
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
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            if (_gridIndex >= maxWidth * maxHeight)
            {
                EndEpisode();
                return;
            }
            
            int? currentMaterial = grid.GetMaterialId(_gridIndex, maxWidth, maxHeight);

            while (_gridIndex < maxWidth * maxHeight && currentMaterial == -1)
            {
                _gridIndex++;
                currentMaterial = grid.GetMaterialId(_gridIndex, maxWidth, maxHeight);
            }

            var placementAction = actions.DiscreteActions[0];

            if (currentMaterial is null)
            {
                grid.PlaceMaterial(_randomizedMaterials[placementAction], _gridIndex, maxWidth, maxHeight);
                _empty--;
                
                if (_materialAreaCounts[placementAction] != int.MinValue)
                {
                    _materialAreaCounts[placementAction] -= 1;
                }
            }
            
            _gridIndex++;
            
            var calculationResult = _calculator.Calculate().CalculationResult;
            var result = (calculationResult.AnimalScore + calculationResult.PlantScore + calculationResult.SoilScore +
                          calculationResult.WaterScore) / 100;
            AddReward(result - _previousCalculationResult);
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

            EndEpisode();
        }

        protected override void Awake()
        {
            _calculator = new Calculator(BasicCalculationModel.Instance, grid, gardenSettings);

            _randomizedMaterials = new IMaterial[]
            {
                new VirtualMaterial("Bush", MaterialCategory.Shrubs, 0),
                new VirtualMaterial("Flowers", MaterialCategory.Flowers, 1),
                new VirtualMaterial("Grass", MaterialCategory.Grass, 2),
                new VirtualMaterial("Tree", MaterialCategory.Tree, 3),
                new VirtualMaterial("Water", MaterialCategory.NonPermeable, 4),
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