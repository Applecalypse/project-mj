using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

// credits: 
// Interaction - https://youtu.be/LtayTVAZD2M?si=1dHW7IbU2KRxcH5V
// Obtainable - https://youtu.be/2IhzPTS4av4?si=AhXazq9DUSgjoH5H
// PickUp - https://youtu.be/8kKLUsn7tcg?si=CMy838TxCqT5vdYy
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    private readonly float interactionDistance = 3f;

    // controls
    private PlayerInput playerInput;
    private InputAction interactAction, dropAction, shootAction;

    // inventory
    private GameObject heldItem = null;
    [SerializeField] private Transform handLocation;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        interactAction = playerInput.actions["Interact"];
        dropAction = playerInput.actions["Drop"];
        shootAction = playerInput.actions["Shoot"];
    }

    void Update()
    {
        Interact();
        Drop();
        Shoot();
    }

    
    void Interact()
    {
        if (!interactAction.triggered) { return; }
        Debug.Log("Interacting");

        // Perform a raycast
        RaycastHit hit;
        Vector3 rayOrigin = cameraTransform.position + (cameraTransform.forward * 0f);
        Physics.Raycast(rayOrigin, cameraTransform.forward, out hit, interactionDistance);
        // Debug.DrawRay(rayOrigin, cameraTransform.forward * interactionDistance, Color.red, 100f);
        if ( hit.transform == null  ) { return; }
        
        Debug.Log("Pointing at " + hit.transform.gameObject.name);

        Interactable targetInteractable = hit.transform.GetComponent<Interactable>();
        if (targetInteractable != null) { targetInteractable.Interact(); return; }
        
        Obtainable targetObtainable = hit.transform.GetComponent<Obtainable>();
        if (targetObtainable != null && heldItem == null)
        {
            targetObtainable.Obtain(handLocation);
            heldItem = targetObtainable.gameObject;
        }
    }

    void Shoot()
    {
        if (!shootAction.triggered) { return; }
        Debug.Log("Shooting");

        if (heldItem != null) // if holding an item
        {
            Throwable targetThrowable = heldItem.GetComponent<Throwable>();
            if (targetThrowable != null) { targetThrowable.Throw(cameraTransform, handLocation); }
            
            Consumable targetConsumable = heldItem.GetComponent<Consumable>();
            if (targetConsumable != null) { targetConsumable.Use(); }

            heldItem = null;
        }
        else
        {
            CameraFlash();
        }
    }

    void Drop()
    {
        if (!dropAction.triggered) { return; }
        Debug.Log("Dropping");

        if (heldItem != null)
        {
            Obtainable targetObtainable = heldItem.GetComponent<Obtainable>();
            targetObtainable.Drop(gameObject);
            heldItem = null;
        }
    }

    
    void CameraFlash()
    {
        Debug.Log("Camera flash");
    }
}
