using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace camera
{
    /// <summary>
    /// Manages the switching between camera modes
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        public bool Is2D { get; private set; }

        [SerializeField] private GameObject camera3D;
        [SerializeField] private GameObject camera2D;

        [SerializeField]
        private Camera cameraComponent3D;
        [SerializeField]
        private Camera cameraComponent2D;
        

        [SerializeField] private InputAction switchButton;
        
        [SerializeField] private Image buttonImage;
        [SerializeField] private Sprite icon3D;
        [SerializeField] private Sprite icon2D;

        private void Start()
        {
            switchButton.Enable();
        }

        private void Update()
        {
            if (switchButton.WasPerformedThisFrame())
            {
                SwitchCameraView();
            }
        }

        /// <summary>
        /// Switch the view between 2D and 3D
        /// </summary>
        public void SwitchCameraView()
        {
            Is2D = !Is2D;
            camera2D.SetActive(Is2D);
            camera3D.SetActive(!Is2D);

            
            buttonImage.sprite = Is2D ? icon3D : icon2D;
            // buttonText.text = Is2D ? "3D" : "2D";
        }

        public Camera GetCurrentCamera()
        {
            return Is2D ? cameraComponent2D : cameraComponent3D;
        }
    }
}