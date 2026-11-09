using UnityEngine;

public class Paint : MonoBehaviour , IInteractable , IInteractablePrompt
{
    [Header("Paint Configuration")]
    
    [SerializeField] private PaintData paintData ;
    [SerializeField] private Transform promptAnchor;
    public PaintData PaintData => paintData;

    public string PromptMessage => "E - Examine paint";

    public Transform PromptAnchor => promptAnchor;

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
    public void Interact()
    {      
        Debug.Log("The Player has interacted with me");

        if(GameManager.Instance != null)
        {
            UIManager.Instance.PaintingUI.ShowPaintInfo(PaintData);
        }
    }
}
