using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    public float interactionDistance = 3f;
    private InputSystem_Actions inputs;

    private void Awake()
    {
        inputs = new();
    }
    private void OnEnable()
    {
        inputs.Enable();
        inputs.Player.Interact.performed += TryInteract;     
    }

    

    private void OnDisable()
    {
        inputs.Disable();
    }
    void Start()
    {
        
    }
    

    
    void Update()
    {
        
    }
    private void TryInteract(InputAction.CallbackContext context)
    {
        if (GameManager.Instance == null || GameManager.Instance.PlayerController == null) return;
        Ray ray = new Ray(GameManager.Instance.PlayerController.PlayerCamera.transform.position,
            GameManager.Instance.PlayerController.PlayerCamera.transform.forward);

        if (Physics.Raycast(ray , out RaycastHit hit , interactionDistance))
        {          
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if(hit.collider != null)
            {
                Debug.Log("Press E to interact");
                interactable.Interact();
            }
        }
    }

    
}
