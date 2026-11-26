using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour 
{
    public float interactionDistance = 3f;
    private InputSystem_Actions inputs;
    private IInteractable currentInteractable;
    private Paint currentPaint;

    
    private void Awake()
    {
        inputs = new();
    }
    private void OnEnable()
    {
        if (inputs == null) inputs = new InputSystem_Actions();
        inputs.Enable();
        inputs.Player.Interact.performed += TryInteract;     
    }

    

    private void OnDisable()
    {
        if (inputs != null)
        {
            inputs.Player.Interact.performed -= TryInteract;
            inputs.Disable();
        }
        currentInteractable = null;
        currentPaint = null;
        MuseumRuntimeUI.SetFocus(null);
    }
    void Start()
    {
        
    }
    

    
    void Update()
    {
        DetecInteractable();
    }
    private void DetecInteractable()
    {
        if (GameManager.Instance == null || GameManager.Instance.PlayerController == null) return;
        var playerCamera = GameManager.Instance.PlayerController.PlayerCamera;
        if (playerCamera == null) return;
        IInteractable detected = null;
        Paint detectedPaint = null;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, interactionDistance))
        {
            detectedPaint = hit.collider.GetComponentInParent<Paint>();
            if (detectedPaint != null) detected = detectedPaint;
            else detected = hit.collider.GetComponentInParent<StatueInteractable>();
        }
        if (ReferenceEquals(detected, currentInteractable)) return;
        currentInteractable = detected;
        currentPaint = detectedPaint;
        GameManager.Instance.Paint = currentPaint;
        var prompt = detected as IInteractablePrompt;
        MuseumRuntimeUI.SetFocus(prompt != null ? prompt.PromptMessage : null);
        if (UIManager.Instance != null) UIManager.Instance.HideInteractionPrompt();
    }
    private void TryInteract(InputAction.CallbackContext context)
    {
        currentInteractable?.Interact();
    }

    
}

