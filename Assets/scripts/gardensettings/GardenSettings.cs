using calculation;
using UnityEngine;

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
    }
}