using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// credits: https://youtu.be/LtayTVAZD2M?si=1dHW7IbU2KRxcH5V
public class Interactable : MonoBehaviour
{
    public UnityEvent onInteract;

    public void Interact()
    {
        Debug.Log("Interacting with " + gameObject.name);
        onInteract?.Invoke();
    }
}
