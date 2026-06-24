using System;
using System.Collections;
using System.Collections.Generic;
using gardensettings;
using GridSystem;
using UI;
using UnityEngine;
using Utils;
using Material = GridSystem.Material;

namespace GardenDataManagement
{
    public class GardenDataManager : MonoBehaviour
    {
        private const int EmptyTile = -2;
        private const int BuildingTile = -1;

        // check for when data has been changed
        private bool _isDirty;

        [SerializeField] private GardenLoadMenu gardenLoadMenu;
        [SerializeField] private ConfirmationPopup confirmationPopup;
        [SerializeField] private NamePromptPopup namePromptPopup;
        [SerializeField] private InfoPopup infoPopup;

        [SerializeField] private GridManager gridManager;
        [SerializeField] private MaterialMenu menu;
        [SerializeField] private GardenSettings gardenSettings;

        private string _gardenName;

        private Dictionary<int, Material> _gardenMaterials;

        private void OnGridChanged() => _isDirty = true;

        private void Awake()
        {
            _gardenMaterials = new Dictionary<int, Material>();
            foreach (var material in menu.Materials)
            {
                _gardenMaterials.Add(material.ID, material);
            }

            gridManager.GridChangeEvent += OnGridChanged;
            gardenLoadMenu.onGardenSelected.AddListener(HandleGardenSelected);
        }


        private GardenDataModel BuildGardenData()
        {
            Tile[,] allTiles = gridManager.GetAllTiles();

            if (allTiles == null)
            {
                Debug.Log("No tiles found!");
                return null;
            }

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
                _gardenName,
                gridManager.GetSize(),
                gardenSettingsDataModel
            );

            for (int x = 0; x < allTiles.GetLength(0); x++)
            {
                for (int y = 0; y < allTiles.GetLength(1); y++)
                {
                    Tile tile = allTiles[x, y];
                    if (tile == null)
                    {
                        dataModel.StoreMaterials(EmptyTile, (x, y));
                        continue;
                    }

                    IMaterial material = tile.GetMaterial();

                    if (material == null)
                    {
                        dataModel.StoreMaterials(EmptyTile, (x, y));
                        continue;
                    }

                    if (material.Category == MaterialCategory.Building)
                    {
                        dataModel.StoreMaterials(BuildingTile, (x, y));
                        continue;
                    }

                    dataModel.StoreMaterials(material.ID, (x, y));
                }
            }

            return dataModel;
        }


        private void OnDestroy()
        {
            if (gridManager != null)
                gridManager.GridChangeEvent -= OnGridChanged;
            if (gardenLoadMenu != null)
                gardenLoadMenu.onGardenSelected.RemoveListener(HandleGardenSelected);
        }

        private void HandleGardenSelected(string selectedName)
        {
            PromptIfDirty(() =>
            {
                _gardenName = selectedName;
                GetGardenData();
            });
        }

        public void CreateNewGarden()
        {
            PromptIfDirty(PerformNewGarden);
        }

        private void PromptIfDirty(Action proceed)
        {
            if (!_isDirty)
            {
                proceed();
                return;
            }

            confirmationPopup.Show(
                "U heeft niet opgeslagen wijzigingen. Wilt u deze opslaan?",
                onConfirm: () => SaveGardenData(onSuccess: proceed),
                onCancel: () =>
                    StartCoroutine(ShowDiscardConfirm(proceed))
            );
        }

        private void PerformNewGarden()
        {
            gridManager.ClearGrid();
            gardenSettings.ResetToDefaults();
            _gardenName = null;

            if (!string.IsNullOrEmpty(gardenSettings.Address))
            {
                gridManager.UpdateAddress();
            }

            _isDirty = false;
        }

        private void PerformSave(Action onSuccess = null)
        {
            var gardenData = BuildGardenData();
            if (!DataInterface.SaveGardenData(gardenData))
            {
                Debug.LogError("Failed to save garden!");
                return;
            }

            Debug.Log("Garden data saved!");
            _isDirty = false;
            onSuccess?.Invoke();
        }

        private IEnumerator ShowDiscardConfirm(Action proceed)
        {
            yield return null; // wait one frame so the first popup fully closes

            confirmationPopup.Show(
                "Weet u zeker dat u de wijzigingen wilt verwijderen?",
                onConfirm: proceed,
                onCancel: () => Debug.Log("New garden cancelled")
            );
        }

        public void SaveGardenData()
        {
            SaveGardenData(null);
        }

        private void SaveGardenData(Action onSuccess)
        {
            var (w, h) = gridManager.GetSize();
            if (w <= 0 || h <= 0 || gridManager.GetAllTiles() == null)
            {
                infoPopup.Show(
                    "U kunt geen tuin opslaan zonder indeling.",
                    onConfirm: () => { }
                );
                return;
            }

            if (string.IsNullOrEmpty(_gardenName))
            {
                namePromptPopup.Show(
                    onConfirm: chosenName => CheckOverwriteThenSave(chosenName, onSuccess),
                    onCancel: () => Debug.Log("Save cancelled")
                );
                return;
            }

            CheckOverwriteThenSave(_gardenName, onSuccess);
        }

        private void CheckOverwriteThenSave(string candidateName, Action onSuccess)
        {
            if (DataInterface.GardenExists(candidateName))
            {
                confirmationPopup.Show(
                    "Weet u het zeker dat u deze tuin indeling wilt overschrijven?",
                    onConfirm: () =>
                    {
                        _gardenName = candidateName;
                        PerformSave(onSuccess);
                    },
                    onCancel: () => Debug.Log("Save cancelled")
                );
                return;
            }

            _gardenName = candidateName;
            PerformSave(onSuccess);
        }

        public void OpenGardenLoadMenu()
        {
            gardenLoadMenu.gameObject.SetActive(true);
        }

        // Need to refresh chartbar i think
        private void GetGardenData()
        {
            // Have to link the name to whatever we are planning to get the name from
            var gardenData = DataInterface.GetGardenData(_gardenName);

            if (gardenData == null)
            {
                Debug.LogError("Failed to find garden!");
                return;
            }

            if (gardenData.Materials == null)
            {
                Debug.LogError("Failed to find garden materials!");
                return;
            }


            gridManager.ImportGridData(gardenData.GridSize.Width, gardenData.GridSize.Height);
            gardenSettings.LoadFrom(gardenData.GardenSettings);

            for (int x = 0; x < gardenData.Materials.GetLength(0); x++)
            {
                for (int y = 0; y < gardenData.Materials.GetLength(1); y++)
                {
                    int id = gardenData.Materials[x, y];

                    if (id == EmptyTile) continue;

                    if (id == BuildingTile)
                    {
                        gridManager.PlaceMaterial(gridManager.BuildingMaterial, (x, y));
                        continue;
                    }

                    if (!_gardenMaterials.TryGetValue(id, out var material)) continue;
                    gridManager.PlaceMaterial(material, (x, y));
                }
            }

            _isDirty = false;
        }
    }
}