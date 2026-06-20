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
                    IMaterial material = allTiles[x, y].GetMaterial();
                    
                    if (material == null)
                    {
                               
                        dataModel.StoreMaterials(-1, (x, y));
                    }
                    else
                    {
                               
                        dataModel.StoreMaterials(material.ID, (x, y));
                    }
             
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
        
        //TODO: ADD PROMPT WHEN TRYING TO READ
        // Need to refresh chartbar
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
            

            gridManager.CreateGrid(gardenData.GridSize.Width, gardenData.GridSize.Height);
            gardenSettings.LoadFrom(gardenData.GardenSettings);
            
            for (int x = 0; x < gardenData.Materials.GetLength(0); x++)
            {
                for (int y = 0; y < gardenData.Materials.GetLength(1); y++)
                {
                    int tile = gardenData.Materials[x, y];
                    if(tile == -1) continue;
                    Material material = _gardenMaterials[tile];
                    var gridPos = (x, y);
                    gridManager.PlaceMaterial(material, gridPos);
                }
            }
        }


        
    }
}