using calculation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace gardensettings
{
    public class GardenSettings : MonoBehaviour
    {
        public Fertilizer Fertilizer { get; set; }
        public CompostCleanup CompostCleanup { get; set; }
        public PlantDiversity PlantDiversity { get; set; }
        public bool FlyingInsects { get; set; }
        public bool Birds { get; set; }
        public bool Spiders { get; set; }
        public bool OtherAnimals { get; set; }
        
        [SerializeField] private TMP_Dropdown fertilizerDropdown;
        [SerializeField] private TMP_Dropdown compostCleanupDropdown;
        [SerializeField] private TMP_Dropdown plantDiversityDropdown;
        [SerializeField] private TMP_Dropdown flyingInsectsDropdown;
        [SerializeField] private TMP_Dropdown birdsDropdown;
        [SerializeField] private TMP_Dropdown spidersDropdown;
        [SerializeField] private TMP_Dropdown otherAnimalsDropdown;

        public void Updated()
        {
            Fertilizer = (Fertilizer) fertilizerDropdown.value;
            CompostCleanup = (CompostCleanup) compostCleanupDropdown.value;
            PlantDiversity = (PlantDiversity)  plantDiversityDropdown.value;
            FlyingInsects = flyingInsectsDropdown.value > 0;
            Birds = birdsDropdown.value > 0;
            Spiders = spidersDropdown.value > 0;
            OtherAnimals = otherAnimalsDropdown.value > 0;
        }
    }
}