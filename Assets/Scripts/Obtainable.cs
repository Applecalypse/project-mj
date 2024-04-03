using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using Unity.Netcode;

public class Obtainable : NetworkBehaviour
{   
    [Header("Item Physics")]
    private Rigidbody rb;
    private Collider col;
    private readonly float dropForce = 3f;

    [Header("Network")]
    private GameObject newObtainable = null;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.isKinematic = false;
        col.isTrigger = false;
    }

    // public void Obtain(Transform hand)
    // {
    //     Debug.Log("Obtaining " + gameObject.name);
        
        
    //     transform.SetParent(hand.transform);
    //     rb.isKinematic = true;
    //     rb.useGravity = false;
    //     col.isTrigger = true;

    //     transform.localPosition = new Vector3(0, 0, 0);
    //     transform.localRotation = Quaternion.Euler(0, 0, 0);
        
    //     originalScale = transform.localScale;
    //     transform.localScale = transform.localScale * 0.5f;
    // }

    public GameObject Obtain(Transform hand)
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        col.enabled = false;
        GetComponent<NetworkObject>().enabled = false;
        GetComponent<ClientNetworkTransform>().enabled = false;

        newObtainable = Instantiate(gameObject, hand.position, hand.rotation, hand);
        newObtainable.name = gameObject.name;
        newObtainable.transform.parent = hand.transform;
        newObtainable.GetComponent<NetworkObject>().enabled = true;
        newObtainable.GetComponent<ClientNetworkTransform>().enabled = true;
        return newObtainable;
    }
    
    public void Drop(GameObject player)
    {
        Debug.Log("Dropping " + gameObject.name);

        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;
        col.enabled = true;
        GetComponent<NetworkObject>().enabled = false;
        GetComponent<ClientNetworkTransform>().enabled = false;

        transform.parent = null;
        GetComponent<NetworkObject>().enabled = true;
        GetComponent<ClientNetworkTransform>().enabled = true;
        rb.AddForce(player.transform.forward * dropForce, ForceMode.Impulse);
        rb.AddForce(player.transform.up * dropForce, ForceMode.Impulse);

        float random = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(random, random, random));
    }
}
