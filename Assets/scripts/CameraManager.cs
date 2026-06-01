using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class CameraManager : MonoBehaviour
{
    [Serializable]
    private class Origin
    {
        public Vector3 originPosition;
        public Vector2 originRotation;
        public float originDistance;
    }

    [SerializeField] private InputAction rotate;
    [SerializeField] private InputAction zoom;
    
    [SerializeField] private Origin origin;

    [SerializeField] private Transform mainCamera;

    private const float MinZoom = 2;
    private const float MaxZoom = 50;

    void Start()
    {
        transform.position = origin.originPosition;
        mainCamera.position = new Vector3(mainCamera.position.x, mainCamera.position.y, -origin.originDistance);
        transform.rotation = Quaternion.Euler(origin.originRotation);

        rotate.Enable();
        zoom.Enable();
    }

    void Update()
    {
        // rotation
        var rotation = rotate.ReadValue<Vector2>();

        transform.Rotate(0, rotation.x * Time.deltaTime, 0, Space.World);
        transform.Rotate(rotation.y * Time.deltaTime, 0, 0, Space.Self);
        
        // zoom
        var zoomAmount = zoom.ReadValue<float>();

        if (Math.Abs(mainCamera.localPosition.z + zoomAmount) < MaxZoom && Math.Abs(mainCamera.localPosition.z + zoomAmount) > MinZoom)
        {
            mainCamera.Translate(0, 0, zoomAmount);
        }
    }
}