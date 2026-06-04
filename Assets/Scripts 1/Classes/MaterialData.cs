using UnityEngine;
using System;
namespace GardenSimulation
{
    [CreateAssetMenu(menuName = "GardenSimulation/GardenMesh", order = 0)]
    public class MaterialData : ScriptableObject
    {
        // public Guid Id {get; set;}
        // public string Meshname {get; set;}
        // public Mesh Mesh {get; set;}
        
        [SerializeField] public Sprite sprite;
        // public bool Transparent {get; set;}
        // public Vector3 Size {get; set;}
    }
}
