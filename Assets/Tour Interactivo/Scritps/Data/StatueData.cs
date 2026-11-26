using UnityEngine;

[CreateAssetMenu(fileName = "StatueData", menuName = "Tour Interactivo/Statue Data")]
public sealed class StatueData : ScriptableObject
{
    [SerializeField] private string statueName;
    [SerializeField] private string cultureAndOrigin;
    [SerializeField] private string period;
    [TextArea(4, 9)] [SerializeField] private string description;
    [SerializeField] private Texture2D image;
    public string StatueName => statueName;
    public string CultureAndOrigin => cultureAndOrigin;
    public string Period => period;
    public string Description => description;
    public Texture2D Image => image;
}
