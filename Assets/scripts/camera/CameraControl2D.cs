using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

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
        private Vector2 CurrentPosition;
        [SerializeField] private Camera cameraComponent;

        [SerializeField] private Origin origin;
        [SerializeField] private InputAction pan;
        [SerializeField] private InputAction zoom;
        [SerializeField] private InputAction reset;
        [SerializeField]private float panSpeedTouch = 0.5f;
        [SerializeField]private float zoomSpeedTouch = 0.05f;


        private const float MinZoom = 1;
        private const float MaxZoom = 15;

        private void Start()
        {
            EnhancedTouchSupport.Enable();
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
            zooming();
            panning();
        }




        // Zoom function for keyboard with = and - keys, mouse scroll wheel, and touch pinch gesture for touchscreen
        private void zooming() {
            if (EventSystem.current.IsPointerOverGameObject()) { return; }

            var zoomAmount = zoom.ReadValue<float>() * Time.deltaTime;
            var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

            if (activeTouches.Count == 2)
            {
                var touch0 = activeTouches[0];
                var touch1 = activeTouches[1];

                float zoomDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);
                float previousZoomDistance = Vector2.Distance(
                    touch0.screenPosition - touch0.delta,
                    touch1.screenPosition - touch1.delta
                );
                float zoom = previousZoomDistance - zoomDistance;
                zoomAmount += zoom * zoomSpeedTouch;
            }

            cameraComponent.orthographicSize = Math.Clamp(cameraComponent.orthographicSize + zoomAmount, MinZoom, MaxZoom);
        }



        // Pan function for arrow keys, middle mouse button, and touch three-finger drag gesture for touchscreen
        private void panning() {
            var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
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

            if (activeTouches.Count == 3)
            {
                Vector2 avgDeltaTouch =
                    (activeTouches[0].delta +
                     activeTouches[1].delta +
                     activeTouches[2].delta) / 3f;

                transform.Translate(
                    -avgDeltaTouch.x * panSpeedTouch * Time.deltaTime,
                    0,
                    -avgDeltaTouch.y * panSpeedTouch * Time.deltaTime,
                    Space.World);
            }
        }

        // Reset the camera position and size to the origin values
        private void ResetTransform()
        {
            transform.position = origin.originPosition;
        }
    }
}