using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// UNUSED
// Will keep this here for now
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
