using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/CardSO")]
public class CardSO : ScriptableObject
{
    public string Id;
    public string Title;
    [TextArea] public string Description;
    public Sprite Artwork;
    public Sprite Background; 

    [Header("Card Impacts")]
    public int heartImpact;
    public int careerImpact;
    public int happinessImpact;

    [Header("Left Answer")]
    public string leftAnswer;

    [Header("Right Answer")]
    public string rightAnswer;

    [Header("Impact Type")]
    public List<ImpactType> impactType;
}

public enum ImpactType
{
    Heart,
    Career,
    Happiness
}