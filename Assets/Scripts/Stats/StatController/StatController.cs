using UnityEngine;

public class StatController : MonoBehaviour
{
    public StatView statView;
    private StatModel statModel;

    private void Awake()
    {
        statModel = StatModel.Instance;

        SaveSystem.Instance.LoadStats(statModel);

        statView.SnapToModel(statModel);
        statView.UpdateAgeText(statModel.age);
    }

    private void OnEnable()
    {
        // VALUE EVENTS
        statModel.OnHeartChanged += statView.UpdateHeartValue;
        statModel.OnCareerChanged += statView.UpdateCareerValue;
        statModel.OnHappinessChanged += statView.UpdateHappinessValue;
        statModel.OnSociabilityChanged += statView.UpdateSociabilityValue;
        statModel.OnAgeChanged += statView.UpdateAgeText;

        // POINTER PREVIEW EVENTS
        StatModel.OnHeartAffected += OnHeartAffected;
        StatModel.OnCareerAffected += OnCareerAffected;
        StatModel.OnHappinessAffected += OnHappinessAffected;
        StatModel.OnSociabilityAffected += OnSociabilityAffected;

        // PREVIEW CANCEL
        StatModel.OnPreviewCancelled += HideAllPointers;

        // FAIL
        StatModel.OnFail += OnFail;
    }

    private void OnDisable()
    {
        if (statModel == null) return;

        statModel.OnHeartChanged -= statView.UpdateHeartValue;
        statModel.OnCareerChanged -= statView.UpdateCareerValue;
        statModel.OnHappinessChanged -= statView.UpdateHappinessValue;
        statModel.OnSociabilityChanged -= statView.UpdateSociabilityValue;
        statModel.OnAgeChanged -= statView.UpdateAgeText;

        StatModel.OnHeartAffected -= OnHeartAffected;
        StatModel.OnCareerAffected -= OnCareerAffected;
        StatModel.OnHappinessAffected -= OnHappinessAffected;
        StatModel.OnSociabilityAffected -= OnSociabilityAffected;

        StatModel.OnPreviewCancelled -= HideAllPointers;
        StatModel.OnFail -= OnFail;
    }

    // ================= POINTER CALLBACKS =================

    private void OnHeartAffected() => statView.ShowHeartPointer(true);
    private void OnCareerAffected() => statView.ShowCareerPointer(true);
    private void OnHappinessAffected() => statView.ShowHappinessPointer(true);
    private void OnSociabilityAffected() => statView.ShowSociabilityPointer(true);

    private void HideAllPointers()
    {
        statView.ShowHeartPointer(false);
        statView.ShowCareerPointer(false);
        statView.ShowHappinessPointer(false);
        statView.ShowSociabilityPointer(false);
    }

    private void OnFail()
    {
        SaveSystem.Instance.ResetStats(statModel);
        statView.SnapToModel(statModel);
        statView.UpdateAgeText(statModel.age);
        HideAllPointers();
    }
}
