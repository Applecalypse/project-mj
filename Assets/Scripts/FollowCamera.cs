using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Camera Settings")]
    private readonly float rotationSpeed = 5f;
    [SerializeField] private bool rotateX = false;
    [SerializeField] private bool rotateY = true;
    [SerializeField] private bool rotateZ = false;

    private int xRotation, yRotation, zRotation;

    void Start()
    {
        if (rotateX) xRotation = 1;
        if (rotateY) yRotation = 1;
        if (rotateZ) zRotation = 1;
    }

    void Update()
    {
        CameraRotation();
    }

    void CameraRotation()
    {
        if (cameraTransform == null) { return; }


        // // Makes this object face the direction of the camera
        float targetAngleX = cameraTransform.eulerAngles.x * xRotation;
        float targetAngleY = cameraTransform.eulerAngles.y * yRotation;
        float targetAngleZ = cameraTransform.eulerAngles.z * zRotation;

        // Form quaternion representation of the target angle
        Quaternion targetRotation = Quaternion.Euler(targetAngleX, targetAngleY, targetAngleZ);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
