using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFlash : MonoBehaviour
{
    private EnemyController target;

    // when the enemy gets in range, track the target
    void OnTriggerEnter(Collider other)
    {
        if (target != null) return; // only track one target at a time

        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null) target = enemy;
    }

    // when the enemy leaves the range, stop tracking the target
    void OnTriggerExit(Collider other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy == null) return; // if the object is not an enemy, ignore it

        if (enemy == target) target = null; // untracks the current target
    }

    // can be triggered at any time
    // will only be effective when the target is in range
    public bool Flash()
    {
        Debug.Log("CameraFlash: Flashing Camera");
        if (target == null) return false;
        
        target.Stun();
        return true;
    }
}
