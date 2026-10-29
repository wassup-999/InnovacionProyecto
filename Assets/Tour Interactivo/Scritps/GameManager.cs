using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("Player References")]
    public PlayerController PlayerController;
    public CameraController CameraController;
    public PlayerInteractor PlayerInteractor;

    [Header("UI References")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
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
        if(messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }
    
    public void ShowMessage(string message)
    {
        if (messagePanel != null && messageText != null)
        {
            messageText.text = message;
            messagePanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (PlayerController != null) PlayerController.SetCanMove(false);
            if (CameraController != null) CameraController.enabled = false;

        }
    }
    public void HideMessage()
    {
        if(messagePanel != null)
        {
            messagePanel.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (PlayerController != null) PlayerController.SetCanMove(true);
            if (CameraController != null) CameraController.enabled = true;
        }
    }
}
