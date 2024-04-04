using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class EnemyInteraction : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    private readonly float interactionDistance = 3f;

    [Header("Enemy Controls")]
    [SerializeField] private bool enablePlayerControls = true;

    [Header("Enemy Status")]
    // private bool isStunned = false;
    public NetworkVariable<bool> isStunned = new NetworkVariable<bool>();

    [Header("Enemy Animation")]
    private Animator animator;

    [Header("Player Input")]
    private PlayerInput playerInput;
    private InputAction interactAction, attackAction;

    [Header("Enemy Attack")]
    [SerializeField] private GameObject weapon;
    private bool canAttack = true;
    private readonly float attackCooldown = 3f;

    [Header("Damage")]
    private Damagable damagable; // should be at weapon

    [Header("Audio")]
    private AudioSource audioSource;

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponentInParent<Animator>();
        damagable = weapon.GetComponent<Damagable>();

        audioSource = GetComponentInChildren<AudioSource>();

        interactAction = playerInput.actions["Interact"];
        attackAction = playerInput.actions["Attack"];
        canAttack = true;
    }

    void Update()
    {
        if (!enablePlayerControls) { return; }

        if (isStunned.Value) { return; }

        Interact();
        Attack();
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
        if (!enablePlayerControls) { return; }
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
        
        damagable.EnableCollision();
        animator.SetTrigger("Attacks");
        SettingManager.Instance.PlaySfx("MonsterAttack", audioSource);
        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        damagable.DisableCollision();
    }

    public void Stun( float stunDuration )
    {
        SetStunServerRpc(true);
        SettingManager.Instance.PlaySfx("MonsterAttack", audioSource);
        StartCoroutine(GetStunned(stunDuration));
        SettingManager.Instance.PlaySfx("MonsterConfusion", audioSource);
    }

    IEnumerator GetStunned(float stunDuration)
    {
        yield return new WaitForSecondsRealtime(stunDuration);
        SetStunServerRpc(false);
    }

    [ServerRpc]
    private void SetStunServerRpc(bool stun)
    {
        isStunned.Value = stun;
    }
}
