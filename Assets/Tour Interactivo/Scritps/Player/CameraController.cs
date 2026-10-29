using UnityEngine;
using System;
using UnityEngine.InputSystem;
public class CameraController : MonoBehaviour
{
    [Header("Camera Properties")]
    public float mouseSensivity = 80.0f;
    public float gamepadSensitivity = 120.0f;
    public float xRotation = 0;

    [Header("Objects References")]
    public Transform PlayerReference;
    public InputSystem_Actions inputs;

    private void Awake()
    {
        if (inputs == null) inputs = new();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void OnEnable()
    {
        inputs.Player.Enable();
    }

    private void OnDisable()
    {
        inputs.Player.Disable();
    }
    void Update()
    {
        MouseMotion();
    }

    private void MouseMotion()
    {
        InputAction look = inputs.Player.Look;
        Vector2 lookInput = look.ReadValue<Vector2>();

        bool fromPointer = look.activeControl != null && look.activeControl.device is Pointer;
        float scale = fromPointer ? mouseSensivity : gamepadSensitivity * Time.deltaTime;

        float mouseX = lookInput.x * scale;
        float mouseY = lookInput.y * scale;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if(PlayerReference != null)
        {
            PlayerReference.Rotate(Vector3.up * mouseX);
        }
        
    }
}
