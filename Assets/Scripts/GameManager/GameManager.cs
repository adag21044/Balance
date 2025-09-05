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

    public static event Action StartButtonPressed;

    public StatModel statModel => StatModel.Instance;

    private bool gameFinished = false;
    [SerializeField] private CardInitialAnimation cardInitialAnimation;
    [SerializeField] private StartScreenAnimator startScreenAnimator;

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

        StatModel.Instance.OnHeartFinished       += _ => FinishGame(GameOverCause.Heart);
        StatModel.Instance.OnCareerFinished      += _ => FinishGame(GameOverCause.Career);
        StatModel.Instance.OnHappinessFinished   += _ => FinishGame(GameOverCause.Happiness);
        StatModel.Instance.OnSociabilityFinished += _ => FinishGame(GameOverCause.Sociability);


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

    public void OnStartButtonPressed()
    {
        Debug.Log("[GameManager] Start button pressed");
        StartButtonPressed?.Invoke();

        if (startScreenAnimator != null)
            startScreenAnimator.gameObject.SetActive(false);


        if (cardInitialAnimation != null)
            cardInitialAnimation.StartAnimation();
        
        statController.statView.AnimateAgeText(StatModel.Instance.age);
    }


    public void FinishGame(GameOverCause cause)
    {
        if (gameFinished) return; // guard
        gameFinished = true;

        // 1-based -> 0-based index conversion is handled below
        switch (cause)
        {
            case GameOverCause.Heart:
                // 0, 1, 2
                cardController.SetEndGameCardByIndices(0, 3);
                Debug.Log("Heart cause");
                break;

            case GameOverCause.Career:
                // 2
                cardController.SetEndGameCardByIndices(3, 5);
                Debug.Log("Career cause");
                break;

            case GameOverCause.Happiness:
                // 1
                cardController.SetEndGameCardByIndex(6);
                Debug.Log("Happiness cause");
                break;

            case GameOverCause.Sociability:
                // 5
                cardController.SetEndGameCardByIndex(5);
                Debug.Log("Sociability cause");
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