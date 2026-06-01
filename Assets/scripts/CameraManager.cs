using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [Serializable]
    private class Origin
    {
        public Vector3 originPosition;
        public Vector2 originRotation;
        public float originDistance;
    }
    
    [SerializeField]
    private InputAction rotate;
    
    
    [SerializeField]
    private Origin origin;

    [SerializeField] 
    private Transform mainCamera;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
