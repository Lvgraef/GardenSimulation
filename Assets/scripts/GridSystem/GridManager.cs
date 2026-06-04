using System;
using camera;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GridSystem
{
    public class GridManager : MonoBehaviour
    {
        private Tile[,] tiles;
        
        [SerializeField] private int width;
        [SerializeField] private int height;
        
        [SerializeField] private CameraManager cameraManager;
        [SerializeField] private MaterialMenu menu;

        public float tileSize = 1f;

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                ShootRay();
            }
        }

        private void ShootRay()
        {
            var material = menu.SelectedMaterial;
            if (material is null) return;
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Plane gridPlane = new Plane(Vector3.up, transform.position);
            Ray ray = cameraManager.GetCurrentCamera().ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, 0));
            gridPlane.Raycast(ray, out float distance);
            var intersectPosition = ray.direction * distance + ray.origin;
            PlaceMaterial(material, intersectPosition);
        }

        public void RemoveTile(int x, int y)
        {
            tiles[x, y].ClearMaterial();
        }

        private bool OutOfBounds(int x, int y)
        {
            if (x > width-1 || y > height-1 || x < 0 || y < 0)
            {
                return true;
            }

            return false;
        }

        private (int x, int y) WorldToGrid(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt((worldPos - transform.position).x);
            int y = Mathf.FloorToInt((worldPos - transform.position).z);

            return (x, y);
        }

        /// <summary>
        /// Places the material on the grid
        /// </summary>
        /// <param name="material"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void PlaceMaterial(Material material, Vector3 vector)
        {
            (int x, int y) = WorldToGrid(vector);
            if (OutOfBounds(x, y))
            {
                return;
            }

            tiles[x, y].SetMaterial(material);
        }

        /// <summary>
        /// Returns the Material object
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Material GetMaterial(int x, int y)
        {
            if (OutOfBounds(x, y))
            {
                return null;
            }

            return tiles[x, y].GetMaterial();
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
                    tiles[x, y] = new Tile(x, y, this);
                }
            }
        }

        /// <summary>
        /// Draw lines in unity
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.forestGreen;
            if (width <= 0 || height <= 0) return;
            Vector3 origin = transform.position;

            for (int y = 0; y <= height; y++)
            {
                Vector3 start = origin + new Vector3(0, 0, y * tileSize);
                Vector3 end = origin + new Vector3(width * tileSize, 0, y * tileSize);
                Gizmos.DrawLine(start, end);
            }

            for (int x = 0; x <= width; x++)
            {
                Vector3 start = origin + new Vector3(x * tileSize, 0f, 0);
                Vector3 end = origin + new Vector3(x * tileSize, 0f, height * tileSize);
                Gizmos.DrawLine(start, end);
            }
        }


        void Start()
        {
            tiles = new Tile[width, height];
            GenerateGrid();
        }
    }
}