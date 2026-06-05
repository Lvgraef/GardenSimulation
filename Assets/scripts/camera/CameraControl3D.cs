using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace camera
{
    /// <summary>
    /// Controls the 3D camera
    /// </summary>
    public class CameraControl3D : MonoBehaviour
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
        [SerializeField] private InputAction pan;
        [SerializeField] private InputAction height;
        [SerializeField] private InputAction reset;
    
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
            pan.Enable();
            height.Enable();
            reset.Enable();
        }

        void Update()
        {
            // reset
            if (reset.IsPressed())
            {
                ResetTransform();
            }
        
            // rotation
            var rotation = rotate.ReadValue<Vector2>() * Time.deltaTime;

            transform.Rotate(0, rotation.x, 0, Space.World);
            transform.Rotate(rotation.y, 0, 0, Space.Self);
        
            // height
            var heightAmount = height.ReadValue<float>() * Time.deltaTime;

            if (Math.Abs(mainCamera.localPosition.y + heightAmount) > 0)
            {
                transform.Translate(0, heightAmount, 0, Space.World);
            }
        
            // zoom
            if (heightAmount == 0)
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    var zoomAmount = zoom.ReadValue<float>() * Time.deltaTime;

                    if (Math.Abs(mainCamera.localPosition.z + zoomAmount) < MaxZoom && Math.Abs(mainCamera.localPosition.z + zoomAmount) > MinZoom)
                    {
                        mainCamera.localPosition = new Vector3(mainCamera.localPosition.x, mainCamera.localPosition.y, Math.Clamp(mainCamera.localPosition.z + zoomAmount, -MaxZoom, -MinZoom));
                    }
                }
            }
        
            // pan
            var panAmount = pan.ReadValue<Vector2>() * Time.deltaTime;

            var forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            var right = transform.right;
            right.y = 0;
            right.Normalize();
        
            transform.Translate(right * panAmount.x + forward * panAmount.y, Space.World);
        }
    
        void ResetTransform()
        {
            transform.position = Vector3.zero;
        }
    }
}