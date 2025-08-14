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

    private Vector3 heartBaseScale;
    private Vector3 careerBaseScale;
    private Vector3 happinessBaseScale;

    private void Awake()
    {
        heartBaseScale = heartPointer.rectTransform.localScale;
        careerBaseScale = careerPointer.rectTransform.localScale;
        happinessBaseScale = happinessPointer.rectTransform.localScale;
    }

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
        AnimatePointer(heartPointer, show, heartBaseScale);
    }

    public void ShowCareerPointer(bool show)
    {
        AnimatePointer(careerPointer, show, careerBaseScale);
    }

    public void ShowHappinessPointer(bool show)
    {
        AnimatePointer(happinessPointer, show, happinessBaseScale);
    }

    private void AnimatePointer(Image pointer, bool show, Vector3 baseScale)
    {
        pointer.gameObject.SetActive(true);

        float targetAlpha = show ? 1f : 0f;
        Vector3 targetScale = show ? baseScale : baseScale * 0.8f;

        pointer.DOFade(targetAlpha, duration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                if (!show)
                    pointer.gameObject.SetActive(false);
            });

        pointer.rectTransform.DOScale(targetScale, duration)
            .SetEase(Ease.OutBack);
    }
}