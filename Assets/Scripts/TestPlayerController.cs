using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayerController : NetworkBehaviour
{
    private float playerBaseSpeed = 5f;
    private float playerFastSpeed = 10f;
    private float playerSlowSpeed = 3f;
    [SerializeField] private float playerSpeed = 5f;

    private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;
    private float rotationSpeed = 5f;    

    private float maxStamina = 50f;
    private float stamina = 50f;
    private bool isTired = false;
    private float staminaRecoveryRate = 5f;
    private float staminaDecreaseRate = 12.5f;
    
    private CharacterController controller;
    private PlayerInput playerInput;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    private InputAction moveAction;
    private InputAction runAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    // private InputAction aimAction;
    private InputAction shootAction;
    private InputAction interactAction;
    // private InputAction throwAction;

    [SerializeField]
    private Transform cameraTransform;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false; 
            return;
        }
        base.OnNetworkSpawn();
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        runAction = playerInput.actions["Run"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        // aimAction = playerInput.actions["Aim"];
        shootAction = playerInput.actions["Shoot"];
        interactAction = playerInput.actions["Interact"];

        // cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        
        MakeMovement();
        CameraRotation();
        Jump();

        // Apply gravity
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
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
        
        // will move relative to the camera's orientation
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized; 
        controller.Move(move * Time.deltaTime * playerSpeed);
    }

    void CameraRotation()
    {
        // Vector2 lookVec = lookAction.ReadValue<Vector2>();
        // Debug.Log(lookVec);
        // // Makes player face the direction of the camera
        float targetAngle = cameraTransform.eulerAngles.y; // this is the angle of the camera around y-axis
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle,0); // Form quaternion representation of the target angle
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void Jump()
    {
        if (jumpAction.triggered && groundedPlayer)
        {
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            if (isTired)
            {
                jumpVelocity /= 2;
            }
            playerVelocity.y += jumpVelocity;
        }
    }

    void Shoot()
    {
        if (shootAction.triggered)
        {
            Debug.Log("Shoot");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // can display icon / message to indicate that player can interact with object
        if (other.gameObject.CompareTag("Interactable"))
        {
            Interact();
        }
    }

    void Interact()
    {
        if (interactAction.triggered)
        {
            Debug.Log("Interact");
        }
    }
}