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
            Building,
            NonPermeable,
            SemiPermeable,
            Bare,
            Flowers,
            Grass,
            Shrubs,
            Tree
        }
        
        private GameObject _material;
        
        public Category category;
        
        public Sprite sprite;

        public Vector3 offset = new(0.5f, 0.05f, 0.5f);
    }
}