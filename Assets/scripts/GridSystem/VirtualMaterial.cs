namespace GridSystem
{
    /// <summary>
    /// This is the class where the material object belongs too
    /// </summary>
    public class VirtualMaterial : IMaterial
    {
        public string MaterialName { get; set; }
        public MaterialCategory Category { get; set; }
        public int ID { get; set; }

        public VirtualMaterial(string materialName, MaterialCategory category, int id)
        {
            MaterialName = materialName;
            Category = category;
            ID = id;
        }

        public IMaterial Assign(Tile tile, GridManager manager)
        {
            return this;
        }

        public void Clear()
        {
        }
    }
}