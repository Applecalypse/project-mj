using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using HGS.CallLimiter;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private GameObject cameraObject;
    private Transform cameraTransform;

    [Header("Player Controls")]
    [SerializeField] private bool enablePlayerControls = true;

    [Header("Player Movement")]
    private readonly float playerBaseSpeed = 5f;
    private readonly float playerFastSpeed = 10f;
    private readonly float playerSpectatorSpeed = 15f;
    private readonly float playerSlowSpeed = 3f;
    [SerializeField] private float playerSpeed = 5f;
    private Vector3 playerVelocity;
    private Vector3 spectatorVelocity;

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
    private GameObject player;
    public TMP_Text nameTag;
    private Animator animator;
    public bool isDead;
    public bool isFrozen;
    private bool isSpectator;
    private SkinnedMeshRenderer playerModel;
    private Canvas username;
    [SerializeField] private Image crosshair;
    

    [Header("Networking - Debug")]
    // [SerializeField] private bool isInLobby;
    public NetworkVariable<FixedString32Bytes> nickname = new NetworkVariable<FixedString32Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Team> team = new NetworkVariable<Team>(Team.Human, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isInLobby = new NetworkVariable<bool>();
    public SpawnPosition sittingPos = new SpawnPosition();
    
    [Header("Sound system")]
    private AudioSource audioSource;
    [SerializeField] float fireRatio = 0.3f;
    Throttle _fireThrottle = new Throttle();
    
    private void Awake()
    {
        StartCoroutine(WaitFrozen());
        controller = GetComponent<CharacterController>();
        controller.enabled = false;
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponentInParent<Animator>();
        audioSource = GetComponentInChildren<AudioSource>();
        moveAction = playerInput.actions["Move"];
        runAction = playerInput.actions["Run"];
        jumpAction = playerInput.actions["Jump"];
        feet = transform.Find("Feet");
        playerModel = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        username = gameObject.GetComponentInChildren<Canvas>();
        
        cameraTransform = cameraObject.transform;
        SetUpCamera();
        if (isDead)
        {
            OnDead();
        }
    }

    void SetUpCamera()
    {
        CinemachineVirtualCamera cmc = cameraObject.GetComponent<CinemachineVirtualCamera>();
        CinemachinePOV vcam = cmc.GetCinemachineComponent<CinemachinePOV>();
        vcam.m_HorizontalAxis.m_MaxSpeed = SettingManager.Instance.GetSensitivityX();
        vcam.m_VerticalAxis.m_MaxSpeed = SettingManager.Instance.GetSensitivityY();
    }
    
    // TODO: find a fix to this hacky solution
    IEnumerator WaitFrozen()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        controller.enabled = true;
        isFrozen = false;
        crosshair.enabled = true;
    }

    void Update()
    {
        if (isInLobby.Value) { crosshair.enabled = false; }
        if (isInLobby.Value || isFrozen) { return; }
        
        if (!enablePlayerControls) { return; }
        
        // Uncomment for real multiplayer stuff
        // if (!IsOwner) { return; }

        CheckGround();
        MakeMovement();
        Jump();
        CheckSpectator();
        ApplyGravity();
        //Debug.Log(isSpectator);
    }

    public override void OnNetworkSpawn()
    {
        nickname.OnValueChanged += OnNameChange;
        team.OnValueChanged += OnTeamChange;
        isInLobby.OnValueChanged += OnLobbyStateChange;
        if (!(isInLobby.Value)) { nameTag.enabled = false; }
        
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
        GameManager.Instance.EmptyPosition(sittingPos.position, sittingPos.rotation);
        base.OnNetworkDespawn();
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(feet.position, Vector3.down, 0.15f);
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
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

        if (isSpectator)
        {
            playerSpeed = playerSpectatorSpeed;
        }
        
        // will move relative to the camera's orientation
        Vector3 xMovement = move.x * cameraTransform.right.normalized;
        Vector3 zMovement = move.z * cameraTransform.forward.normalized;
        xMovement.y = 0;
        zMovement.y = 0;
        move = xMovement + zMovement;

        // set animator & move
        if (isDead)
        {
            controller.Move(move * (Time.deltaTime * (playerSpeed * 0.5f)));
            animator.SetFloat("movement", controller.velocity.magnitude);
        }
        else
        {
            controller.Move(move * (Time.deltaTime * playerSpeed));
            animator.SetBool("isMoving", isMoving);
        }

        if (move.magnitude > Mathf.Epsilon && isGrounded)
        {
            Debug.Log("Playing sound");
            _fireThrottle.Run(()=>
            {
                SettingManager.Instance.PlaySfxGrass("OnGrass", audioSource, 0.3f);
            }, 1/ (controller.velocity.magnitude * fireRatio) );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isKeyItem = other.CompareTag("KeyItem");
        if (isKeyItem)
        {
            SettingManager.Instance.PlaySfx("KeyItemPickUp", audioSource);
        }
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

    void CheckSpectator()
    {
        if (isSpectator)
        {
            username.enabled = false;
            playerModel.enabled = false;
            controller.Move(spectatorVelocity * Time.deltaTime);
        }
        else
        {
            username.enabled = true;
            playerModel.enabled = true;
        }
    }

    public void setSpectator(bool state)
    {
        isSpectator = state;
    }
    
    public void onFly(InputAction.CallbackContext context)
    {
        if (!isSpectator)
        {
            return;
        }

        float verticalDirection = context.ReadValue<Vector2>().y;
        spectatorVelocity.x = playerVelocity.x;
        spectatorVelocity.y =  verticalDirection * playerSpectatorSpeed;
        spectatorVelocity.z = playerVelocity.z;
    }

    void ApplyGravity()
    {
        if (isSpectator)
        {
            return;
        }
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void Revive()
    {
        isDead = false;
        animator.Play("idling");
    }

    public void OnDead()
    {
        isDead = true;
        animator.Play("player_crawling");
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

        ChangeTeamServerRpc(OwnerClientId, team.Value);
        
        foreach (var VARIABLE in GameManager.Instance.uidToTeam)
        {
            Debug.LogFormat("{0} : {1}", VARIABLE.Key, VARIABLE.Value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangeTeamServerRpc(ulong Id, Team newTeam)
    {
        GameManager.Instance.uidToTeam[Id] = newTeam;
        foreach (var VARIABLE in GameManager.Instance.uidToTeam)
        {
            Debug.LogFormat("{0} : {1}", VARIABLE.Key, VARIABLE.Value);
        }
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