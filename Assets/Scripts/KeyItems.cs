using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyItems : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("collide with a key item");
            GameManager.Instance.increaseKeyCount();
            Destroy(gameObject);
        }
    }
}
