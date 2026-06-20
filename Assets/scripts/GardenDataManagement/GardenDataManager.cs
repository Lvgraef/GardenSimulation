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
        
        [SerializeField] private GardenLoadMenu gardenLoadMenu;
        [SerializeField] private ConfirmationPopup confirmationPopup;
        [SerializeField] private NamePromptPopup namePromptPopup;
        
        [SerializeField] private GridManager gridManager;
        [SerializeField] private MaterialMenu menu;
        [SerializeField] private GardenSettings gardenSettings;
        
        private string _gardenName;

        private Dictionary<int, Material> _gardenMaterials;
        
        private GardenDataModel BuildGardenData()
        {
            Tile [,] allTiles = gridManager.GetAllTiles();

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
        
        private void Awake()
        {
            _gardenMaterials = new Dictionary<int, Material>();
            foreach (var material in menu.Materials)
            {
                _gardenMaterials.Add(material.ID, material);
            }

            gardenLoadMenu.onGardenSelected.AddListener(HandleGardenSelected);
        }
        
        private void OnDestroy()
        {
            if (gardenLoadMenu != null)
                gardenLoadMenu.onGardenSelected.RemoveListener(HandleGardenSelected);
        }
        
        private void HandleGardenSelected(string selectedName)
        {
            _gardenName = selectedName;
            GetGardenData();
        }

        private void PerformSave()
        {
            var gardenData = BuildGardenData();
            if (!Datainterface.SaveGardenData(gardenData))
            {
                Debug.LogError("Failed to save garden!");
            }
            else
            {
                Debug.Log("Garden data saved!");
            }
        }
        
        public void SaveGardenData()
        {
            if (string.IsNullOrEmpty(_gardenName))
            {
              
                namePromptPopup.Show(
                    onConfirm: chosenName =>
                    {
                        _gardenName = chosenName;
                        PerformSave();
                    },
                    onCancel: () => Debug.Log("Save cancelled")
                );
                return;
            }
            var gardenData = BuildGardenData(); 
            
            if (Datainterface.GardenExists(gardenData.GardenName))
            {
                confirmationPopup.Show(
                    "Weet u het zeker dat u deze tuin indeling wilt overschrijven?",
                    onConfirm: PerformSave,
                    onCancel: () => Debug.Log("Save cancelled")
                );

                return;
            }

            PerformSave();
        }

        public void OpenGardenLoadMenu()
        {
            gardenLoadMenu.gameObject.SetActive(true);
        }
        
        // Need to refresh chartbar i think
        private void GetGardenData()
        {
            // Have to link the name to whatever we are planning to get the name from
            var gardenData = Datainterface.GetGardenData(_gardenName);

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
        }


        
    }
}