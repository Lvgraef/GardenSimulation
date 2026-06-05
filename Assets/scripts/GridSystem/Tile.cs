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
        private readonly int _x;
        private readonly int _z;

        public Tile(int x, int z, GridManager manager)
        {
            _x = x;
            _z = z;
            _manager = manager;
        }

        public void SetMaterial(Material material)
        {
            ClearMaterial();
            _material = Object.Instantiate(material,
                _manager.transform.position + new Vector3(_x * _manager.tileSize + material.offset.x, material.offset.y, _z * _manager.tileSize + material.offset.z),
                new Quaternion());
        }

        public void ClearMaterial()
        {
            if (_material is null) return;
            Object.Destroy(_material.gameObject);
            _material = null;
        }

        public Material GetMaterial()
        {
            return this._material;
        }
    }
}