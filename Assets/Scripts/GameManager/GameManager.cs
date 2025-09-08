using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private CardController cardController;
    [SerializeField] private StatController statController;

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
        // Robust singleton guard
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        Debug.Log("[GameManager] OnEnable");

        // Build delegates ONCE per enable
        onHeartFinished       = _ => FinishGame(GameOverCause.Heart);
        onCareerFinished      = _ => FinishGame(GameOverCause.Career);
        onHappinessFinished   = _ => FinishGame(GameOverCause.Happiness);
        onSociabilityFinished = _ => FinishGame(GameOverCause.Sociability);

        var sm = StatModel.Instance;
        sm.OnHeartFinished       += onHeartFinished;
        sm.OnCareerFinished      += onCareerFinished;
        sm.OnHappinessFinished   += onHappinessFinished;
        sm.OnSociabilityFinished += onSociabilityFinished;
    }

    private void OnDisable()
    {
        var sm = StatModel.Instance;
        if (sm == null) return;

        // Unsubscribe exactly what we subscribed
        if (onHeartFinished       != null) sm.OnHeartFinished       -= onHeartFinished;
        if (onCareerFinished      != null) sm.OnCareerFinished      -= onCareerFinished;
        if (onHappinessFinished   != null) sm.OnHappinessFinished   -= onHappinessFinished;
        if (onSociabilityFinished != null) sm.OnSociabilityFinished -= onSociabilityFinished;
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
                cardController.SetEndGameCardByIndices(0, 3);
                
                statController.statView.AnimateFailAnimation();
                Debug.Log("Heart cause");
                break;

            case GameOverCause.Career:
                cardController.SetEndGameCardByIndices(3, 5);
                
                statController.statView.AnimateFailAnimation();
                Debug.Log("Career cause");
                break;

            case GameOverCause.Happiness:
                cardController.SetEndGameCardByIndex(6);
                
                statController.statView.AnimateFailAnimation();
                Debug.Log("Happiness cause");
                break;

            case GameOverCause.Sociability:
                cardController.SetEndGameCardByIndex(5);
                
                statController.statView.AnimateFailAnimation();
                Debug.Log("Sociability cause");
                break;
        }

        Debug.Log($"[GameManager] Game Over by {cause}");
    }

    public void FinishGame()
    {
        cardController.ForceEndCardRotation(0f); 
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