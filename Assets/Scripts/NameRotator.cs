using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameRotator : MonoBehaviour
{
    private Transform cameraTransform;

    private Vector3 offset = new Vector3(0, 180, 180);

    private void Start()
    {
        try
        {
            cameraTransform = Camera.main.transform;
        }
        catch (Exception e)
        {
            Debug.LogWarning("This came from NameRotator script, " +
                             "im catching and throwing this error rn if " +
                             "this is a problem later plz fix - Logs: " + e);
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(cameraTransform);
        transform.Rotate(offset);
    }
}
