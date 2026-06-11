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
        private Material _material;
        private readonly GridManager _manager;
        public readonly int X;
        public readonly int Z;

        public Tile(int x, int z, GridManager manager)
        {
            X = x;
            Z = z;
            _manager = manager;
        }

        public void SetMaterial(Material material)
        {
            ClearMaterial();
            _material = Object.Instantiate(material,
                _manager.transform.position + new Vector3(X * _manager.tileSize + material.offset.x, material.offset.y, Z * _manager.tileSize + material.offset.z),
                new Quaternion());
        }

        public void ClearMaterial()
        {
            if (_material is null) return;
            Object.Destroy(_material.gameObject);
            _material = null;
        }

        [CanBeNull]
        public Material GetMaterial()
        {
            return this._material;
        }
    }
}