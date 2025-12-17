using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
    [SerializeField] private SoundManager soundManager;

    private void Awake()
    {
        // Robust singleton guard
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        Debug.Log("[GameManager] OnEnable");

        // Build delegates ONCE per enable
        onHeartFinished = _ => FinishGame(GameOverCause.Heart);
        onCareerFinished = _ => FinishGame(GameOverCause.Career);
        onHappinessFinished = _ => FinishGame(GameOverCause.Happiness);
        onSociabilityFinished = _ => FinishGame(GameOverCause.Sociability);

        var sm = StatModel.Instance;
        sm.OnHeartFinished += onHeartFinished;
        sm.OnCareerFinished += onCareerFinished;
        sm.OnHappinessFinished += onHappinessFinished;
        sm.OnSociabilityFinished += onSociabilityFinished;
    }

    private void OnDisable()
    {
        var sm = StatModel.Instance;
        if (sm == null) return;

        // Unsubscribe exactly what we subscribed
        if (onHeartFinished != null) sm.OnHeartFinished -= onHeartFinished;
        if (onCareerFinished != null) sm.OnCareerFinished -= onCareerFinished;
        if (onHappinessFinished != null) sm.OnHappinessFinished -= onHappinessFinished;
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
        Debug.Log($"[GameManager] FinishGame CALLED with cause={cause}");



        // 1-based -> 0-based index conversion is handled below
        switch (cause)
        {
            case GameOverCause.Heart:
                cardController.SetEndGameCardByIndices(0, 3);

                statController.statView.AnimateFailAnimation();
                soundManager.PlayHeartFailSound();
                SaveSystem.Instance.ResetStats(statModel);
                Debug.Log("Heart cause");
                StartCoroutine(ReloadSceneAfterDelay(5f));
                break;

            case GameOverCause.Career:
                cardController.SetEndGameCardByIndices(3, 5);

                statController.statView.AnimateFailAnimation();
                soundManager.PlayCareerFailSound();
                SaveSystem.Instance.ResetStats(statModel);
                Debug.Log("Career cause");
                StartCoroutine(ReloadSceneAfterDelay(5f));
                break;

            case GameOverCause.Happiness:
                cardController.SetEndGameCardByIndex(6);

                statController.statView.AnimateFailAnimation();
                soundManager.PlayHappinessFailSound();
                SaveSystem.Instance.ResetStats(statModel);
                Debug.Log("Happiness cause");
                StartCoroutine(ReloadSceneAfterDelay(5f));
                break;

            case GameOverCause.Sociability:
                cardController.SetEndGameCardByIndex(5);

                statController.statView.AnimateFailAnimation();
                soundManager.PlaySociabilityFailSound();
                SaveSystem.Instance.ResetStats(statModel);
                Debug.Log("Sociability cause");
                StartCoroutine(ReloadSceneAfterDelay(5f));
                break;

            default:
                Debug.LogError($"[GameManager] FinishGame called with unknown cause: {cause}");
                StartCoroutine(ReloadSceneAfterDelay(5f));
                break;
        }
        Debug.Log($"[GameManager] Game Over by {cause}");


        gameFinished = true;
        if (gameFinished) return; // guard
    }
    
    private IEnumerator ReloadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 🔴 KRİTİK SATIR
        StatModel.ResetStaticEvents();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

public enum GameOverCause
{
    Heart,
    Career,
    Happiness,
    Sociability
}