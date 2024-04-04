using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class exit : MonoBehaviour
{
    private bool isPlayer;
    private void OnTriggerEnter(Collider other)
    {
        isPlayer = other.CompareTag("Player");
        if (isPlayer)
        {
            GameManager.Instance.isEscaped.Value = true;
            GameManager.Instance.OnPlayerDeath();
        }
    }
}
