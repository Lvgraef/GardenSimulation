using UnityEngine;

namespace GridSystem
{
    /// <summary>
    /// This is the class where the material object belongs too
    /// </summary>
    public class Material : MonoBehaviour, IMaterial
    {
        public Sprite sprite;

        public Vector3 offset = new(0.25f, 0.05f, 0.25f);

        public string MaterialName { get; set; }
        public MaterialCategory Category { get; set; }

        public IMaterial Assign(Tile tile, GridManager manager)
        {
            return Instantiate(this,
                manager.transform.position + new Vector3(tile.X * manager.tileSize + offset.x, offset.y,
                    tile.Z * manager.tileSize + offset.z), new Quaternion());
        }

        public void Clear()
        {
            Destroy(gameObject);
        }
    }
}