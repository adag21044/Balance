using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/CardSO")]
public class CardSO : ScriptableObject
{
    [Header("Visuals & Text")]
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
    public int leftSociabilityImpact;

    [Header("RIGHT Swipe Impacts")]
    public int rightHeartImpact;
    public int rightCareerImpact;
    public int rightHappinessImpact;
    public int rightSociabilityImpact;

    [Header("Impact Type (optional tags)")]
    public List<ImpactType> impactType;

    public float ageImpact; // Age impact

    [Header("Chaining (optional)")]
    public bool isChainCard; // If true, this card is part of a chain
    public CardSO nextOnLeft;
    public CardSO nextOnRight;
    public CardSO[] nextPoolLeft;
    public CardSO[] nextPoolRight;

    public LifeStage lifeStage;
    public bool isOnlyOnce; // If true, this card can be used only once per game
}

public enum ImpactType
{
    Heart,
    Career,
    Happiness,
    Sociability
}

public enum LifeStage
{
    Childhood,   // 0 – 12
    Teenager,    // 13 – 19
    YoungAdult,  // 20 – 29
    Adult,       // 30 – 59
    Senior,       // 60+
    Any
}
