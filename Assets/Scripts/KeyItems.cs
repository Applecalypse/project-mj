using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KeyItems : NetworkBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("collide with a key item");
            GameManager.Instance.IncreaseKeyCount();
            Destroy(gameObject);
        }
    }

    [ServerRpc]
    private void DestroyItemServerRpc(NetworkObjectReference obj)
    {
        if (obj.TryGet(out NetworkObject nobj))
        {
            Destroy(nobj.gameObject);
        }
    }

}
