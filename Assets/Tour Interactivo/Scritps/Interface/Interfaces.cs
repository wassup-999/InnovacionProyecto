using UnityEngine;

public interface IInteractable
{
    public void Interact();
}

public interface IInteractablePrompt
{
    string PromptMessage { get; }
    Transform PromptAnchor { get; }
}


