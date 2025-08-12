using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class StatView : MonoBehaviour
{
    [SerializeField] private Image heartImage;
    [SerializeField] private Image careerImage;
    [SerializeField] private Image happinessImage;

    [SerializeField] private float duration = 0.5f;

    [ContextMenu("Update Heart")]
    private void UpdateHeartValue()
    {
        heartImage.DOFillAmount(0.5f, duration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => Debug.Log("Heart value updated to: " + 0.5f));
    }

    private void UpdateCareerValue(float value)
    {
        heartImage.DOFillAmount(value, duration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => Debug.Log("Career value updated to: " + value));
    }

    private void UpdateHappinessValue(float value)
    {
        heartImage.DOFillAmount(value, duration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => Debug.Log("Happiness value updated to: " + value));
    }
}