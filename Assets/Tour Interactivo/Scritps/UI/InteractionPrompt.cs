using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI actionTxt;
    [SerializeField] private Camera playerCamera;
    private Transform target;

   
    

    private void Awake()
    {
        playerCamera = Camera.main;
        Hide();
    }
    void Start()
    {
        
    }

    
    
    private void LateUpdate()
    {
        if (target == null || playerCamera == null) return;

        transform.position = target.position;
        
        transform.rotation = Quaternion.LookRotation(playerCamera.transform.position - transform.position);
        
        Debug.Log("Objetive: " + target.position);//<-
        Debug.Log("Canvas: " + transform.position);//<-
    }

    public void Show(Transform newTarget , string message)
    {
        target = newTarget;
        actionTxt.text = message;
        gameObject.SetActive(true);


        Debug.Log("Active Object: " + gameObject.activeSelf);//<-
        Debug.Log("Hierarchy Active: " + gameObject.activeInHierarchy);//<-      
    }

    public void Hide()
    {
        target = null;
        gameObject.SetActive(false);
    }
}
