using System;
using Unity.MLAgents.Sensors;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
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
        [SerializeField] private float rotationSpeedMouse = 0.001f;
        [SerializeField] private float rotationSpeedTouch = 0.1f;
        [SerializeField] private float panSpeedTouch = 0.5f;
        [SerializeField] private float zoomSpeedTouch = 0.05f;
        
        private const float MinZoom = 1;
        private const float MaxZoom = 25;

        void Start()
        {
            transform.position = origin.originPosition;
            mainCamera.position = new Vector3(mainCamera.position.x, mainCamera.position.y, -origin.originDistance);
            transform.rotation = Quaternion.Euler(origin.originRotation);
            EnhancedTouchSupport.Enable();
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
            panning();
            rotating();
            zooming();
            
            
        }

        ///rotate function that handles mouse, touch and keyboard input for rotating the camera
        private void rotating() {
            if (EventSystem.current.IsPointerOverGameObject()) { return; }
            var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
            bool isMouseRotating = Mouse.current != null && Mouse.current.rightButton.isPressed;

            bool isArrowRotating = Keyboard.current != null &&
            (Keyboard.current.upArrowKey.isPressed ||
             Keyboard.current.downArrowKey.isPressed ||
             Keyboard.current.leftArrowKey.isPressed ||
             Keyboard.current.rightArrowKey.isPressed);

            if (isArrowRotating)
            {
                var rotation = rotate.ReadValue<Vector2>() * Time.deltaTime;
                transform.Rotate(0, rotation.x, 0, Space.World);
                transform.Rotate(rotation.y, 0, 0, Space.Self);
            }

            if (isMouseRotating)
            {
                var rotation = rotate.ReadValue<Vector2>() * rotationSpeedMouse;
                transform.Rotate(0, rotation.x, 0, Space.World);
                transform.Rotate(rotation.y, 0, 0, Space.Self);
            }

            if (activeTouches.Count == 2) {
                Vector2 avgDeltaTouch =
                    (activeTouches[0].delta +
                     activeTouches[1].delta) / 2f;
                transform.Rotate(0, avgDeltaTouch.x * rotationSpeedTouch, 0, Space.World);
                transform.Rotate(avgDeltaTouch.y * rotationSpeedTouch, 0, 0, Space.Self);
            }
        }

        ///zoom function for keyboard with = and - keys, mouse scroll wheel and touch pinch gesture for touchscreen
        private void zooming() {
            if (EventSystem.current.IsPointerOverGameObject()) { return; }
            var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
            var zoomAmount = zoom.ReadValue<float>() * Time.deltaTime;

            if (activeTouches.Count == 2) { 
                var touch0 = activeTouches[0];
                var touch1 = activeTouches[1];

                float zoomDistance = Vector2.Distance(
                    touch0.screenPosition, 
                    touch1.screenPosition
                    );
                float previousZoomDistance = Vector2.Distance(
                    touch0.screenPosition - touch0.delta, 
                    touch1.screenPosition - touch1.delta
                    );
                float zoom = zoomDistance - previousZoomDistance;
                zoomAmount += zoom * zoomSpeedTouch;
            }
            if (Math.Abs(mainCamera.localPosition.z + zoomAmount) < MaxZoom &&
                           Math.Abs(mainCamera.localPosition.z + zoomAmount) > MinZoom)
            {
                mainCamera.localPosition = new Vector3(mainCamera.localPosition.x, mainCamera.localPosition.y,
                    Math.Clamp(mainCamera.localPosition.z + zoomAmount, -MaxZoom, -MinZoom));
            }
        }

        ///pan function for keyboard with WASD, mouse middle button and touch three finger drag gesture for touchscreen
        private void panning() {
            if (EventSystem.current.IsPointerOverGameObject()) { return; }
            var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
            bool isArrowPanning = Keyboard.current != null &&
                            (Keyboard.current.wKey.isPressed ||
                             Keyboard.current.aKey.isPressed ||
                             Keyboard.current.dKey.isPressed ||
                             Keyboard.current.sKey.isPressed);
            bool isMiddleMousePanning = Mouse.current != null && Mouse.current.middleButton.isPressed;

            if (isArrowPanning || isMiddleMousePanning) {
                var panAmount = pan.ReadValue<Vector2>() * Time.deltaTime;

                var forward = transform.forward;
                forward.y = 0;
                forward.Normalize();

                var right = transform.right;
                right.y = 0;
                right.Normalize();

                transform.Translate(right * panAmount.x + forward * panAmount.y, Space.World);
            }
            if (activeTouches.Count == 3) { 
                Vector2 avgDeltaTouch =
                    (activeTouches[0].delta +
                     activeTouches[1].delta +
                     activeTouches[2].delta) / 3f;

                Vector3 forward = transform.forward;
                forward.y = 0;
                forward.Normalize();

                Vector3 right = transform.right;
                right.y = 0;
                right.Normalize();

                Vector3 move = (-right * avgDeltaTouch.x + -forward * avgDeltaTouch.y)
                    * panSpeedTouch * Time.deltaTime;

                transform.position += move;
            }
        }

        void ResetTransform()
        {
            transform.position = Vector3.zero;
        }
    }
}