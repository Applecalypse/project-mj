using System;
using System.Collections;
using System.Collections.Generic;
using HGS.CallLimiter;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EnemyController : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Enemy Controls")]
    [SerializeField] private bool enablePlayerControls = true;

    [Header("Enemy Movement")]
    // private readonly float enemyBaseSpeed = 6f;
    [SerializeField] private float enemySpeed = 6f;
    private Vector3 enemyVelocity;

    [Header("Enemy Physics")]
    private readonly float jumpHeight = 1.0f;
    private readonly float gravityValue = -9.81f;
    
    [Header("Player Input")]
    private PlayerInput playerInput;
    private CharacterController controller;
    private InputAction moveAction;
    private InputAction jumpAction;

    [Header("Enemy Model")]
    private Transform feet;
    private bool isGrounded;
    public bool isFrozen;
    private Animator animator;

    [Header("Enemy Stun settings")]
    private bool isStunned = false;
    private readonly float stunDuration = 3f;
    [SerializeField] private Image stunScreen;

    [Header("Audio")]
    private AudioSource audioSource;
    
    [Header("Network")]
    private bool isInLobby;
    public NetworkVariable<FixedString32Bytes> nickname = new NetworkVariable<FixedString32Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Team> team = new NetworkVariable<Team>(Team.Human, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] private TMP_Text nameTag;

    [SerializeField] float fireRatio = 0.1f;
    Throttle _fireThrottle = new Throttle();
    
    private void Start()
    {
        StartCoroutine(WaitFrozen());
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponentInParent<Animator>();
        audioSource = GetComponentInChildren<AudioSource>();
        moveAction = playerInput.actions["Move"];

        jumpAction = playerInput.actions["Jump"];

        feet = transform.Find("Feet");
    }

    IEnumerator WaitFrozen()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        controller.enabled = true;
        isFrozen = false;
    }

    void Update()
    {
        if (isInLobby || isFrozen || isStunned) { return; }
    
        if (!enablePlayerControls) { return; }

        // Uncomment for real multiplayer stuff
        // if (!IsOwner) { return; }

        CheckGround();
        MakeMovement();
        Jump();
        ApplyGravity();
    }

    public override void OnNetworkSpawn()
    {
        nickname.OnValueChanged += OnNameChange;
        team.OnValueChanged += OnTeamChange;
        
        if (!IsOwner)
        {
            nameTag.text = nickname.Value.ToString();
            enabled = false; 
            return;
        }
        
        ChangePlayerNickname(OwnerClientId.ToString());
        base.OnNetworkSpawn();
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(feet.position, Vector3.down, 0.15f);
        if (isGrounded && enemyVelocity.y < 0)
        {
            enemyVelocity.y = 0f;
        }
    }

    void MakeMovement()
    {
        // Move
        Vector2 input2DVec = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input2DVec.x, 0, input2DVec.y);
        bool isMoving = move.magnitude > 0.1f;

        // set animator
        animator.SetBool("isMoving", isMoving);
        
        // will move relative to the camera's orientation
        Vector3 xMovement = move.x * cameraTransform.right.normalized;
        Vector3 zMovement = move.z * cameraTransform.forward.normalized;
        xMovement.y = 0;
        zMovement.y = 0;
        move = xMovement + zMovement;
        controller.Move(move * (Time.deltaTime * enemySpeed));
        
        if (move.magnitude > Mathf.Epsilon && isGrounded)
        {
            _fireThrottle.Run(()=>
            {
                SettingManager.Instance.PlaySfxGrass("MonsterFootstep", audioSource, 0.2f);
            }, 1/ (controller.velocity.magnitude * fireRatio) );
        }
    }

    void Jump()
    {
        if (jumpAction.triggered && isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            enemyVelocity.y += jumpVelocity;
        }
    }

    public void Stun()
    {
        Debug.Log("Stunned");
        // animation does not work properly
        animator.SetBool("isStunned", true);
        
        isStunned = true;
        StartCoroutine(GetStunned());
        StartCoroutine(FlashBang());
        
        EnemyInteraction enemyInteraction = GetComponent<EnemyInteraction>();
        enemyInteraction.Stun(stunDuration);
    }

    IEnumerator FlashBang()
    {
        stunScreen.color = new Color(1, 1, 1, 1);
        Debug.Log("alpha" + stunScreen.color.a);
        float fadeSpeed = 0.2f * Time.deltaTime;
        while (stunScreen.color.a > 0)
        {
            stunScreen.color = new Color(1, 1, 1, stunScreen.color.a - fadeSpeed);
            yield return null;
        }
    }

    IEnumerator GetStunned()
    {
        yield return new WaitForSecondsRealtime(stunDuration);
        animator.SetBool("isStunned", false);
        isStunned = false;
    }

    void ApplyGravity()
    {
        enemyVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(enemyVelocity * Time.deltaTime);
    }

    public void ChangePlayerNickname(string playerName)
    {
        nickname.Value = playerName;
        nameTag.text = playerName;
    }

    private void OnNameChange(FixedString32Bytes oldname, FixedString32Bytes newName)
    {
        Debug.Log("Name has been changed");
        nickname.Value = newName;
        nameTag.text = newName.ToString();
    }
    
    public void ChangePlayerTeam()
    {
        team.Value = team.Value == Team.Human ? Team.Monster : Team.Human;
    }

    private void OnTeamChange(Team oldTeam, Team newTeam)
    {
        Debug.Log("Team has been changed");
        team.Value = newTeam;
    }
    
    public bool IsInLobby
    {
        get => isInLobby;
        set {
            isInLobby = value;
            animator = GetComponentInParent<Animator>();
            animator.SetBool("isSitting", value);
            nameTag.enabled = value;
        }
    }
}
