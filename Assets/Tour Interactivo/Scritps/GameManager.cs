using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("Player References")]
    public PlayerController PlayerController;
    public CameraController CameraController;
    public PlayerInteractor PlayerInteractor;
    [Header("Paint References")]
    public Paint Paint;

    
    private void Awake()
    {
        if(Instance != null && Instance != this)
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
 
}
