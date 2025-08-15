using System;
using UnityEngine;

public class StatModel
{
    public static StatModel Instance { get; private set; } = new StatModel();

    private float heartPercantage;
    public float HeartPercantage => heartPercantage;
    
    private float careerPercantage;
    public float CareerPercantage => careerPercantage;

    private float happinessPercantage;
    public float HappinessPercantage => happinessPercantage;

    public event Action<float> OnHeartChanged;
    public event Action<float> OnCareerChanged;
    public event Action<float> OnHappinessChanged;

    public static event Action OnHeartAffected;
    public static event Action OnCareerAffected;
    public static event Action OnHappinessAffected;

    public const float IMPACT_SCALE = 0.01f;

    public StatModel()
    {
        heartPercantage = 0.5f;
        careerPercantage = 0.5f;
        happinessPercantage = 0.5f;
    }

    public static void PreviewImpacts(CardSO card)
    {
        if (card.heartImpact != 0) OnHeartAffected?.Invoke();
        if (card.careerImpact != 0) OnCareerAffected?.Invoke();
        if (card.happinessImpact != 0) OnHappinessAffected?.Invoke();
    }


    public void ApplyCard(CardSO card)
    {
        Debug.Log($"Applying card impacts: {card.name}");
        
        if (card == null) return;

        if (card.heartImpact != 0)
            ApplyAndRaise(ref heartPercantage, card.heartImpact * IMPACT_SCALE, OnHeartChanged);

        if (card.careerImpact != 0)
            ApplyAndRaise(ref careerPercantage, card.careerImpact * IMPACT_SCALE, OnCareerChanged);

        if (card.happinessImpact != 0)
            ApplyAndRaise(ref happinessPercantage, card.happinessImpact * IMPACT_SCALE, OnHappinessChanged);
    }
    

    private void ApplyAndRaise(ref float statValue, float delta, Action<float> evt)
    {
        float before = statValue;
        statValue = Mathf.Clamp01(statValue + delta);
        Debug.Log($"[StatModel] stat before={before:F2}, delta={delta:F2}, after={statValue:F2}");
        evt?.Invoke(statValue);
    }
}