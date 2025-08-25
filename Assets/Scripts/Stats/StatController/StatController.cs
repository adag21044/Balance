using UnityEngine;

public class StatController : MonoBehaviour
{
    public StatView statView;
    public StatModel statModel;

    private void Awake()
    {
        statModel = StatModel.Instance;
        statModel.OnHeartChanged += statView.UpdateHeartValue;
        statModel.OnCareerChanged += statView.UpdateCareerValue;
        statModel.OnHappinessChanged += statView.UpdateHappinessValue;
        statModel.OnAgeChanged += age => statView.UpdateAgeText(age);

        StatModel.OnHeartAffected += () => statView.ShowHeartPointer(true);
        StatModel.OnCareerAffected += () => statView.ShowCareerPointer(true);
        StatModel.OnHappinessAffected += () => statView.ShowHappinessPointer(true);

        statView.ShowHeartPointer(false);
        statView.ShowCareerPointer(false);
        statView.ShowHappinessPointer(false);

        statView.SnapToModel(statModel);
    }
}