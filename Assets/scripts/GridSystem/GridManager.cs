using System;
using System.Collections.Generic;
using camera;
using JetBrains.Annotations;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GridSystem
{
    public class GridManager : MonoBehaviour
    {
        [ItemCanBeNull] private Tile[,] _tiles;

        private UnityEngine.Material _lineMaterial;

        [SerializeField] private int width;
        [SerializeField] private int height;

        [SerializeField] private CameraManager cameraManager;
        [SerializeField] private MaterialMenu menu;


        public float tileSize = 0.5f;
        public float tileArea;

        public event Action GridChangeEvent;

        public void ForEachTile(Action<Tile, int, int> action)
        {
            for (var i = 0; i < _tiles.GetLength(0); i++)
            {
                for (var j = 0; j < _tiles.GetLength(1); j++)
                {
                    var tile = _tiles[i, j];
                    if (tile?.GetMaterial().category == Material.Category.Building) continue;
                    action(tile, i, j);
                }
            }
        }

        private void Update()
        {
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

            if (mat is not null && mat.category == Material.Category.Building) return;

            if (menu.Eraser)
            {
                RemoveMaterial((gridPos.x, gridPos.y));
                GridChangeEvent?.Invoke();
                return;
            }

            if (selectedMaterial is null || (mat is not null &&
                                             mat.materialName[..selectedMaterial.materialName.Length] ==
                                             selectedMaterial.materialName)) return;

            PlaceMaterial(selectedMaterial, gridPos);
            GridChangeEvent?.Invoke();
        }

        public void RemoveTile(int x, int y)
        {
            if (_tiles[x, y]?.GetMaterial().category == Material.Category.Building) return;
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

        /// <summary>
        /// Places the material on the grid
        /// </summary>
        /// <param name="material"></param>
        /// <param name="vector"></param>
        public void PlaceMaterial(Material material, Vector3 vector)
        {
            (int x, int y) = WorldToGrid(vector);
            if (OutOfBounds(x, y))
            {
                return;
            }

            if (_tiles[x, y]?.GetMaterial().category == Material.Category.Building) return;
            _tiles[x, y]?.SetMaterial(material);
        }

        /// <summary>
        /// Places the material on the grid
        /// </summary>
        public void PlaceMaterial(Material material, (int x, int y) tile)
        {
            if (OutOfBounds(tile.x, tile.y))
            {
                return;
            }

            if (_tiles[tile.x, tile.y]?.GetMaterial().category == Material.Category.Building) return;
            _tiles[tile.x, tile.y]?.SetMaterial(material);
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

            if (_tiles[x, y]?.GetMaterial().category == Material.Category.Building) return;
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

            if (_tiles[tile.x, tile.y]?.GetMaterial().category == Material.Category.Building) return;
            _tiles[tile.x, tile.y]?.ClearMaterial();
        }

        /// <summary>
        /// Returns the Material object
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Material GetMaterial(int x, int y)
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
        }

        public void Reset()
        {
            foreach (var tile in _tiles)
            {
                if (tile?.GetMaterial().category == Material.Category.Building) continue;
                tile?.ClearMaterial();
            }
        }

        public Dictionary<string, int> GetMaterialList()
        {
            Dictionary<string, int> materialList = new();
            ForEachTile((tile, x, y) =>
            {
                if (tile is not null)
                {
                    materialList[tile.GetMaterial().name] = materialList.GetValueOrDefault(tile.GetMaterial().name) + 1;
                }
            });

            return materialList;
        }
    }
}