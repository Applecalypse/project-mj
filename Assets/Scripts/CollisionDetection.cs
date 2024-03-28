using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// UNUSED
// Will keep this here for now
// right now "Throwable" and "Melee Weapon" individually have their own collision detection scripts
// if we want to have a generic collision detection script that can be used for both, we can use this
public class CollisionDetection : MonoBehaviour
{
    [Header("Functions")]
    [SerializeField] 
    private UnityEvent onCollisionEnterFunction;

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision detected with " + other.gameObject.name);
        onCollisionEnterFunction?.Invoke();
    }
}
