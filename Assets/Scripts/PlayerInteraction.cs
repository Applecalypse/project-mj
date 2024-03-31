using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

// credits: 
// Interaction - https://youtu.be/LtayTVAZD2M?si=1dHW7IbU2KRxcH5V
// Obtainable - https://youtu.be/2IhzPTS4av4?si=AhXazq9DUSgjoH5H
// PickUp - https://youtu.be/8kKLUsn7tcg?si=CMy838TxCqT5vdYy
public class PlayerInteraction : NetworkBehaviour
{
    [Header("Testing")]
    [SerializeField] private bool enablePlayerControls = true;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    private readonly float interactionDistance = 3f;

    [Header("Player Input")]
    private PlayerInput playerInput;
    private InputAction interactAction, prayAction, dropAction, shootAction;

    [Header("Item Holding")]
    private GameObject heldItem = null;
    [SerializeField] private Transform handLocation;

    [Header("Camera Flash")]
    [SerializeField] private GameObject cameraFlashObject;
    private CameraFlash cameraFlash;
    private Collider cameraFlashCollider;

    [Header("Player Model")]
    private Animator animator;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        interactAction = playerInput.actions["Interact"];
        dropAction = playerInput.actions["Drop"];
        shootAction = playerInput.actions["Shoot"];
        animator = GetComponentInParent<Animator>();
        
        /*
        prayAction.started += context =>
        {
            if (context.interaction is HoldInteraction)
            {
                Debug.Log("Start Praying");
                animator.SetBool("isPraying", true);
            }
        };

        prayAction.performed += context =>
        {
            if (context.interaction is HoldInteraction)
            {
                Pray();

            }
        };
        
        prayAction.canceled += context =>
        {
            Debug.Log("Stop Praying");
            animator.SetBool("isPraying", false);
        };*/

        cameraFlashCollider = cameraFlashObject.GetComponent<Collider>();
        cameraFlash = cameraFlashObject.GetComponent<CameraFlash>();
    }

    void Update()
    {
        if (!enablePlayerControls) { return; }
        
        Interact();
        Drop();
        Shoot();
        Vector3 rayOrigin = cameraTransform.position + (cameraTransform.forward * 1f);
        //Debug.DrawRay(rayOrigin, cameraTransform.forward * 5, Color.red, 100f);
    }

    public void onPray(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Debug.Log("Start Praying");
                animator.SetBool("isPraying", true);
                break;
            case InputActionPhase.Performed:
                Debug.Log("Perform Praying");
                Pray();
                break;
            case InputActionPhase.Canceled:
                Debug.Log("Stop Praying");
                animator.SetBool("isPraying", false);
                break;
        }
    }

    


    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false; 
            return;
        }
        
        base.OnNetworkSpawn();
    }
    
    void Interact()
    {
        if (!interactAction.triggered) { return; }
        Debug.Log("Interacting");

        // disable the camera flash collider
        cameraFlashCollider.enabled = false;

        // Perform a raycast
        RaycastHit hit;
        Vector3 rayOrigin = cameraTransform.position + (cameraTransform.forward * 0f);
        Physics.Raycast(rayOrigin, cameraTransform.forward, out hit, interactionDistance);
        // Debug.DrawRay(rayOrigin, cameraTransform.forward * interactionDistance, Color.red, 100f);

        // enable back the camera flash collider
        cameraFlashCollider.enabled = true;
        
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

    void Pray()
    {
        RaycastHit hit;
        Vector3 rayOrigin = cameraTransform.position + (cameraTransform.forward * 1.2f);
        Physics.Raycast(rayOrigin, cameraTransform.forward, out hit, 5);
        Debug.DrawRay(rayOrigin, cameraTransform.forward * 5, Color.red, 100f);
        if ( hit.transform == null  ) { return; }
        
        Debug.Log("Pointing at " + hit.transform.gameObject.name);

        Interactable targetInteractable = hit.transform.GetComponent<Interactable>();
        if (targetInteractable != null && GameManager.Instance.keyItemCount.Value == 3) { targetInteractable.Interact(); return; }
    }

    void Shoot()
    {
        if (!shootAction.triggered) { return; }
        // Debug.Log("Shooting");

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
            DefaultAction();
        }
    }

    void Drop()
    {
        if (!dropAction.triggered) { return; }
        // Debug.Log("Dropping");

        if (heldItem != null)
        {
            Obtainable targetObtainable = heldItem.GetComponent<Obtainable>();
            targetObtainable.Drop(gameObject);
            heldItem = null;
        }
    }

    
    void DefaultAction()
    {
        bool flashSuccessful = cameraFlash.Flash();

        // if (flashSuccessful) { Debug.Log("Player Interaction: Flash Hit"); }
        // else { Debug.Log("Player Interaction: No target in range"); }
    }
}
