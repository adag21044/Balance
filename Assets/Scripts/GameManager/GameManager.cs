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

        StatModel.Instance.OnHeartFinished += onHeartFinished;
        StatModel.Instance.OnCareerFinished += onCareerFinished;
        StatModel.Instance.OnHappinessFinished += onHappinessFinished;
    }

    private void OnDisable()
    {
        StatModel.Instance.OnHeartFinished -= onHeartFinished;
        StatModel.Instance.OnCareerFinished -= onCareerFinished;
        StatModel.Instance.OnHappinessFinished -= onHappinessFinished;
    }

    public void FinishGame()
    {
        StatModel.Instance.RaiseHeartFinished(zero);
        StatModel.Instance.RaiseCareerFinished(zero);
        StatModel.Instance.RaiseHappinessFinished(zero);

        Debug.Log("Game Over");
    }
}