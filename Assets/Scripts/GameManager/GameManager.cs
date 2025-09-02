using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private CardController cardController;
    [SerializeField] private StatController statController;

    private float zero = 0f;

    private Action<float> onHeartFinished;
    private Action<float> onCareerFinished;
    private Action<float> onHappinessFinished;
    private Action<float> onSociabilityFinished;

    public StatModel statModel => StatModel.Instance;

    private bool gameFinished = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        Debug.Log("[GameManager] OnEnable");

        onHeartFinished = _ => FinishGame();
        onCareerFinished = _ => FinishGame();
        onHappinessFinished = _ => FinishGame();
        onSociabilityFinished = _ => FinishGame();

        StatModel.Instance.OnHeartFinished += onHeartFinished;
        StatModel.Instance.OnCareerFinished += onCareerFinished;
        StatModel.Instance.OnHappinessFinished += onHappinessFinished;
        StatModel.Instance.OnSociabilityFinished += onSociabilityFinished;
    }

    private void OnDisable()
    {
        StatModel.Instance.OnHeartFinished -= onHeartFinished;
        StatModel.Instance.OnCareerFinished -= onCareerFinished;
        StatModel.Instance.OnHappinessFinished -= onHappinessFinished;
        StatModel.Instance.OnSociabilityFinished -= onSociabilityFinished;
    }

    public void FinishGame(GameOverCause cause)
    {
        if (gameFinished) return; // guard
        gameFinished = true;

        // 1-based -> 0-based index conversion is handled below
        switch (cause)
        {
            case GameOverCause.Heart:
                // 2nd or 5th -> indices 1 or 4
                cardController.SetEndGameCardByIndices(new[] { 1, 4 });
                break;

            case GameOverCause.Career:
                // 3rd or 4th -> indices 2 or 3
                cardController.SetEndGameCardByIndices(new[] { 2, 3 });
                break;

            case GameOverCause.Happiness:
                // 1st -> index 0
                cardController.SetEndGameCardByIndex(0);
                break;

            case GameOverCause.Sociability:
                // 6th -> index 5
                cardController.SetEndGameCardByIndex(5);
                break;
        }

        Debug.Log($"[GameManager] Game Over by {cause}");
    }

    public void FinishGame()
    {
        if (gameFinished) return;
        gameFinished = true;
        cardController.SetEndGameCard(statModel);
        Debug.Log("[GameManager] Game Over (generic)");
    }
}

public enum GameOverCause
{
    Heart,
    Career,
    Happiness,
    Sociability
}
