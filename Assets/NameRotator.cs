using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameRotator : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    private Vector3 offset = new Vector3(0, 180, 180);

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(cameraTransform);
        transform.Rotate(offset);
    }
}
