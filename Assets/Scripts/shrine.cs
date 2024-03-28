using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shrine : MonoBehaviour
{
    [SerializeField] private GameObject[] exit;

    public void openExit()
    {
        foreach (var e in exit)
        {
            Destroy(e);
        }
    }
}
