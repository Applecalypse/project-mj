using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// https://youtu.be/F20Sr5FlUlE?si=zOHVPfghQtbm6rs3
public class Throwable : MonoBehaviour
{

    // throwing
    private Rigidbody rb;
    private Collider col;
    private readonly float forwardThrowForce = 20f;
    private readonly float upwardThrowForce = 5f;

    // hitting the target
    private bool collisionEnabled = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        
    }

    public void Throw(Transform camera, Transform hand)
    {
        rb.isKinematic = false;
        rb.transform.parent = null;
        rb.useGravity = true;
        col.isTrigger = false;
        collisionEnabled = true;

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

    void OnCollisionEnter(Collision other)
    {
        if (!collisionEnabled) { return; }
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Hit the enemy");
            // other.gameObject.GetComponent<Enemy>().TakeDamage(10);
        }
        else { collisionEnabled = false; Debug.Log("Hit something else"); }

        Destroy(gameObject, 1f);
    }
}
