using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// https://youtu.be/F20Sr5FlUlE?si=zOHVPfghQtbm6rs3
public class Throwable : MonoBehaviour
{
    [Header("Throwing")]
    private Rigidbody rb;
    private Collider col;
    private readonly float forwardThrowForce = 10f;
    private readonly float upwardThrowForce = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Throw(Transform camera, Transform hand)
    {
        rb.isKinematic = false;
        rb.transform.parent = null;
        rb.useGravity = true;
        col.enabled = true;

        // default direction is the direction that the camera is facing
        Vector3 throwDirection = camera.forward;
        // perform Raycast to calculate the direction from Hand to the target
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, 100f))
        {
            throwDirection = (hit.point - hand.position).normalized;
        }

        Vector3 throwForce = (throwDirection * forwardThrowForce) + (camera.transform.up * upwardThrowForce);
        rb.AddForce(throwForce, ForceMode.Impulse);
    }
}
