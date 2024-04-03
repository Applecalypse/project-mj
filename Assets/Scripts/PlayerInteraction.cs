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
    // [SerializeField] private Transform handLocation;
    [SerializeField] private GameObject hand;

    [Header("Camera Flash")]
    [SerializeField] private GameObject cameraFlashObject;
    private CameraFlash cameraFlash;
    private Collider cameraFlashCollider;

    [Header("Player Model")]
    private Animator animator;
    private PlayerController playerController;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        interactAction = playerInput.actions["Interact"];
        dropAction = playerInput.actions["Drop"];
        shootAction = playerInput.actions["Shoot"];
        animator = GetComponentInParent<Animator>();
        playerController = GetComponent<PlayerController>();
        
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

        hand = GetComponent<ParentableNetworkObject>().GetParentableObject();
    }

    void Update()
    {
        if (!enablePlayerControls || playerController.isDead) { return; }
        
        Interact();
        Drop();
        Shoot();
        Vector3 rayOrigin = cameraTransform.position + (cameraTransform.forward * 1f);
        //Debug.DrawRay(rayOrigin, cameraTransform.forward * 5, Color.red, 100f);
    }

    public void onPray(InputAction.CallbackContext context)
    {
        if (playerController.isDead) { return; }
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

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (playerController.isDead) { return; }
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Debug.Log("Start Interact");
                if (context.interaction is PressInteraction) { break; }
                if (!CheckIfPointingAtPlayer())
                { Debug.Log("Pointed At Player"); interactAction.Reset(); }
                break;
            case InputActionPhase.Performed:
                if (context.interaction is HoldInteraction) { Debug.Log("Done Holding Interact"); Revive(); }
                break;
            case InputActionPhase.Canceled:
                Debug.Log("Stopped Holding Interact");
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
        
        // Debug.Log("Pointing at " + hit.transform.gameObject.name);

        Interactable targetInteractable = hit.transform.GetComponent<Interactable>();
        if (targetInteractable != null && !hit.transform.gameObject.CompareTag("shrine")) { targetInteractable.Interact(); return; }
        
        Obtainable targetObtainable = hit.transform.GetComponent<Obtainable>();
        if (targetObtainable != null && heldItem == null)
        {
            heldItem = targetObtainable.Obtain(hand.transform);
            if (heldItem) 
            {
                // hit.transform.GetComponent<NetworkObject>().Despawn();
                Destroy(hit.transform.gameObject);
            }
        }
        
    }


    void Revive()
    {
        RaycastHit hit;
        Vector3 rayOrigin = cameraTransform.position + (cameraTransform.forward);
        Physics.Raycast(rayOrigin, cameraTransform.forward, out hit, 5);
        Debug.DrawRay(rayOrigin, cameraTransform.forward * 5, Color.red, 100f);
        
        if ( hit.transform == null  ) { return; }
        
        Debug.Log("Revived : " + hit.transform.gameObject.name);
        
        if (hit.transform.gameObject.name == "bob")
        {
            Debug.Log("Reviving");
            HealthController healthController = hit.transform.GetComponent<HealthController>();
            healthController.RevivePlayer();
        }
    }

    // Shitty code, I hate this
    bool CheckIfPointingAtPlayer()
    {
        RaycastHit hit;
        Vector3 rayOrigin = cameraTransform.position + (cameraTransform.forward);
        Physics.Raycast(rayOrigin, cameraTransform.forward, out hit, 5);
        Debug.DrawRay(rayOrigin, cameraTransform.forward * 5, Color.red, 100f);
        
        if ( hit.transform == null  ) { return false; }

        if (hit.transform.gameObject.name == "bob") { Debug.Log("FOUND BOB"); return true; }

        return false;
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
            if (targetThrowable != null) 
            {
                targetThrowable.Throw(cameraTransform, hand.transform);
                
                Damagable targetDamagable = heldItem.GetComponent<Damagable>();
                if (targetDamagable != null) { targetDamagable.EnableCollision(); }
            }
            
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

        if (heldItem == null) { return; }
        
        Obtainable targetObtainable = heldItem.GetComponent<Obtainable>();
        targetObtainable.Drop(gameObject);
        heldItem = null;
    }

    // [ServerRpc(RequireOwnership = true)]
    // void DropServerRpc()
    // {
    //     Obtainable targetObtainable = heldItem.GetComponent<Obtainable>();
    //     targetObtainable.Drop(gameObject);
    //     heldItem = null;
    // }

    
    void DefaultAction()
    {
        bool flashSuccessful = cameraFlash.Flash();

        // if (flashSuccessful) { Debug.Log("Player Interaction: Flash Hit"); }
        // else { Debug.Log("Player Interaction: No target in range"); }
    }
}
