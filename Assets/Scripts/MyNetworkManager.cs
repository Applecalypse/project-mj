using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MyNetworkManager : MonoBehaviour
{
    private void Awake()
    {
        NetworkManager[] netList = FindObjectsOfType<NetworkManager>();
        if (netList.Length > 1)
        {
            Destroy(gameObject);
        }
    }
}
