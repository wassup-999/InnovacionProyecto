using UnityEngine;

[CreateAssetMenu(fileName = "PaintData", menuName = "Tour Interactivo/PaintData")]
public class PaintData : ScriptableObject
{
    [SerializeField] private string paintTitle;
    [SerializeField] private string author;

    [TextArea(3, 6)]
    [SerializeField] private string description;
    [SerializeField] private Sprite authorImage;
    [SerializeField] private Texture2D artworkImage;

    public string PaintTitle => paintTitle;
    public string Author => author;
    public string Description => description;
    public Sprite AuthorImage => authorImage;
    public Texture2D ArtworkImage => artworkImage;
}
