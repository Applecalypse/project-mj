using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class SwitchCam : MonoBehaviour
{
    [SerializeField]
    private PlayerInput playerInput; // to detect when user press a button to aim

    [SerializeField] int priorityIncrement = 10;
    private CinemachineVirtualCamera virtualCamera;
    private InputAction aimAction;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        aimAction = playerInput.actions["Aim"];
    }

    private void OnEnable() 
    {
        aimAction.performed += _ => StartAim(); // have it run your code when the action is triggered
        aimAction.canceled += _ => CancelAim(); // have it run your code when the action is triggered
    }

    private void OnDisable() 
    {
        aimAction.performed -= _ => StartAim(); // have it run your code when the action is triggered
        aimAction.canceled -= _ => CancelAim(); // have it run your code when the action is triggered
    }

    private void StartAim()
    {
        virtualCamera.Priority += priorityIncrement;
    }

    private void CancelAim()
    {
        virtualCamera.Priority -= priorityIncrement;
    }

}
