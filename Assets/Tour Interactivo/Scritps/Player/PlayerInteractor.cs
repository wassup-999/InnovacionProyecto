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
        inputs.Player.Interact.performed -= TryInteract;
        inputs.Disable();
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
        Ray ray = new Ray(GameManager.Instance.PlayerController.PlayerCamera.transform.position,
            GameManager.Instance.PlayerController.PlayerCamera.transform.forward);

        Paint detectedPaint = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            detectedPaint = hit.collider.GetComponentInParent<Paint>();

            
            if (detectedPaint != null)
            {
                Debug.Log("Paint detected" + detectedPaint.name);//<-
            }
        }

        if (detectedPaint == GameManager.Instance.Paint) return;

        GameManager.Instance.Paint = detectedPaint;

        if (GameManager.Instance.Paint != null)
        {
            UIManager.Instance.ShowInteractionPrompt(GameManager.Instance.Paint.transform, "E - Examine paint");
            Debug.Log("Show message");//<-
        }
        else
        {
            UIManager.Instance.HideInteractionPrompt();
        }
    }
    private void TryInteract(InputAction.CallbackContext context)
    {
        if(GameManager.Instance.Paint != null)
        {
            GameManager.Instance.Paint.Interact();
        }
    }

    
}
