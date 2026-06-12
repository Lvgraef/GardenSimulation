using UnityEngine;

namespace GridSystem
{
    /// <summary>
    /// This is the class where the material object belongs too
    /// </summary>
    public class VirtualMaterial : IMaterial
    {
        public string MaterialName { get; set; }
        public MaterialCategory Category { get; set; }

        public VirtualMaterial(string materialName, MaterialCategory category)
        {
            MaterialName = materialName;
            Category = category;
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