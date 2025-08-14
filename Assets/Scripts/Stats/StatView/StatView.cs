using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class StatView : MonoBehaviour
{
    [SerializeField] private Image heartImage;
    [SerializeField] private Image careerImage;
    [SerializeField] private Image happinessImage;

    [Header("Pointers")]
    [SerializeField] private Image heartPointer;
    [SerializeField] private Image careerPointer;
    [SerializeField] private Image happinessPointer;

    [SerializeField] private float duration = 0.5f;

    [Method]
    public void UpdateHeartValue(float value)
    {
        heartImage.DOFillAmount(value, duration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => Debug.Log("Heart value updated to: " + value));
    }

    [Method]
    public void UpdateCareerValue(float value)
    {
        careerImage.DOFillAmount(value, duration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => Debug.Log("Career value updated to: " + value));
    }

    [Method]
    public void UpdateHappinessValue(float value)
    {
        happinessImage.DOFillAmount(value, duration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => Debug.Log("Happiness value updated to: " + value));
    }

    public void ShowHeartPointer(bool show)
    {
        heartPointer.gameObject.SetActive(show);
    }

    public void ShowCareerPointer(bool show)
    {
        careerPointer.gameObject.SetActive(show);
    }

    public void ShowHappinessPointer(bool show)
    {
        happinessPointer.gameObject.SetActive(show);
    }
}