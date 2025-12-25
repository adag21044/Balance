using UnityEngine;

public class ProgressManager
{
    private readonly ProgressTracker tracker;
    private readonly ProgressConfig config;

    private float currentProgress;
    private readonly float baseProgressPerCard;

    public float CurrentProgress => currentProgress;

    public ProgressManager(ProgressConfig config)
    {
        this.config = config;
        tracker = new ProgressTracker();

        baseProgressPerCard = 100f / config.totalCardCount;
        currentProgress = 0f;
    }

    public void OnCardShown(CardSO card)
    {
        if (!card.contributesToProgress)
            return;

        // Aynı kart tekrar gelirse sayma
        if (!tracker.TryRegister(card))
            return;

        if (card.isFinalCard)
        {
            currentProgress = config.finalCardProgress;
            return;
        }

        float progressDelta = baseProgressPerCard;

        if (card.compensatesSkippedCard)
            progressDelta *= 2f;

        currentProgress += progressDelta;

        // %100 sadece final kartta
        currentProgress = Mathf.Min(currentProgress, 99f);
    }

    public void ResetRun()
    {
        tracker.Reset();
        currentProgress = 0f;
    }
}
