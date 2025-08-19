using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/CardSO")]
public class CardSO : ScriptableObject
{
    public string Id;
    public string Title;
    [TextArea] public string Description;
    public Sprite Artwork;

    [Header("Answers (UI Text)")]
    public string leftAnswer;
    public string rightAnswer;

    [Header("LEFT Swipe Impacts")]
    public int leftHeartImpact;
    public int leftCareerImpact;
    public int leftHappinessImpact;

    [Header("RIGHT Swipe Impacts")]
    public int rightHeartImpact;
    public int rightCareerImpact;
    public int rightHappinessImpact;

    [Header("Impact Type (optional tags)")]
    public List<ImpactType> impactType;
}

public enum ImpactType
{
    Heart,
    Career,
    Happiness
}
