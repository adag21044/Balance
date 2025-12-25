using UnityEngine;

public class RunController : MonoBehaviour
{
    [SerializeField] private ProgressConfig progressConfig;

    private ProgressManager progressManager;

    public float CurrentProgress01 =>
        progressManager.CurrentProgress / 100f;

    private void Start()
    {
        StartNewRun();
    }

    public void StartNewRun()
    {
        progressManager = new ProgressManager(progressConfig);
        LogProgress();
    }

    // 🔥 BU METOT ŞART
    public void OnCardDisplayed(CardSO card)
    {
        progressManager.OnCardShown(card);
        LogProgress();
    }

    private void LogProgress()
    {
        Debug.Log($"[RunController] Progress: {progressManager.CurrentProgress:0.00}%");
    }

    public void EndRun()
    {
        progressManager.ResetRun();
        LogProgress();
    }
}
