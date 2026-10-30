using UnityEngine;

[CreateAssetMenu(fileName = "PaintData", menuName = "Tour Interactivo/PaintData")]
public class PaintData : ScriptableObject
{
    [SerializeField] private string paintTitle;
    [SerializeField] private string author;

    [TextArea(3, 6)]
    [SerializeField] private string description;
    [SerializeField] Sprite authorImage;  

    public string PaintTitle => paintTitle;
    public string Author => author;
    public string Description => description;
    public Sprite AuthorImage => authorImage;
}
