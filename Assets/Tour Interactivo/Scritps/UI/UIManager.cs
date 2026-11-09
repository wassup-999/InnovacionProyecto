using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("References")]
    public PaintingUI PaintingUI;
    public InteractionPrompt InteractionPrompt;

   
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        
    }
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    public void ShowInteractionPrompt(Transform target , string message)
    {
        Debug.Log("application recieved from UIManager for show message");//<-
        
        InteractionPrompt.Show(target , message);
    }
    public void HideInteractionPrompt()
    {
        InteractionPrompt.Hide();
    }
}
