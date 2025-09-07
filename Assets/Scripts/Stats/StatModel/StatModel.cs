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

    private float sociabilityPercantage;
    public float SociabilityPercantage => sociabilityPercantage;

    public float age;

    public event Action<float> OnHeartChanged;
    public event Action<float> OnCareerChanged;
    public event Action<float> OnHappinessChanged;
    public event Action<float> OnAgeChanged;
    public event Action<float> OnSociabilityChanged;

    public static event Action OnHeartAffected;
    public static event Action OnCareerAffected;
    public static event Action OnHappinessAffected;
    public static event Action OnAgeAffected;
    public static event Action OnSociabilityAffected;

    public event Action<float> OnHeartFinished;
    public event Action<float> OnCareerFinished;
    public event Action<float> OnHappinessFinished;
    public event Action<float> OnSociabilityFinished;


    public static event Action OnPreviewCancelled;
    public static event Action OnFail;

    public const float IMPACT_SCALE = 0.01f;

    public StatModel()
    {
        heartPercantage = 0.5f;
        careerPercantage = 0.5f;
        happinessPercantage = 0.5f;
        sociabilityPercantage = 0.5f;
        age = 18f; // Initialize age
    }

    /// <summary>
    /// Preview pointers while user starts dragging.
    /// We don't know direction yet, so show pointers for any stat that will change on either side.
    /// </summary>
    public static void PreviewImpacts(CardSO card)
    {
        if (card == null) return;

        if (card.leftHeartImpact != 0 || card.rightHeartImpact != 0) OnHeartAffected?.Invoke();
        if (card.leftCareerImpact != 0 || card.rightCareerImpact != 0) OnCareerAffected?.Invoke();
        if (card.leftHappinessImpact != 0 || card.rightHappinessImpact != 0) OnHappinessAffected?.Invoke();
        if (card.leftSociabilityImpact != 0 || card.rightSociabilityImpact != 0) OnSociabilityAffected?.Invoke();
        if (card.ageImpact != 0) OnAgeAffected?.Invoke();
    }

    /// <summary>
    /// Apply impacts according to swipe direction.
    /// </summary>
    public void ApplyCard(CardSO card, SwipeDirection dir)
    {
        Debug.Log($"Applying {dir} impacts: {card?.name}");
        if (card == null) return;

        if (dir == SwipeDirection.Left)
        {
            if (card.leftHeartImpact != 0)
                ApplyAndRaise(ref heartPercantage, card.leftHeartImpact * IMPACT_SCALE, OnHeartChanged, OnHeartFinished);
            if (card.leftCareerImpact != 0)
                ApplyAndRaise(ref careerPercantage, card.leftCareerImpact * IMPACT_SCALE, OnCareerChanged, OnCareerFinished);
            if (card.leftHappinessImpact != 0)
                ApplyAndRaise(ref happinessPercantage, card.leftHappinessImpact * IMPACT_SCALE, OnHappinessChanged, OnHappinessFinished);
            if (card.leftSociabilityImpact != 0)
                ApplyAndRaise(ref sociabilityPercantage, card.leftSociabilityImpact * IMPACT_SCALE, OnSociabilityChanged, OnSociabilityFinished);
        }
        else // Right
        {
            if (card.rightHeartImpact != 0)
                ApplyAndRaise(ref heartPercantage, card.rightHeartImpact * IMPACT_SCALE, OnHeartChanged, OnHeartFinished);
            if (card.rightCareerImpact != 0)
                ApplyAndRaise(ref careerPercantage, card.rightCareerImpact * IMPACT_SCALE, OnCareerChanged, OnCareerFinished);
            if (card.rightHappinessImpact != 0)
                ApplyAndRaise(ref happinessPercantage, card.rightHappinessImpact * IMPACT_SCALE, OnHappinessChanged, OnHappinessFinished);
            if (card.rightSociabilityImpact != 0)
                ApplyAndRaise(ref sociabilityPercantage, card.rightSociabilityImpact * IMPACT_SCALE, OnSociabilityChanged, OnSociabilityFinished);
        }

        // Age update
        if (card.ageImpact != 0)
        {
            float beforeAge = age;
            age += card.ageImpact / 2; // burada istediğin mantığa göre clamp ekleyebilirsin
            Debug.Log($"[StatModel] age before={beforeAge}, impact={card.ageImpact}, after={age}");
            OnAgeChanged?.Invoke(age);
        }
    }

    private void ApplyAndRaise(ref float statValue, float delta, Action<float> evt, Action<float> finishedEvt)
    {
        float before = statValue;
        statValue = Mathf.Clamp01(statValue + delta);
        Debug.Log($"[StatModel] stat before={before:F2}, delta={delta:F2}, after={statValue:F2}");
        evt?.Invoke(statValue);

        // Call finished event if stat reached 0 or below
        if (statValue <= 0.001f)
        {
            Debug.Log("[StatModel] Stat finished!");
            finishedEvt?.Invoke(statValue);
        }
    }


    public static void CancelPreview() => OnPreviewCancelled?.Invoke();

    public void RaiseCareerFinished(float value) => OnCareerFinished?.Invoke(value);
    public void RaiseHeartFinished(float value) => OnHeartFinished?.Invoke(value);
    public void RaiseHappinessFinished(float value) => OnHappinessFinished?.Invoke(value);
    public void RaiseSociabilityFinished(float value) => OnSociabilityFinished?.Invoke(value);
}
