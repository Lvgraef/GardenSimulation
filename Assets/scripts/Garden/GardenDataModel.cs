using GridSystem;

namespace Data
{
    public class GardenDataModel
    {
        public string GardenName { get;  }
        public (int Width, int Height) GridSize { get;  }
        public Material.Category[,]  Materials { get; }

        public void StoreMaterials(Material.Category materials, (int x, int y) tile)
        {
            Materials[tile.x, tile.y] = materials;
        }
        
        public GardenDataModel(string gardenName, (int Width, int Height) gridSize)
        {
            GardenName = gardenName;
            GridSize = gridSize;
            Materials =  new Material.Category[gridSize.Width, gridSize.Height];
        }
    }
    

}