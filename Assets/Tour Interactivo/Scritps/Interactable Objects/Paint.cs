using UnityEngine;

public class Paint : MonoBehaviour, IInteractable, IInteractablePrompt
{
    [Header("Paint Configuration")]
    [SerializeField] private PaintData paintData;
    [SerializeField] private Transform promptAnchor;
    public PaintData PaintData => paintData;
    public string PromptMessage => "E - Examinar obra";
    public Transform PromptAnchor => promptAnchor;

    public void Interact()
    {
        MuseumSceneEnhancements.RegisterViewed(this);
        if (paintData != null && MuseumRuntimeUI.Instance != null)
            MuseumRuntimeUI.ShowPainting(paintData);
        else if (paintData != null && UIManager.Instance != null && UIManager.Instance.PaintingUI != null)
            UIManager.Instance.PaintingUI.ShowPaintInfo(paintData);
        else
            Debug.LogWarning($"No se pudo abrir la ficha de la obra '{name}': faltan referencias de UI o PaintData.", this);
    }
}
