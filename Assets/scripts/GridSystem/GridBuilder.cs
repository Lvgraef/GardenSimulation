using UnityEngine;

namespace GridSystem
{
    public class GridBuilder : MonoBehaviour
    {
          private Tile[,] tiles;


            [SerializeField] private int width;
            [SerializeField] private int height;

            private float tileSize = 1f;
            
            public void RemoveTile(int x, int y)
            {
                tiles[x, y].ClearMaterial();
            }

            private bool OutOfBounds(int x, int y)
            {
                if (x > width || y > height || x < 0 || y < 0)
                {
                    return true;
                }

               return  false;
            }

            private (int x, int y) WorldToGrid(Vector3 worldPos)
            {
                int x = Mathf.FloorToInt((worldPos - transform.position).x );
                int y = Mathf.FloorToInt((worldPos - transform.position).z );
                
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
                    Debug.LogError("Material is being placed out of bounds");
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
                        tiles[x, y] = new Tile ();
                    }
                }
            }
            
            /// <summary>
            /// Draw lines in unity
            /// </summary>
            private void OnDrawGizmos()
            {
                Gizmos.color = Color.black;
                if (width <= 0 || height <= 0) return;
                Vector3 origin = transform.position;

                for (int y = 0; y <= height; y++)
                {
                    Vector3 start = origin + new Vector3(0, 0.01f, y * tileSize);
                    Vector3 end = origin + new Vector3(width * tileSize, 0.01f, y * tileSize);
                    Gizmos.DrawLine(start, end);
                }

                for (int x = 0; x <= width; x++)
                {
                    Vector3 start = origin + new Vector3(x * tileSize, 0.01f, 0);
                    Vector3 end = origin + new Vector3(x * tileSize, 0.01f, height * tileSize);
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