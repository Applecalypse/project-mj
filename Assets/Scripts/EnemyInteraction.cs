using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyInteraction : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    private readonly float interactionDistance = 3f;

    [Header("Enemy Animation")]
    private Animator animator;

    [Header("Player Input")]
    private PlayerInput playerInput;
    private InputAction interactAction, attackAction;

    [Header("Enemy Attack")]
    [SerializeField] private GameObject weapon;
    private bool canAttack = false;
    private readonly float attackCooldown = 5f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        interactAction = playerInput.actions["Interact"];
        attackAction = playerInput.actions["Attack"];
        canAttack = true;
    }

    void Update()
    {
        Interact();
        Attack();
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
        
    }

    void Attack()
    {
        if (!attackAction.triggered) { return; }
        if (!canAttack) { return; }

        canAttack = false;
        weapon.SetActive(true);
        // uncomment when we have an attacking animation
        // animator.SetTrigger("Attack");
        
        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        weapon.SetActive(false);
    }
}
