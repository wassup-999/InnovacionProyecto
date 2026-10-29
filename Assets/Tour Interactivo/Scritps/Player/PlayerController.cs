using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float backwardSpeedMultiplier = 0.75f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float deceleration = 14f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedStickForce = -2f;

    #region References
    public CinemachineCamera PlayerCamera;
    private CharacterController characterController;
    private InputSystem_Actions inputs;
    #endregion

    private Vector2 moveInput;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private bool canMove = true;

    public bool CanMove => canMove;
    public bool IsMoving => horizontalVelocity.sqrMagnitude > 0.01f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();      
        inputs = new();
    }
    private void OnEnable()
    {
        inputs.Player.Enable();
    }
    private void OnDisable()
    {
        inputs.Player.Disable();
    }
    private void OnDestroy()
    {
        inputs.Dispose();
    }
    private void Update()
    {
        ReadInput();
        HandleMovement();
    }
    private void ReadInput()
    {
        moveInput = canMove ? inputs.Player.Move.ReadValue<Vector2>() : Vector2.zero;
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }
    private void HandleMovement()
    {
        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        moveDir.y = 0;

        Vector3 targetVelocity = Vector3.zero;
        if(moveDir.sqrMagnitude > 0.0001f)
        {
            targetVelocity = moveDir * (walkSpeed * GetBackwardFactor(moveInput.y));
        }

        float rate = targetVelocity.sqrMagnitude > 0.0001f ? acceleration : deceleration;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, rate * Time.deltaTime);

        if(characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedStickForce;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        characterController.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
    }
    private float GetBackwardFactor(float inputY)
    {
        if (inputY >= 0f) return 1f;
        return Mathf.Lerp(1f, backwardSpeedMultiplier, -inputY);
    }
    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!value) horizontalVelocity = Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(PlayerCamera.transform.position, PlayerCamera.transform.position + PlayerCamera.transform.forward * 3f);
    }
}
