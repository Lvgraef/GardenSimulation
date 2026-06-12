using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GridSystem
{
    /// <summary>
    /// This is the class that would hold the material
    /// </summary>
    public class Tile
    {
        private IMaterial _material;
        private readonly GridManager _manager;
        public readonly int X;
        public readonly int Z;

        public Tile(int x, int z, GridManager manager)
        {
            X = x;
            Z = z;
            _manager = manager;
        }

        public void SetMaterial(IMaterial material)
        {
            ClearMaterial();
            _material = material.Assign(this, _manager);
        }

        public void ClearMaterial()
        {
            if (_material is null) return;
            _material.Clear();
            _material = null;
        }

        [CanBeNull]
        public IMaterial GetMaterial()
        {
            return _material;
        }
    }
}