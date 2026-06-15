using System;
using UnityEngine;
using UnityEngine.EventSystems;
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

        [SerializeField] private float panSpeedTouch = 0.5f;
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
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var zoomAmount = zoom.ReadValue<float>() * Time.deltaTime;
                cameraComponent.orthographicSize = Math.Clamp(cameraComponent.orthographicSize + zoomAmount, MinZoom, MaxZoom);
            }

            // Pan
            bool isTouchPanning = false;
            if (Touchscreen.current != null)
            {
                int count = 0;
                foreach (var p in Touchscreen.current.touches)
                {
                    if (p.press.IsPressed())
                    {
                        count++;
                    }
                }
                isTouchPanning = count == 3;
            }

            bool isArrowPanning = Keyboard.current != null &&
                            (Keyboard.current.wKey.isPressed ||
                             Keyboard.current.aKey.isPressed ||
                             Keyboard.current.dKey.isPressed ||
                             Keyboard.current.sKey.isPressed);
            bool isMiddleMousePanning = Mouse.current != null && Mouse.current.middleButton.isPressed;

            if (isArrowPanning || isMiddleMousePanning)
            {
                var panAmount = pan.ReadValue<Vector2>() * Time.deltaTime;
                transform.Translate(panAmount.x, 0, panAmount.y, Space.World);
            }

            if (isTouchPanning)
            {
                var panAmount = pan.ReadValue<Vector2>();
                transform.Translate(panAmount.x * panSpeedTouch, 0, panAmount.y * panSpeedTouch, Space.World);
            }
        }

        private void ResetTransform()
        {
            transform.position = origin.originPosition;
        }
    }
}