using UnityEngine;

[CreateAssetMenu(menuName = "Card/CardSO")]
public class CardSO : ScriptableObject
{
    public string Id;
    public string Title;
    [TextArea] public string Description;
    public Sprite Artwork;

    [Header("Card Impacts")]
    public int heartImpact;
    public int careerImpact;
    public int happinessImpact;

    [Header("Left Answer")]
    public string leftAnswer;

    [Header("Right Answer")]
    public string rightAnswer;
}