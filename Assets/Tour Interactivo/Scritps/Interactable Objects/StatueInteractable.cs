using UnityEngine;

public sealed class StatueInteractable : MonoBehaviour, IInteractable, IInteractablePrompt
{
    [SerializeField] private StatueData data;
    public StatueData Data => data;
    public string PromptMessage => "E - Explorar escultura";
    public Transform PromptAnchor => transform;

    public void Interact()
    {
        MuseumSceneEnhancements.RegisterViewed(this);
        MuseumRuntimeUI.ShowStatue(data);
    }
}
