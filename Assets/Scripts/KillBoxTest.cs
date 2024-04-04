using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class KillBoxTest : NetworkBehaviour
{
    // Start is called before the first frame update
    //[SerializeField] private HealthController bob;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("kill box");
            other.gameObject.GetComponent<HealthController>().TakeDamageServerRpc(1000);
        }
    }
}
