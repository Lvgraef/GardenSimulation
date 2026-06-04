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
        private int _x;
        private int _y;

        public Tile(int x, int y)
        {
            this._x = x;
            this._y = y;
        }

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