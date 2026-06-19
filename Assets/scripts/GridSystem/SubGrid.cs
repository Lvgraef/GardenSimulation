using System;
using System.Collections.Generic;
using camera;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GridSystem
{
    public class SubGrid
    {
        private Tile[,] _tiles;
        
        private int width;
        private int height;
        
        public void ForEachTile(Action<Tile, int, int> action)
        {
            for (var i = 0; i < _tiles.GetLength(0); i++)
            {
                for (var j = 0; j < _tiles.GetLength(1); j++)
                {
                    var tile = _tiles[i, j];
                    if (tile.GetMaterial()?.Category == MaterialCategory.Building) continue;
                    action(tile, i, j);
                }
            }
        }

        public void RemoveTile(int x, int y)
        {
            if (_tiles[x, y].GetMaterial()?.Category == MaterialCategory.Building) return;
            _tiles[x, y]?.ClearMaterial();
        }

        private bool OutOfBounds(int x, int y)
        {
            return x > width - 1 || y > height - 1 || x < 0 || y < 0;
        }

        public void PlaceMaterial(IMaterial material, int index, int maxWidth, int maxHeight)
        {
            int x = index % maxWidth;
            int y = index / maxHeight;

            if (OutOfBounds(x, y)) return;
            
            _tiles[x, y].SetMaterial(material);
        }

        /// <summary>
        /// Places the material on the grid
        /// </summary>
        public void PlaceMaterial(IMaterial material, (int x, int y) tile)
        {
            if (OutOfBounds(tile.x, tile.y))
            {
                return;
            }

            if (_tiles[tile.x, tile.y].GetMaterial()?.Category == MaterialCategory.Building) return;
            _tiles[tile.x, tile.y].SetMaterial(material);
        }

        /// <summary>
        /// Removes the material from the grid
        /// </summary>
        /// <param name="tile"></param>
        public void RemoveMaterial((int x, int y) tile)
        {
            if (OutOfBounds(tile.x, tile.y))
            {
                return;
            }

            if (_tiles[tile.x, tile.y].GetMaterial()?.Category == MaterialCategory.Building) return;
            _tiles[tile.x, tile.y]?.ClearMaterial();
        }

        /// <summary>
        /// Returns the Material object
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public IMaterial GetMaterial(int x, int y)
        {
            if (OutOfBounds(x, y) || _tiles[x, y] is null)
            {
                return null;
            }

            return _tiles[x, y].GetMaterial();
        }

        public void Reset()
        {
            foreach (var tile in _tiles)
            {
                tile.ClearMaterial();
            }
        }
        
        public void Clear()
        {
            foreach (var tile in _tiles)
            {
                if (tile.GetMaterial()?.Category == MaterialCategory.Building) continue;
                tile.ClearMaterial();
            }
        }

        public Dictionary<string, int> GetMaterialList()
        {
            Dictionary<string, int> materialList = new();
            ForEachTile((tile, _, _) =>
            {
                if (tile.GetMaterial() is not null)
                {
                    materialList[tile.GetMaterial()!.MaterialName] = materialList.GetValueOrDefault(tile.GetMaterial()!.MaterialName) + 1;
                }
            });

            return materialList;
        }

        public string GetMaterialName(int x, int y)
        {
            return OutOfBounds(x, y) ? "Building" : GetMaterial(x, y)?.MaterialName;
        }
        
        public string GetMaterialName(int index, int w, int h)
        {
            var x = index % w;
            var y =index / h;

            if (OutOfBounds(x, y))
            {
                return "Building";
            }
            
            return _tiles[x, y].GetMaterial()?.MaterialName;
        }

        public int? GetMaterialId(int index, int maxWidth, int maxHeight)
        {
            var x = index % maxWidth;
            var y =index / maxHeight;

            if (OutOfBounds(x, y))
            {
                return -1;
            }
            
            return _tiles[x, y].GetMaterial()?.ID;
        }

        public SubGrid(Tile[,] tiles, int width, int height)
        {
            _tiles = tiles;
            this.width = width;
            this.height = height;
        }
    }
}