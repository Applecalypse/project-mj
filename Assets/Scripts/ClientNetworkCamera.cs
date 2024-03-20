using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class ClientNetworkCamera : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);
        }
        base.OnNetworkSpawn();
    }
}
