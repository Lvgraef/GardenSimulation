using camera;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GridSystem
{
    public class GridManager : MonoBehaviour
    {
        private Tile[,] tiles;

        private UnityEngine.Material material;

        [SerializeField] private int width;
        [SerializeField] private int height;

        [SerializeField] private CameraManager cameraManager;
        [SerializeField] private MaterialMenu menu;

        public float tileSize = 1f;

        private void Update()
        {
            if (Mouse.current.leftButton.isPressed)
            {
                ShootRay();
            }
        }

        private void ShootRay()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            var selectedMaterial = menu.SelectedMaterial;
            if (selectedMaterial is null) return;
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Plane gridPlane = new Plane(Vector3.up, transform.position);
            Ray ray = cameraManager.GetCurrentCamera()
                .ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, 0));
            gridPlane.Raycast(ray, out float distance);
            var intersectPosition = ray.direction * distance + ray.origin;
            var gridPos = WorldToGrid(intersectPosition);
            var mat = GetMaterial(gridPos.x, gridPos.y);
            if (mat is not null &&
                mat.name.Substring(0, selectedMaterial.name.Length) == selectedMaterial.name) return;
            if (menu.Eraser)
            {
                RemoveTile(gridPos.x, gridPos.y);
            }

            PlaceMaterial(selectedMaterial, intersectPosition);
        }

        public void RemoveTile(int x, int y)
        {
            tiles[x, y].ClearMaterial();
        }

        private bool OutOfBounds(int x, int y)
        {
            return x > width - 1 || y > height - 1 || x < 0 || y < 0;
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
            if (OutOfBounds(x, y) || tiles[x, y] is null)
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
        private void OnRenderObject()
        {
            if (!material) return;

            GL.PushMatrix();
            material.SetPass(0);

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


        void Start()
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            material = new UnityEngine.Material(shader);
            tiles = new Tile[width, height];
            GenerateGrid();
        }
    }
}