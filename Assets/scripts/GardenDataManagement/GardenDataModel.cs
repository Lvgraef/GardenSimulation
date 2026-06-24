using gardensettings;
using Newtonsoft.Json;

namespace GardenDataManagement
{
    public class GardenDataModel
    {
        public string GardenName { get; }
        public (int Width, int Height) GridSize { get; }
        public int[,] Materials { get; }
        public GardenSettingsDataModel GardenSettings { get; }

        [JsonConstructor]
        public GardenDataModel(string gardenName, (int Width, int Height) gridSize,
            int[,] materials,
            GardenSettingsDataModel gardenSettings)
        {
            GardenName = gardenName;
            GridSize = gridSize;
            Materials = materials;
            GardenSettings = gardenSettings;
        }

        public GardenDataModel(string gardenName, (int Width, int Height) gridSize,
            GardenSettingsDataModel gardenSettings)
        {
            GardenName = gardenName;
            GridSize = gridSize;
            Materials = new int[gridSize.Width, gridSize.Height];
            GardenSettings = gardenSettings;
        }

        public void StoreMaterials(int material, (int x, int y) tile)
            => Materials[tile.x, tile.y] = material;
    }

    public class GardenSettingsDataModel
    {
        public Fertilizer Fertilizer { get; }
        public CompostCleanup CompostCleanup { get; }
        public PlantDiversity PlantDiversity { get; }

        public bool FlyingInsects { get; }
        public bool Birds { get; }
        public bool Spiders { get; }
        public bool OtherAnimals { get; }

        [JsonConstructor]
        public GardenSettingsDataModel(
            Fertilizer fertilizer,
            CompostCleanup compostCleanup,
            PlantDiversity plantDiversity,
            bool flyingInsects,
            bool birds,
            bool spiders,
            bool otherAnimals)
        {
            Fertilizer = fertilizer;
            CompostCleanup = compostCleanup;
            PlantDiversity = plantDiversity;
            FlyingInsects = flyingInsects;
            Birds = birds;
            Spiders = spiders;
            OtherAnimals = otherAnimals;
        }
    }
}