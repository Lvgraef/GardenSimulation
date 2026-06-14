using calculation;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace gardensettings
{
    public class GardenSettings : MonoBehaviour
    {
        public Fertilizer Fertilizer { get; private set; }
        public CompostCleanup CompostCleanup { get; private set; }
        public PlantDiversity PlantDiversity { get; private set; }
        public bool FlyingInsects { get; private set; }
        public bool Birds { get; private set; }
        public bool Spiders { get; private set; }
        public bool OtherAnimals { get; private set; }
        
        [SerializeField] private TMP_Dropdown fertilizerDropdown;
        [SerializeField] private TMP_Dropdown compostCleanupDropdown;
        [SerializeField] private TMP_Dropdown plantDiversityropdown;
        [SerializeField] private TMP_Dropdown flyingInsectsDropdown;
        [SerializeField] private TMP_Dropdown birdsDropdown;
        [SerializeField] private TMP_Dropdown spidersDropdown;
        [SerializeField] private TMP_Dropdown otherAnimalsDropdown;

        public void Updated()
        {
            Fertilizer = (Fertilizer) fertilizerDropdown.value;
            CompostCleanup = (CompostCleanup) compostCleanupDropdown.value;
            PlantDiversity = (PlantDiversity)  plantDiversityropdown.value;
            FlyingInsects = flyingInsectsDropdown.value > 0;
            Birds = birdsDropdown.value > 0;
            Spiders = spidersDropdown.value > 0;
            OtherAnimals = otherAnimalsDropdown.value > 0;
        }
        
        public void LoadFrom(GardenSettingsDataModel data)
        {
            fertilizerDropdown.value     = (int)data.Fertilizer;
            compostCleanupDropdown.value = (int)data.CompostCleanup;
            plantDiversityropdown.value  = (int)data.PlantDiversity;
            flyingInsectsDropdown.value  = data.FlyingInsects ? 1 : 0;
            birdsDropdown.value          = data.Birds ? 1 : 0;
            spidersDropdown.value        = data.Spiders ? 1 : 0;
            otherAnimalsDropdown.value   = data.OtherAnimals ? 1 : 0;

            // make the visible labels match 
            fertilizerDropdown.RefreshShownValue();
            compostCleanupDropdown.RefreshShownValue();
            plantDiversityropdown.RefreshShownValue();
            flyingInsectsDropdown.RefreshShownValue();
            birdsDropdown.RefreshShownValue();
            spidersDropdown.RefreshShownValue();
            otherAnimalsDropdown.RefreshShownValue();

            Updated();
        }
    }
}