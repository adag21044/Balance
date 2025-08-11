using UnityEngine;

[CreateAssetMenu(menuName = "Card/CardSO")]
public class CardSO : ScriptableObject
{
    public string Id;
    public string Title;
    [TextArea] public string Description;
    public Sprite Artwork;
}