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
        [SerializeField] private string materialName;
        [SerializeField] private MaterialCategory category;
        [SerializeField] private int _id;


        public string MaterialName
        {
            get => materialName;
            set => materialName = value;
        }

        public MaterialCategory Category
        {
            get => category;
            set => category = value;
        }

        public int ID
        {
            get => _id;
            set => _id = value;
        }

        public IMaterial Assign(Tile tile, GridManager manager)
        {
            return Instantiate(this,
                manager.transform.position + new Vector3(tile.X * manager.tileSize + offset.x, offset.y,
                    tile.Z * manager.tileSize + offset.z), this.transform.rotation);
        }

        public void Clear()
        {
            Destroy(gameObject);
        }
    }
}