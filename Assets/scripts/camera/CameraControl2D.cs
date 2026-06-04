using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace camera
{
    /// <summary>
    /// Controls the 2D camera
    /// </summary>
    public class CameraControl2D : MonoBehaviour
    {
        [Serializable]
        private class Origin
        {
            public Vector3 originPosition;
            public float originSize = 5;
        }

        [SerializeField] private Camera cameraComponent;

        [SerializeField] private Origin origin;

        [SerializeField] private InputAction pan;
        [SerializeField] private InputAction zoom;
        [SerializeField] private InputAction reset;
        
        private const float MinZoom = 2;
        private const float MaxZoom = 50;

        private void Start()
        {
            ResetTransform();

            pan.Enable();
            zoom.Enable();
            reset.Enable();
        }

        private void Update()
        {
            // Reset
            if (reset.IsPressed())
            {
                ResetTransform();
            }
            
            // Zoom
            var zoomAmount = zoom.ReadValue<float>() * Time.deltaTime;
            cameraComponent.orthographicSize = Math.Clamp(cameraComponent.orthographicSize + zoomAmount, MinZoom, MaxZoom);
            
            // Pan
            var panAmount = pan.ReadValue<Vector2>() * Time.deltaTime;
            transform.Translate(panAmount.x, 0, panAmount.y, Space.World);
        }

        private void ResetTransform()
        {
            transform.position = origin.originPosition;
        }
    }
}