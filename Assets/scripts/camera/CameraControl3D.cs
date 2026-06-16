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
        private float rotationSpeedMouse = 0.001f;
        private float rotationSpeedTouch = 0.1f;
        private float panSpeedTouch = 0.1f;
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

            bool isTouchRotating = false;
            if (Touchscreen.current != null) {
                int count = 0;
                foreach (var r in Touchscreen.current.touches) {
                    if (r.press.IsPressed()) {
                        count++;
                    }
                }
                isTouchRotating = count == 2;
            }
            bool isMouseRotating = Mouse.current != null && Mouse.current.rightButton.isPressed;
            bool isArrowRotating = Keyboard.current != null &&
            (Keyboard.current.upArrowKey.isPressed ||
             Keyboard.current.downArrowKey.isPressed ||
             Keyboard.current.leftArrowKey.isPressed ||
             Keyboard.current.rightArrowKey.isPressed);

            if (isArrowRotating) {
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
            if (isTouchRotating)
            {
                var rotation = rotate.ReadValue<Vector2>() * rotationSpeedTouch;
                transform.Rotate(0, rotation.x, 0, Space.World);
                transform.Rotate(rotation.y, 0, 0, Space.Self);
            }



            if (!EventSystem.current.IsPointerOverGameObject())
            {
                // height
                var heightAmount = height.ReadValue<float>() * Time.deltaTime;

                if (Math.Abs(mainCamera.localPosition.y + heightAmount) > 0)
                {
                    transform.Translate(0, heightAmount, 0, Space.World);
                }

                // zoom
                bool isKeyboardZooming = Keyboard.current != null &&
                                  (Keyboard.current.equalsKey.isPressed ||
                                  Keyboard.current.minusKey.isPressed);
                                 
                bool isMouseZooming = Mouse.current != null && Mouse.current.scroll.ReadValue().y != 0;
                if (isMouseZooming || isKeyboardZooming)
                {
                    if (heightAmount == 0)
                    {
                        var zoomAmount = zoom.ReadValue<float>() * Time.deltaTime;

                        if (Math.Abs(mainCamera.localPosition.z + zoomAmount) < MaxZoom &&
                            Math.Abs(mainCamera.localPosition.z + zoomAmount) > MinZoom)
                        {
                            mainCamera.localPosition = new Vector3(mainCamera.localPosition.x, mainCamera.localPosition.y,
                                Math.Clamp(mainCamera.localPosition.z + zoomAmount, -MaxZoom, -MinZoom));
                        }
                    }
                }
            }

            // pan
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

            if (isTouchPanning) {
                var panAmount = pan.ReadValue<Vector2>() * Time.deltaTime;

                var forward = transform.forward;
                forward.y = 0;
                forward.Normalize();

                var right = transform.right;
                right.y = 0;
                right.Normalize();

                transform.Translate(right * panAmount.x * panSpeedTouch + forward * panAmount.y * panSpeedTouch, Space.World);
            }

            
        }

        void ResetTransform()
        {
            transform.position = Vector3.zero;
        }
    }
}