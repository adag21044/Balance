using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;

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
    [SerializeField] private RunController runController;

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

    private async void Start()
    {
        await LoadGameAsync();
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
        StartGameFlowAsync().Forget();
    }

    private async UniTaskVoid StartGameFlowAsync()
    {
        Debug.Log("[GameManager] Start button pressed");
        StartButtonPressed?.Invoke();

        var token = this.GetCancellationTokenOnDestroy();

        // 1️⃣ Fade (dark → light)
        if (startScreenAnimator != null)
            await startScreenAnimator.PlayStartTransitionAsync(token);

        // 2️⃣ Fade bittikten sonra deck animasyonu
        if (cardInitialAnimation != null)
            cardInitialAnimation.StartAnimation();

        statController.statView.AnimateAgeText(StatModel.Instance.age);
    }


    public void FinishGame(GameOverCause cause)
    {
        // Guard: prevent double finish triggers from multiple stat events
        if (gameFinished)
            return;

        gameFinished = true;

        Debug.Log($"[GameManager] FinishGame CALLED with cause={cause}");

        switch (cause)
        {
            case GameOverCause.Heart:
                cardController.SetEndGameCardByIndices(0, 3);
                statController.statView.AnimateFailAnimation();
                soundManager.PlayHeartFailSound();
                SaveSystem.Instance.ResetStatsOnFail(statModel);
                Debug.Log("Heart cause");
                ReloadSceneAfterDelayAsync(5f).Forget();
                break;

            case GameOverCause.Career:
                cardController.SetEndGameCardByIndices(3, 5);
                statController.statView.AnimateFailAnimation();
                soundManager.PlayCareerFailSound();
                SaveSystem.Instance.ResetStatsOnFail(statModel);
                Debug.Log("Career cause");
                ReloadSceneAfterDelayAsync(5f).Forget();
                break;

            case GameOverCause.Happiness:
                cardController.SetEndGameCardByIndex(6);
                statController.statView.AnimateFailAnimation();
                soundManager.PlayHappinessFailSound();
                SaveSystem.Instance.ResetStatsOnFail(statModel);
                Debug.Log("Happiness cause");
                ReloadSceneAfterDelayAsync(5f).Forget();
                break;

            case GameOverCause.Sociability:
                cardController.SetEndGameCardByIndex(5);
                statController.statView.AnimateFailAnimation();
                soundManager.PlaySociabilityFailSound();
                SaveSystem.Instance.ResetStatsOnFail(statModel);
                Debug.Log("Sociability cause");
                ReloadSceneAfterDelayAsync(5f).Forget();
                break;

            default:
                Debug.LogError($"[GameManager] FinishGame called with unknown cause: {cause}");
                ReloadSceneAfterDelayAsync(5f).Forget();
                break;
        }

        Debug.Log($"[GameManager] Game Over by {cause}");
    }

    private async UniTaskVoid ReloadSceneAfterDelayAsync(float delaySeconds)
    {
        // CancellationToken: auto-cancel if GameObject is destroyed during scene change
        CancellationToken token = this.GetCancellationTokenOnDestroy();

        try
        {
            // WaitForSeconds-equivalent (scaled time).
            // If you want it to work even when Time.timeScale == 0, set ignoreTimeScale: true.
            await UniTask.Delay(
                TimeSpan.FromSeconds(delaySeconds),
                ignoreTimeScale: false,
                cancellationToken: token
            );
        }
        catch (OperationCanceledException)
        {
            // Object destroyed or scene changed before delay finished
            return;
        }

        StatModel.ResetStaticEvents();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private async UniTask LoadGameAsync()
    {
        if (startScreenAnimator != null)
            startScreenAnimator.gameObject.SetActive(false);

        LoadingUI.Instance.Show();

        // %0 → %20 (loading başladı hissi)
        LoadingUI.Instance.SetProgress(0.2f);
        await UniTask.Delay(200);

        // UI init
        statController.InitializeFromModel();

        // %100
        await LoadingUI.Instance.CompleteAndHide();

        startScreenAnimator.EnableBlinkText();
        
        if (startScreenAnimator != null)
            startScreenAnimator.gameObject.SetActive(true);
    }
}

public enum GameOverCause
{
    Heart,
    Career,
    Happiness,
    Sociability
}
