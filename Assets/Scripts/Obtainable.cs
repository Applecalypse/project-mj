using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class Obtainable : MonoBehaviour
{   
    [Header("Item Physics")]
    private Rigidbody rb;
    private Collider col;
    private Vector3 originalScale;
    private readonly float dropForce = 3f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.isKinematic = false;
        col.isTrigger = false;
    }

    public void Obtain(Transform hand)
    {
        Debug.Log("Obtaining " + gameObject.name);

        transform.SetParent(hand);        
        rb.isKinematic = true;
        rb.useGravity = false;
        col.isTrigger = true;

        transform.localPosition = new Vector3(0, 0, 0);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        
        originalScale = transform.localScale;
        transform.localScale = transform.localScale * 0.5f;
    }

    public void Drop(GameObject player)
    {
        Debug.Log("Dropping " + gameObject.name);
        // resize back the object if needed
        transform.localScale = originalScale;

        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;
        col.isTrigger = false;

        // rb.velocity = player.GetComponent<Rigidbody>().velocity;
        rb.AddForce(player.transform.forward * dropForce, ForceMode.Impulse);
        rb.AddForce(player.transform.up * dropForce, ForceMode.Impulse);

        float random = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(random, random, random));

        

    }
}
