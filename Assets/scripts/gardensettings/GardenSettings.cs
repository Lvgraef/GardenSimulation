using GardenDataManagement;
using GridSystem;
using TMPro;
using UnityEngine;

namespace gardensettings
{
    public class GardenSettings : MonoBehaviour
    {
        public string Address { get; set; }
        public Fertilizer Fertilizer { get; set; }
        public CompostCleanup CompostCleanup { get; set; }
        public PlantDiversity PlantDiversity { get; set; }
        public bool FlyingInsects { get; set; }
        public bool Birds { get; set; }
        public bool Spiders { get; set; }
        public bool OtherAnimals { get; set; }

        [SerializeField] private TMP_InputField addressInput;
        [SerializeField] private TMP_Dropdown fertilizerDropdown;
        [SerializeField] private TMP_Dropdown compostCleanupDropdown;
        [SerializeField] private TMP_Dropdown plantDiversityDropdown;
        [SerializeField] private TMP_Dropdown flyingInsectsDropdown;
        [SerializeField] private TMP_Dropdown birdsDropdown;
        [SerializeField] private TMP_Dropdown spidersDropdown;
        [SerializeField] private TMP_Dropdown otherAnimalsDropdown;

        [SerializeField] private GridManager gridManager;
        public void Updated()
        {
            Address = addressInput.text;
            Fertilizer = (Fertilizer)fertilizerDropdown.value;
            CompostCleanup = (CompostCleanup)compostCleanupDropdown.value;
            PlantDiversity = (PlantDiversity)plantDiversityDropdown.value;
            FlyingInsects = flyingInsectsDropdown.value > 0;
            Birds = birdsDropdown.value > 0;
            Spiders = spidersDropdown.value > 0;
            OtherAnimals = otherAnimalsDropdown.value > 0;
            
            gridManager.InvokeGridChangeEvent();
        }

        public void LoadFrom(GardenSettingsDataModel data)
        {
            fertilizerDropdown.value = (int)data.Fertilizer;
            compostCleanupDropdown.value = (int)data.CompostCleanup;
            plantDiversityDropdown.value = (int)data.PlantDiversity;
            flyingInsectsDropdown.value = data.FlyingInsects ? 1 : 0;
            birdsDropdown.value = data.Birds ? 1 : 0;
            spidersDropdown.value = data.Spiders ? 1 : 0;
            otherAnimalsDropdown.value = data.OtherAnimals ? 1 : 0;

            // make the visible labels match 
            fertilizerDropdown.RefreshShownValue();
            compostCleanupDropdown.RefreshShownValue();
            plantDiversityDropdown.RefreshShownValue();
            flyingInsectsDropdown.RefreshShownValue();
            birdsDropdown.RefreshShownValue();
            spidersDropdown.RefreshShownValue();
            otherAnimalsDropdown.RefreshShownValue();

            Updated();
        }
    }
}