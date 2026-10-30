using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public PaintingUI PaintingUI;
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
}
