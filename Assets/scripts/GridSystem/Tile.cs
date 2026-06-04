using System;
using UnityEngine;

namespace GridSystem
{
    /// <summary>
    /// This is the class that would hold the material
    /// </summary>
    public class Tile 
    {
        private Material _material;

        public void SetMaterial(Material material)
        {
            this._material = material;
        }
        
        public void ClearMaterial()
        {
            this._material = null;
        }

        public Material GetMaterial()
        {
            return this._material;
        }
    }
}