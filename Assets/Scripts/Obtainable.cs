using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obtainable : MonoBehaviour
{   
    private Rigidbody rb;
    private Collider col;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.isKinematic = false;
        col.isTrigger = false;
    }

    public void Obtain(GameObject player)
    {
        Debug.Log("Obtaining " + gameObject.name);
        transform.SetParent(player.transform);
        rb.isKinematic = true;
        col.isTrigger = true;
    }

    public void Drop(GameObject player)
    {
        Debug.Log("Dropping " + gameObject.name);
        transform.SetParent(null);

        rb.isKinematic = false;
        col.isTrigger = false;

        // rb.velocity = player.GetComponent<Rigidbody>().velocity;
        rb.AddForce(player.transform.forward * 2f, ForceMode.Impulse);
        rb.AddForce(player.transform.up * 2f, ForceMode.Impulse);

        float random = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(random, random, random));

    }
}