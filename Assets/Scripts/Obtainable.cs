using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class Obtainable : NetworkBehaviour
{   
    [Header("Item Physics")]
    private Rigidbody rb;
    private Collider col;
    private readonly float dropForce = 3f;
    private bool onPickUp;
    private Transform hand;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.isKinematic = false;
        col.isTrigger = false;
    }

    private void Update()
    {
        if (!onPickUp || !hand) return;
        var transform1 = transform;
        var transform2 = hand.transform;
        transform1.position = transform2.position;
        transform1.rotation = transform2.rotation;
    }

    public GameObject Obtain(Transform _hand)
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        col.enabled = false;
        onPickUp = true;
        hand = _hand;

        return gameObject;
    }
    
    public void Drop(GameObject player)
    {
        Debug.Log("Dropping " + gameObject.name);

        rb.isKinematic = false;
        rb.useGravity = true;
        col.enabled = true;
        rb.AddForce(player.transform.forward * dropForce, ForceMode.Impulse);
        rb.AddForce(player.transform.up * dropForce, ForceMode.Impulse);

        float random = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(random, random, random));
    }

    public void StopPickUp()
    {
        onPickUp = false;
        hand = null;
    }
}
