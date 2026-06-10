using UnityEngine;

namespace GridSystem
{
    
    /// <summary>
    /// This is the class where the material object belongs too
    /// </summary>
    public class Material : MonoBehaviour
    {
        public enum Category
        {
            NonPermeable,
            SemiPermeable,
            Bare,
            Flowers,
            Grass,
            Shrubs,
            Tree,
            Building
        }

        public string materialName;
        
        private GameObject _material;
        
        public Category category;
        
        public Sprite sprite;

        public Vector3 offset = new(0.25f, 0.05f, 0.25f);
    }
}