using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace camera
{
    public class CameraManager : MonoBehaviour
    {
        public bool Is2D { get; private set; }

        [SerializeField] private GameObject camera3D;
        [SerializeField] private GameObject camera2D;

        [SerializeField] private TMP_Text buttonText;

        [SerializeField] private InputAction switchButton;

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

        public void SwitchCameraView()
        {
            Is2D = !Is2D;
            camera2D.SetActive(Is2D);
            camera3D.SetActive(!Is2D);
            buttonText.text = Is2D ? "3D" : "2D";
        }
    }
}