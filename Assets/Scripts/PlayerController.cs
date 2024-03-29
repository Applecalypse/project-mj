using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [Header("Testing")]
    [SerializeField] private bool enablePlayerControls = true;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Player Movement")]
    private readonly float playerBaseSpeed = 5f;
    private readonly float playerFastSpeed = 10f;
    private readonly float playerSlowSpeed = 3f;
    [SerializeField] private float playerSpeed = 5f;
    private Vector3 playerVelocity;

    [Header("Player Physics")]
    private readonly float jumpHeight = 1.0f;
    private readonly float gravityValue = -9.81f;

    [Header("Player Stamina")]
    private readonly float maxStamina = 50f;
    private float stamina = 50f;
    private bool isTired = false;
    private readonly float staminaRecoveryRate = 5f;
    private readonly float staminaDecreaseRate = 12.5f;
    
    [Header("Player Input")]
    private PlayerInput playerInput;
    private CharacterController controller;
    private InputAction moveAction;
    private InputAction runAction;
    private InputAction jumpAction;

    [Header("Player Model")]
    private Transform feet;
    private bool isGrounded;
    [SerializeField] private TMP_Text nameTag;
    private Animator animator;
    public bool isDead;
    

    [Header("Networking - Debug")]
    // [SerializeField] private bool isInLobby;
    public NetworkVariable<FixedString32Bytes> nickname = new NetworkVariable<FixedString32Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Team> team = new NetworkVariable<Team>(Team.Human, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isInLobby = new NetworkVariable<bool>();
    public SpawnPosition sittingPos = new SpawnPosition();
    
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponentInParent<Animator>();
        moveAction = playerInput.actions["Move"];
        runAction = playerInput.actions["Run"];
        jumpAction = playerInput.actions["Jump"];
        feet = transform.Find("Feet");
    }

    void Update()
    {
        if (isInLobby.Value) { return; }
        
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
        isInLobby.OnValueChanged += OnLobbyStateChange;
        
        if (!IsOwner)
        {
            nameTag.text = nickname.Value.ToString();
            enabled = false; 
            return;
        }
        
        ChangePlayerNickname(OwnerClientId.ToString());
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        gameManager.EmptyPosition(sittingPos.position, sittingPos.rotation);
        base.OnNetworkDespawn();
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(feet.position, Vector3.down, 0.15f);
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            // Debug.Log("Grounded");
        }
        else
        {
            // Debug.Log("Not Grounded");
        }
    }

    void MakeMovement()
    {
        // Run
        bool runButtonPressed = runAction.ReadValue<float>() > 0.5f;

        // Move
        Vector2 input2DVec = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input2DVec.x, 0, input2DVec.y);
        bool isMoving = move.magnitude > 0.1f;

        if (!isTired && runButtonPressed && isMoving)
        {
            playerSpeed = playerFastSpeed;
            stamina -= Time.deltaTime * staminaDecreaseRate;
            if (stamina <= 0)
            {
                isTired = true;
                playerSpeed = playerSlowSpeed;
            }
        }
        else
        {
            if (isTired) { playerSpeed = playerSlowSpeed; }
            else { playerSpeed = playerBaseSpeed; }

            if (stamina < maxStamina)
            {
                stamina += Time.deltaTime * staminaRecoveryRate;
            }
            if ( stamina > (maxStamina * 0.33f) )
            {
                isTired = false;
            }
        }
        
        // set animator
        animator.SetBool("isMoving", isMoving);
        
        
        // will move relative to the camera's orientation
        Vector3 xMovement = move.x * cameraTransform.right.normalized;
        Vector3 zMovement = move.z * cameraTransform.forward.normalized;
        xMovement.y = 0;
        zMovement.y = 0;
        move = xMovement + zMovement;
        controller.Move(move * (Time.deltaTime * playerSpeed));
    }

    void Jump()
    {
        if (jumpAction.triggered && isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            if (isTired)
            {
                jumpVelocity /= 2;
            }
            playerVelocity.y += jumpVelocity;
        }
    }

    void ApplyGravity()
    {
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
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

    private void OnLobbyStateChange(bool oldValue, bool newValue)
    {
        Debug.Log("Lobby State has been changed");
        IsInLobby = newValue;
    }
    
    public bool IsInLobby
    {
        // get => isInLobby;
        set {
            animator = GetComponentInParent<Animator>();
            animator.SetBool("isSitting", value);
            nameTag.enabled = value;
            isInLobby.Value = value;
        }
    }
}

public enum Team 
{
    Human,
    Monster,
}