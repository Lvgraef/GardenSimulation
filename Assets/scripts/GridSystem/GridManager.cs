using System;
using camera;
using UI;
using gardensettings;
using GardenSimulation.Services.Api;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using GardenSimulation.Model;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

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
		[SerializeField] private string address;

        [SerializeField] private CameraManager cameraManager;
        [SerializeField] private MaterialMenu menu;
		[SerializeField] private GardenSettings gardenSettings;
		private double minY;
		private double minX;

        public float tileSize = 0.5f;
        public float tileArea;

        private (double, double)[] ParcelCoordinates;
        private (double, double)[] PandCoordinates;

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

        public Tile[,] GetAllTiles()
        {
            return _tiles;
        }

        public (int width, int height) GetSize()
        {
            return (width, height);
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
        
        public void PlaceMaterial(Material material, Vector3 vector)
        {
            (int x, int y) = WorldToGrid(vector);
            
            if (OutOfBounds(x, y)) return;

            if (_tiles[x, y].GetMaterial()?.Category == MaterialCategory.Building) return;
            _tiles[x, y].SetMaterial(material);
        }

        public void PlaceMaterial(Material material, (int x, int y) tile)
        {
            if (OutOfBounds(tile.x, tile.y)) return;

            if (_tiles[tile.x, tile.y].GetMaterial()?.Category == MaterialCategory.Building) return;
            _tiles[tile.x, tile.y].SetMaterial(material);
        }
        
        public void RemoveMaterial(Vector3 vector)
        {
            (int x, int y) = WorldToGrid(vector);
            if (OutOfBounds(x, y)) return;

            if (_tiles[x, y].GetMaterial()?.Category == MaterialCategory.Building) return;
            _tiles[x, y]?.ClearMaterial();
        }

        public void RemoveMaterial((int x, int y) tile)
        {
            if (OutOfBounds(tile.x, tile.y)) return;

            if (_tiles[tile.x, tile.y].GetMaterial()?.Category == MaterialCategory.Building) return;
            _tiles[tile.x, tile.y]?.ClearMaterial();
        }

        public Material GetMaterial(int x, int y)
        {
            if (OutOfBounds(x, y) || _tiles[x, y] is null) return null;

            return _tiles[x, y].GetMaterial();
        }
        
        private void ClearAllTiles()
        {
            var allTiles = GetAllTiles();
            
            for (int x = 0; x < allTiles.GetLength(0); x++)
            {
                for (int y = 0; y < allTiles.GetLength(1); y++)
                {
                    RemoveTile(x, y);
                }
            }
        }
        
        //TODO: probably doesn't work
        public void CreateGrid(int Width, int Height)
        {
            if(Width <= 0 || Height <= 0) return;
            ClearAllTiles();
            GenerateGrid();
        }
        

        private IEnumerator GenerateGrid()
        {
			yield return new WaitUntil(() =>
			gardenSettings != null &&
			!string.IsNullOrWhiteSpace(gardenSettings.Address));
			address = gardenSettings.Address;
			int index = address.IndexOf(',');
			int count = 0;
			for (int i = 0; i < address.Length; i++)
			{

				if (i > index && address[i] == ' ')
				{
					if (count == 1)
					{
						address = address.Remove(i, 1);
						break;
					}
					count++;
				}
			}
            APIClient api = new APIClient();
            Response response = null;
            yield return api.GetCoordinatesByAddress(address, _response =>
            {
                response = _response;
            });

            if (!response.Success)
            {
                Debug.Log(response.Message);
                yield break;
            }

            double originX = response.ParcelCoordinates[0].Item1;
            double originY = response.ParcelCoordinates[0].Item2;

            double maxX = double.NegativeInfinity;
            double maxY = double.NegativeInfinity;
            minX = double.PositiveInfinity;
            minY = double.PositiveInfinity;

            for (int i = 0; i < response.ParcelCoordinates.Length; i++)
            {
                var p = response.ParcelCoordinates[i];

                p.Item1 -= originX;
                p.Item2 -= originY;

                response.ParcelCoordinates[i] = p;

                maxX = Math.Max(maxX, p.Item1);
                maxY = Math.Max(maxY, p.Item2);
                minX = Math.Min(minX, p.Item1);
                minY = Math.Min(minY, p.Item2);
            }

            width  = Mathf.CeilToInt((float)((maxX - minX) / tileSize)) + 1;
            height = Mathf.CeilToInt((float)((maxY - minY) / tileSize)) + 1;

            _tiles = new Tile[width, height];

            for (int i = 0; i < response.PandCoordinates.Length; i++)
            {
                var p = response.PandCoordinates[i];
                p.Item1 -= originX;
                p.Item2 -= originY;
                response.PandCoordinates[i] = p;
            }

            ParcelCoordinates = response.ParcelCoordinates;
            PandCoordinates = response.PandCoordinates;
			float xOffset = width * tileSize / 2;
            float zOffset = height * tileSize / 2;
            transform.position = new Vector3(-xOffset, 0, -zOffset);
            for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					_tiles[x, y] = new Tile(x, y, this);

					Vector2 p = new Vector2(
						(float)(minX + x * tileSize),
						(float)(minY + y * tileSize)
					);

					bool insideParcel = PointInPolygon(p, ParcelCoordinates);
					bool insideBuilding = PointInPolygon(p, PandCoordinates);
					if (!insideParcel || insideBuilding)
					{
						PlaceMaterial(buildingMaterial, (x, y));
					}	
				}
			}
        }

        private void OnRenderObject()
        {
            if (training) return;
            if (!_lineMaterial) return;
			if (ParcelCoordinates == null || PandCoordinates == null) return;
            GL.PushMatrix();
            _lineMaterial.SetPass(0);

            if (width <= 0 || height <= 0)
            {
                GL.PopMatrix();
                return;
            }

            Vector3 origin = transform.position;

            GL.Begin(GL.LINES);
            GL.Color(Color.green);

            for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (_tiles[x, y].GetMaterial() == buildingMaterial)
						continue;
						Vector3 bl = origin + new Vector3(
						x * tileSize,
						0,
						y * tileSize
					);

					Vector3 br = bl + new Vector3(tileSize, 0, 0);
					Vector3 tr = bl + new Vector3(tileSize, 0, tileSize);
					Vector3 tl = bl + new Vector3(0, 0, tileSize);

					GL.Vertex(bl); GL.Vertex(br);
					GL.Vertex(br); GL.Vertex(tr);
					GL.Vertex(tr); GL.Vertex(tl);
					GL.Vertex(tl); GL.Vertex(bl);
				}
			}
            GL.End();
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
        
        private bool PointInPolygon(Vector2 p, (double, double)[] coords)
        {
            if (coords == null || coords.Length < 3)
                return false;

            bool inside = false;

            for (int i = 0, j = coords.Length - 1; i < coords.Length; j = i++)
            {
                Vector2 a = new Vector2((float)coords[i].Item1, (float)coords[i].Item2);
                Vector2 b = new Vector2((float)coords[j].Item1, (float)coords[j].Item2);

                bool intersect = ((a.y > p.y) != (b.y > p.y)) &&
                                 (p.x < (b.x - a.x) * (p.y - a.y) /
                                     ((b.y - a.y) == 0 ? 0.00001f : (b.y - a.y)) + a.x);

                if (intersect)
                    inside = !inside;
            }

            return inside;
        }

        private void Awake()
        {
            tileArea = tileSize * tileSize;
        }

        IEnumerator Start()
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            _lineMaterial = new UnityEngine.Material(shader);
            yield return GenerateGrid();
            CreateSubGrids();
        }
    }
}