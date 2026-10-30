using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaintingUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleTxt;
    [SerializeField] private TextMeshProUGUI authorTxt;
    [SerializeField] private TextMeshProUGUI descriptionTxt;
    [SerializeField] private UnityEngine.UI.Image authorImage;    
    void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    
    void Update()
    {
        
    }
    public void ShowPaintInfo(PaintData paintInfo)
    {
        if (panel != null && 
            titleTxt != null &&
            authorTxt != null &&
            descriptionTxt != null &&
            authorImage != null)
        {
            
            titleTxt.text = paintInfo.PaintTitle;
            authorTxt.text = paintInfo.Author;
            descriptionTxt.text = paintInfo.Description;
            authorImage.sprite = paintInfo.AuthorImage;
            panel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (GameManager.Instance.PlayerController != null) GameManager.Instance.PlayerController.SetCanMove(false);
            if (GameManager.Instance.CameraController != null) GameManager.Instance.CameraController.enabled = false;

        }       
    }

    public void HidePaintInfo()
    {
        if(panel != null)
        {
            panel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (GameManager.Instance.PlayerController != null) GameManager.Instance.PlayerController.SetCanMove(true);
            if (GameManager.Instance.CameraController != null) GameManager.Instance.CameraController.enabled = true;
        }
        
    }
}
