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

    [Header("Tween Settings")]
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private bool useUnscaledTime = true; // run even if Time.timeScale == 0

    // Keep last tweens to avoid stacking
    private Tween heartTween, careerTween, happinessTween;

    private Vector3 heartBaseScale;
    private Vector3 careerBaseScale;
    private Vector3 happinessBaseScale;

    private void Awake()
    {
        heartBaseScale = heartPointer.rectTransform.localScale;
        careerBaseScale = careerPointer.rectTransform.localScale;
        happinessBaseScale = happinessPointer.rectTransform.localScale;
    }

    private void OnDisable()
    {
        // kill tweens to avoid ghosts
        heartTween?.Kill(false);
        careerTween?.Kill(false);
        happinessTween?.Kill(false);
    }

    // Call once after you subscribe events (no tween; just snap to model)
    public void SnapToModel(StatModel model)
    {
        heartImage.fillAmount     = Mathf.Clamp01(model.HeartPercantage);
        careerImage.fillAmount    = Mathf.Clamp01(model.CareerPercantage);
        happinessImage.fillAmount = Mathf.Clamp01(model.HappinessPercantage);
    }

    [Method] // optional if you use any inspector tool
    public void UpdateHeartValue(float value)
    {
        heartTween?.Kill(false); // kill previous tween targeting this image
        heartTween = heartImage
            .DOFillAmount(Mathf.Clamp01(value), duration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(useUnscaledTime);
    }

    [Method]
    public void UpdateCareerValue(float value)
    {
        careerTween?.Kill(false);
        careerTween = careerImage
            .DOFillAmount(Mathf.Clamp01(value), duration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(useUnscaledTime);
    }

    [Method]
    public void UpdateHappinessValue(float value)
    {
        happinessTween?.Kill(false);
        happinessTween = happinessImage
            .DOFillAmount(Mathf.Clamp01(value), duration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(useUnscaledTime);
    }

    public void ShowHeartPointer(bool show)    => AnimatePointer(heartPointer, show, heartBaseScale);
    public void ShowCareerPointer(bool show)   => AnimatePointer(careerPointer, show, careerBaseScale);
    public void ShowHappinessPointer(bool show)=> AnimatePointer(happinessPointer, show, happinessBaseScale);

    private void AnimatePointer(Image pointer, bool show, Vector3 baseScale)
    {
        pointer.gameObject.SetActive(true);

        float targetAlpha = show ? 1f : 0f;
        Vector3 targetScale = show ? baseScale : baseScale * 0.8f;

        pointer.DOFade(targetAlpha, duration)
               .SetEase(Ease.InOutSine)
               .OnComplete(() => { if (!show) pointer.gameObject.SetActive(false); })
               .SetUpdate(useUnscaledTime);

        pointer.rectTransform.DOScale(targetScale, duration)
               .SetEase(Ease.OutBack)
               .SetUpdate(useUnscaledTime);
    }
}
