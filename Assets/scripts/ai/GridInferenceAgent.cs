using System;
using System.Collections.Generic;
using calculation;
using gardensettings;
using GridSystem;
using UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Material = GridSystem.Material;
using Random = UnityEngine.Random;

namespace ai
{
    public class GridInferenceAgent : Agent
    {
        public AIMenu aiMenu;
        public SubGrid Grid;
        public GridManager gridManager;
        [SerializeField] private GardenSettings gardenSettings;
        private IMaterial[] _randomizedMaterials;

        [SerializeField] private Material[] randomizedMaterialsObject;
        [SerializeField] private Material buildingMaterialObject;

        private Calculator _calculator;

        private int _preFilled;
        private int _empty;

        [SerializeField] private int maxWidth = 5;
        [SerializeField] private int maxHeight = 5;

        private int _gridIndex;

        private int _step;

        private List<int> _materialAreaCounts = new(6);
        private List<(int, int)> _emptyTiles = new();

        public override void OnEpisodeBegin()
        {
            _gridIndex = 0;

            _emptyTiles.Clear();

            Grid.ForEachTile((tile, x, y) =>
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
                    int id = Grid.GetMaterial(i, j)?.ID ?? -1;
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
            if (_gridIndex >= maxWidth * maxHeight)
            {
                EndEpisode();
                return;
            }

            int? currentMaterial = Grid.GetMaterialId(_gridIndex, maxWidth, maxHeight);

            while (_gridIndex < maxWidth * maxHeight && currentMaterial == -1)
            {
                _gridIndex++;
                currentMaterial = Grid.GetMaterialId(_gridIndex, maxWidth, maxHeight);
            }

            var placementAction = actions.DiscreteActions[0];

            if (currentMaterial is null)
            {
                Grid.PlaceMaterial(_randomizedMaterials[placementAction], _gridIndex, maxWidth, maxHeight);
                _empty--;

                if (_materialAreaCounts[placementAction] != int.MinValue)
                {
                    _materialAreaCounts[placementAction] -= 1;
                }
            }

            _gridIndex++;

            if (_empty > 0) return;

            EndEpisode();
        }

        protected override void Awake()
        {
            Academy.Instance.AutomaticSteppingEnabled = false;

            _randomizedMaterials = new IMaterial[randomizedMaterialsObject.Length];

            for (var index = 0; index < randomizedMaterialsObject.Length; index++)
            {
                _randomizedMaterials[index] = randomizedMaterialsObject[index];
            }
        }
        
        private int[] DistributeUniformly(int totalAmount, int buckets)
        {
            int[] result = new int[buckets];

            if (totalAmount == int.MinValue)
            {
                for (int i = 0; i < buckets; i++) result[i] = int.MinValue;
                return result;
            }
    
            if (totalAmount <= 0) return result; 

            float[] weights = new float[buckets];
            float totalWeight = 0f;
            for (int i = 0; i < buckets; i++)
            {
                weights[i] = Random.Range(0.5f, 1.5f); 
                totalWeight += weights[i];
            }

            int currentTotal = 0;
            for (int i = 0; i < buckets; i++)
            {
                result[i] = Mathf.FloorToInt((weights[i] / totalWeight) * totalAmount);
                currentTotal += result[i];
            }

            int remainder = totalAmount - currentTotal;
            for (int i = 0; i < remainder; i++)
            {
                result[Random.Range(0, buckets)]++;
            }

            return result;
        }

        public void Step()
        {
            if (aiMenu.SelectedSubGrid is null)
            {
                int oY = -1;
                int oX = -1;

                var subGridsLength = gridManager.SubGrids.Length;
                
                int[] bushAreas = DistributeUniformly(aiMenu.BushArea, subGridsLength);
                int[] flowerAreas = DistributeUniformly(aiMenu.FlowerArea, subGridsLength);
                int[] grassAreas = DistributeUniformly(aiMenu.GrassArea, subGridsLength);
                int[] treeAreas = DistributeUniformly(aiMenu.TreeArea, subGridsLength);
                int[] waterAreas = DistributeUniformly(aiMenu.WaterArea, subGridsLength);
                int[] tileAreas = DistributeUniformly(aiMenu.TileArea, subGridsLength);

                for (int i = 0; i < Math.Min(aiMenu.GetSteps(), gridManager.width * gridManager.height); i++)
                {
                    var x = (i % (gridManager.width * GridManager.SubGridSize)) /
                            (GridManager.SubGridSize * GridManager.SubGridSize);
                    var y = i / (GridManager.SubGridSize * gridManager.width);


                    if (oY != y || oX != x)
                    {
                        Grid = gridManager.SubGrids[x, y];
                        _materialAreaCounts.Clear();
                        _materialAreaCounts.Add(waterAreas[y * (gridManager.width / GridManager.SubGridSize) + x]);
                        _materialAreaCounts.Add(tileAreas[y * (gridManager.width / GridManager.SubGridSize) + x]);
                        _materialAreaCounts.Add(flowerAreas[y * (gridManager.width / GridManager.SubGridSize) + x]);
                        _materialAreaCounts.Add(grassAreas[y * (gridManager.width / GridManager.SubGridSize) + x]);
                        _materialAreaCounts.Add(treeAreas[y * (gridManager.width / GridManager.SubGridSize) + x]);
                        _materialAreaCounts.Add(bushAreas[y * (gridManager.width / GridManager.SubGridSize) + x]);
                        OnEpisodeBegin();
                    }

                    RequestDecision();
                    Academy.Instance.EnvironmentStep();
                    oX = x;
                    oY = y;
                }

                gridManager.InvokeGridChangeEvent();
            }
            else
            {
                Grid = gridManager.SubGrids[
                    Math.Min(aiMenu.SelectedSubGrid.Value.Item1, gridManager.SubGrids.GetLength(0)),
                    Math.Min(aiMenu.SelectedSubGrid.Value.Item2, gridManager.SubGrids.GetLength(1))];
                _materialAreaCounts.Clear();
                _materialAreaCounts.Add(aiMenu.WaterArea);
                _materialAreaCounts.Add(aiMenu.TileArea);
                _materialAreaCounts.Add(aiMenu.FlowerArea);
                _materialAreaCounts.Add(aiMenu.GrassArea);
                _materialAreaCounts.Add(aiMenu.TreeArea);
                _materialAreaCounts.Add(aiMenu.BushArea);
                OnEpisodeBegin();
                for (int i = 0; i < Math.Min(aiMenu.GetSteps(), gridManager.width * gridManager.height); i++)
                {
                    RequestDecision();
                    Academy.Instance.EnvironmentStep();
                }

                gridManager.InvokeGridChangeEvent();
            }
        }
    }
}