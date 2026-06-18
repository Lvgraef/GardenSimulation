using System;
using System.Collections.Generic;
using camera;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GridSystem
{
    public class GridManager : MonoBehaviour
    {
        public const int SubGridSize = 5;
        
        [SerializeField] private bool training;
        
        private Tile[,] _tiles;

        private UnityEngine.Material _lineMaterial;

        public int width;
        public int height;

        [SerializeField] private CameraManager cameraManager;
        [SerializeField] private MaterialMenu menu;


        public float tileSize = 0.5f;
        public float tileArea;

        public event Action GridChangeEvent;

        public SubGrid[,] SubGrids;

        public void InvokeGridChangeEvent()
        {
            GridChangeEvent?.Invoke();
        }
        

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

        private void Update()
        {
            if (training) return;
            if (Mouse.current.leftButton.isPressed)
            {
                ShootRay();
            }
        }

        private void ShootRay()
        {
            if (cameraManager is null || menu is null || EventSystem.current.IsPointerOverGameObject()) return;

            var selectedMaterial = menu.SelectedMaterial;
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Plane gridPlane = new Plane(Vector3.up, transform.position);
            Ray ray = cameraManager.GetCurrentCamera()
                .ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, 0));
            gridPlane.Raycast(ray, out float distance);
            var intersectPosition = ray.direction * distance + ray.origin;
            var gridPos = WorldToGrid(intersectPosition);
            var mat = GetMaterial(gridPos.x, gridPos.y);

            if (mat is not null && mat.Category == MaterialCategory.Building) return;

            if (menu.Eraser)
            {
                RemoveMaterial((gridPos.x, gridPos.y));
                GridChangeEvent?.Invoke();
                return;
            }

            if (selectedMaterial is null || (mat is not null &&
                                             mat.MaterialName[..selectedMaterial.MaterialName.Length] ==
                                             selectedMaterial.MaterialName)) return;

            PlaceMaterial(selectedMaterial, gridPos);
            GridChangeEvent?.Invoke();
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

        private (int x, int y) WorldToGrid(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt((worldPos - transform.position).x / tileSize);
            int y = Mathf.FloorToInt((worldPos - transform.position).z / tileSize);

            return (x, y);
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
        /// <param name="material"></param>
        /// <param name="vector"></param>
        public void PlaceMaterial(IMaterial material, Vector3 vector)
        {
            (int x, int y) = WorldToGrid(vector);
            if (OutOfBounds(x, y))
            {
                return;
            }

            if (_tiles[x, y].GetMaterial()?.Category == MaterialCategory.Building) return;
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
        /// <param name="vector"></param>
        public void RemoveMaterial(Vector3 vector)
        {
            (int x, int y) = WorldToGrid(vector);
            if (OutOfBounds(x, y))
            {
                return;
            }

            if (_tiles[x, y].GetMaterial()?.Category == MaterialCategory.Building) return;
            _tiles[x, y]?.ClearMaterial();
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


        /// <summary>
        /// Generates the grid
        /// </summary>
        private void GenerateGrid()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _tiles[x, y] = new Tile(x, y, this);
                }
            }

            var xOffset = width * tileSize / 2;
            var zOffset = height * tileSize / 2;

            transform.position = new Vector3(-xOffset, 0, -zOffset);
        }

        /// <summary>
        /// Draw lines in unity
        /// </summary>
        private void OnRenderObject()
        {
            if (training) return;
            if (!_lineMaterial) return;

            GL.PushMatrix();
            _lineMaterial.SetPass(0);

            if (width <= 0 || height <= 0)
            {
                GL.PopMatrix();
                return;
            }

            Vector3 origin = transform.position;

            for (int y = 0; y <= height; y++)
            {
                Vector3 start = origin + new Vector3(0, 0, y * tileSize);
                Vector3 end = origin + new Vector3(width * tileSize, 0, y * tileSize);
                GL.Begin(GL.LINES);
                GL.Color(Color.forestGreen);
                GL.Vertex(start);
                GL.Vertex(end);
                GL.End();
            }

            for (int x = 0; x <= width; x++)
            {
                Vector3 start = origin + new Vector3(x * tileSize, 0f, 0);
                Vector3 end = origin + new Vector3(x * tileSize, 0f, height * tileSize);
                GL.Begin(GL.LINES);
                GL.Color(Color.forestGreen);
                GL.Vertex(start);
                GL.Vertex(end);
                GL.End();
            }

            GL.PopMatrix();
        }

        private void CreateSubGrids()
        {
            SubGrids = new SubGrid[Mathf.CeilToInt((float) width / SubGridSize),Mathf.CeilToInt((float) width / SubGridSize)];
            
            for (int i = 0; i < width; i += 5)
            {
                for (int j = 0; j < height; j += 5)
                {
                    Tile[,] subGridTiles = new Tile[5, 5];
                        
                    for (int k = 0; k < SubGridSize; k++)
                    {
                        for (int l = 0; l < SubGridSize; l++)
                        {
                            subGridTiles[k, l] = _tiles[i + k, j + l];
                        }
                    }
                    
                    SubGrids[i / 5, j / 5] = new SubGrid(subGridTiles, SubGridSize, SubGridSize);
                }
            }
        }
        
        private void Awake()
        {
            tileArea = tileSize * tileSize;
        }

        void Start()
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            _lineMaterial = new UnityEngine.Material(shader);
            _tiles = new Tile[width, height];
            GenerateGrid();
            CreateSubGrids();
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
                return null;
            }
            
            return _tiles[x, y].GetMaterial()?.ID;
        }
    }
}