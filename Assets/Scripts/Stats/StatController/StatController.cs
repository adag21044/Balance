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
        statModel.OnSociabilityChanged += statView.UpdateSociabilityValue;

        statModel.OnAgeChanged += age =>
        {
            statView.UpdateAgeText(age);
            SaveSystem.Instance.SaveStats(statModel); // yaş değiştikçe kaydet
        };

        statModel.OnAgeChanged += age => statView.UpdateAgeText(age);

        SaveSystem.Instance.LoadStats(statModel);
        statView.SnapToModel(statModel);
        statView.UpdateAgeText(statModel.age);

        StatModel.OnHeartAffected += () => statView.ShowHeartPointer(true);
        StatModel.OnCareerAffected += () => statView.ShowCareerPointer(true);
        StatModel.OnHappinessAffected += () => statView.ShowHappinessPointer(true);
        StatModel.OnSociabilityAffected += () => statView.ShowSociabilityPointer(true);




        StatModel.OnFail += () =>
        {
            Debug.Log("Fail state → Resetting stats");
            SaveSystem.Instance.ResetStats(statModel);
            statView.SnapToModel(statModel);
            statView.UpdateAgeText(statModel.age);
        };

        statView.ShowHeartPointer(false);
        statView.ShowCareerPointer(false);
        statView.ShowHappinessPointer(false);
        statView.ShowSociabilityPointer(false);

        statView.SnapToModel(statModel);

        // Hide all pointers when preview is cancelled
        StatModel.OnPreviewCancelled += () =>
        {
            statView.ShowHeartPointer(false);
            statView.ShowCareerPointer(false);
            statView.ShowHappinessPointer(false);
            statView.ShowSociabilityPointer(false);
        };
        
        statModel.OnHeartChanged += _ => SaveSystem.Instance.SaveStats(statModel);
        statModel.OnCareerChanged += _ => SaveSystem.Instance.SaveStats(statModel);
        statModel.OnHappinessChanged += _ => SaveSystem.Instance.SaveStats(statModel);
        statModel.OnSociabilityChanged += _ => SaveSystem.Instance.SaveStats(statModel);
    }
    
    private void OnDestroy()
    {
        if (statModel != null)
        {
            statModel.OnHeartChanged -= statView.UpdateHeartValue;
            statModel.OnCareerChanged -= statView.UpdateCareerValue;
            statModel.OnHappinessChanged -= statView.UpdateHappinessValue;
            statModel.OnSociabilityChanged -= statView.UpdateSociabilityValue;
            statModel.OnAgeChanged -= statView.UpdateAgeText;
        }

        StatModel.OnHeartAffected -= () => statView.ShowHeartPointer(true);
        StatModel.OnCareerAffected -= () => statView.ShowCareerPointer(true);
        StatModel.OnHappinessAffected -= () => statView.ShowHappinessPointer(true);
        StatModel.OnSociabilityAffected -= () => statView.ShowSociabilityPointer(true);

        StatModel.OnPreviewCancelled -= () =>
        {
            statView.ShowHeartPointer(false);
            statView.ShowCareerPointer(false);
            statView.ShowHappinessPointer(false);
            statView.ShowSociabilityPointer(false);
        };
    }
}