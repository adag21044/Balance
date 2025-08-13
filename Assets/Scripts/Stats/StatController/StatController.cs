using UnityEngine;

public class StatController : MonoBehaviour
{
    [SerializeField] private StatView statView;
    private StatModel statModel;

    private void Awake()
    {
        statModel = new StatModel();
        statModel.OnHeartChanged += statView.UpdateHeartValue;
        statModel.OnCareerChanged += statView.UpdateCareerValue;
        statModel.OnHappinessChanged += statView.UpdateHappinessValue;
    }
}