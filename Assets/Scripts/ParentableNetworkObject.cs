using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ParentableNetworkObject : NetworkBehaviour
{
    [SerializeField] private Transform parentLocation;
    [SerializeField] private GameObject parentableObjectPrefab;
    private GameObject parentableObject;

    private void Awake()
    {
        GameObject gameObj = Instantiate(parentableObjectPrefab, parentLocation.position, parentLocation.rotation, parentLocation);
        gameObj.transform.parent = parentableObjectPrefab.transform;
        gameObj.GetComponent<NetworkObject>().enabled = true;
        gameObj.GetComponent<ClientNetworkTransform>().enabled = true;

        parentableObject = gameObj;
    }

    public GameObject GetParentableObject()
    {
        return parentableObject;
    }
}
