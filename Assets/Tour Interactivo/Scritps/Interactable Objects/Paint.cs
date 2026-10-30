using UnityEngine;

public class Paint : MonoBehaviour , IInteractable
{
    [Header("Paint Configuration")]
    
    [SerializeField] private PaintData paintData ;

    public PaintData PaintData => paintData;

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
