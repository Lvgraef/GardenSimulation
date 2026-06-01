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

    [SerializeField] private InputAction rotate;

    [SerializeField] private Origin origin;

    [SerializeField] private Transform mainCamera;


    void Start()
    {
        transform.position = origin.originPosition;
        mainCamera.position = new Vector3(mainCamera.position.x, mainCamera.position.y, -origin.originDistance);
        transform.rotation = Quaternion.Euler(origin.originRotation);

        rotate.Enable();
    }

    void Update()
    {
        var rotation = rotate.ReadValue<Vector2>();

        transform.Rotate(0, rotation.x * Time.deltaTime, 0, Space.World);
        transform.Rotate(rotation.y * Time.deltaTime, 0, 0, Space.Self);
    }
}