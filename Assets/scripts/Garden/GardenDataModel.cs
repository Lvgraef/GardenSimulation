using gardensettings;
using GridSystem;

namespace Data
{
    public class GardenDataModel
    {
        public string GardenName { get;  }
        public (int Width, int Height) GridSize { get;  }
        public Material.Category[,]  Materials { get; }
        
        public GardenSettingsDataModel GardenSettingsDataModel { get; }

        public void StoreMaterials(Material.Category materials, (int x, int y) tile)
        {
            Materials[tile.x, tile.y] = materials;
        }
        
        public GardenDataModel(string gardenName, (int Width, int Height) gridSize, GardenSettingsDataModel gardenSettingsDataModel)
        {
            GardenName = gardenName;
            GridSize = gridSize;
            Materials =  new Material.Category[gridSize.Width, gridSize.Height];
            GardenSettingsDataModel = gardenSettingsDataModel;
        }
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