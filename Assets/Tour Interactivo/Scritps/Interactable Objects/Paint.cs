using UnityEngine;

public class Paint : MonoBehaviour , IInteractable
{
    [Header("Paint Configuration")]
    [SerializeField] private string paintDescription = "Obra : 'Atardecer en el Valle'. Autor: Anónimo, Siglo XIX.'";

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
    public void Interact()
    {
        //logic of show a message on screen
        Debug.Log("The Player has interacted with me");

        if(GameManager.Instance != null)
        {
            GameManager.Instance.ShowMessage(paintDescription);
        }
    }
}
