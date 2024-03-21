using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyController : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Enemy Movement")]
    // private readonly float enemyBaseSpeed = 6f;
    [SerializeField] private float enemySpeed = 6f;
    private Vector3 enemyVelocity;

    [Header("Enemy Physics")]
    private readonly float jumpHeight = 1.0f;
    private readonly float gravityValue = -9.81f;
    private readonly float rotationSpeed = 5f;    

    // [Header("Player Stamina")]
    // private readonly float maxStamina = 50f;
    // private float stamina = 50f;
    // private bool isTired = false;
    // private readonly float staminaRecoveryRate = 5f;
    // private readonly float staminaDecreaseRate = 12.5f;
    
    [Header("Player Input")]
    private PlayerInput playerInput;
    private CharacterController controller;
    private InputAction moveAction;
    // private InputAction runAction;
    private InputAction jumpAction;

    [Header("Enemy Model")]
    private Transform feet;
    private bool isGrounded;

    private Animator animator;
    

    [Header("Network")]
    private bool isInLobby;
    public NetworkVariable<FixedString32Bytes> nickname = new NetworkVariable<FixedString32Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Team> team = new NetworkVariable<Team>(Team.Human, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] private TMP_Text nameTag;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponentInParent<Animator>();
        moveAction = playerInput.actions["Move"];
        // runAction = playerInput.actions["Run"];
        jumpAction = playerInput.actions["Jump"];

        feet = transform.Find("Feet");
    }

    void Update()
    {
        if (isInLobby) { return; }
        // Uncomment for real multiplayer stuff
        // if (!IsOwner) { return; }

        CheckGround();
        MakeMovement();
        CameraRotation();
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
            Debug.Log("Grounded");
        }
        else
        {
            Debug.Log("Not Grounded");
        }
    }

    void MakeMovement()
    {
        // Move
        Vector2 input2DVec = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input2DVec.x, 0, input2DVec.y);
        // bool isMoving = move.magnitude > 0.1f;

        // set animator
        // animator.SetBool("isMoving", isMoving);
        
        // will move relative to the camera's orientation
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized; 
        controller.Move(move * (Time.deltaTime * enemySpeed));
    }

    void CameraRotation()
    {
        // // Makes player face the direction of the camera
        float targetAngle = cameraTransform.eulerAngles.y; // this is the angle of the camera around y-axis
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle,0); // Form quaternion representation of the target angle
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void Jump()
    {
        if (jumpAction.triggered && isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            enemyVelocity.y += jumpVelocity;
        }
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
