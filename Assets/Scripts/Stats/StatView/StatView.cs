using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class StatView : MonoBehaviour
{
    [SerializeField] private Image heartImage;
    [SerializeField] private Image careerImage;
    [SerializeField] private Image happinessImage;
    [SerializeField] private Image sociabilityImage;

    [SerializeField] private TextMeshProUGUI ageText;

    [Header("Pointers")]
    [SerializeField] private Image heartPointer;
    [SerializeField] private Image careerPointer;
    [SerializeField] private Image happinessPointer;
    [SerializeField] private Image sociabilityPointer;


    [Header("Tween Settings")]
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private bool useUnscaledTime = true; // run even if Time.timeScale == 0

    [Header("Flash Colors")]
    [SerializeField] private Color increaseColor = new Color(0.25f, 1f, 0.25f); // green-ish
    [SerializeField] private Color decreaseColor = new Color(1f, 0.25f, 0.25f); // red-ish

    // Keep last tweens to avoid stacking
    private Tween heartTween, careerTween, happinessTween, sociabilityTween;

    // Cache base colors & scales to restore quickly
    private Color heartBaseColor, careerBaseColor, happinessBaseColor, sociabilityBaseColor;
    private Vector3 heartBaseScale;
    private Vector3 careerBaseScale;
    private Vector3 happinessBaseScale;
    private Vector3 sociabilityBaseScale;

    private Tween heartPtrTween, careerPtrTween, happinessPtrTween, sociabilityPtrTween;
    [SerializeField] private float pointerAutoHideDelay = 0.9f; // seconds

    private Tween heartPtrTimer, careerPtrTimer, happinessPtrTimer, sociabilityPtrTimer;

    [SerializeField] private float ageAnimationDuration = 2f; // total duration
    [SerializeField] private AnimationCurve ageEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Image CardImage;
    [SerializeField] private Image CardBackImage;
    [SerializeField] private Image TopImage;
    [SerializeField] private Image BottomImage;
    [SerializeField] private Camera mainCamera;


    private void Awake()
    {
        // Cache original colors so we can restore after the flash
        heartBaseColor = heartImage.color;
        careerBaseColor = careerImage.color;
        happinessBaseColor = happinessImage.color;
        sociabilityBaseColor = sociabilityImage.color;

        heartBaseScale = heartPointer.rectTransform.localScale;
        careerBaseScale = careerPointer.rectTransform.localScale;
        happinessBaseScale = happinessPointer.rectTransform.localScale;
        sociabilityBaseScale = sociabilityPointer.rectTransform.localScale;
    }

    private void OnDisable()
    {
        heartTween?.Kill(true);
        careerTween?.Kill(true);
        happinessTween?.Kill(true);
        sociabilityTween?.Kill(true);

        heartPtrTween?.Kill(true);
        careerPtrTween?.Kill(true);
        happinessPtrTween?.Kill(true);
        sociabilityPtrTween?.Kill(true);

        heartPtrTimer?.Kill(false);
        careerPtrTimer?.Kill(false);
        happinessPtrTimer?.Kill(false);
        sociabilityPtrTimer?.Kill(false);

        heartPointer.gameObject.SetActive(false);
        careerPointer.gameObject.SetActive(false);
        happinessPointer.gameObject.SetActive(false);
        sociabilityPointer.gameObject.SetActive(false);
    }


    // Call once after you subscribe events (no tween; just snap to model)
    public void SnapToModel(StatModel model)
    {
        heartImage.fillAmount = Mathf.Clamp01(model.HeartPercantage);
        careerImage.fillAmount = Mathf.Clamp01(model.CareerPercantage);
        happinessImage.fillAmount = Mathf.Clamp01(model.HappinessPercantage);
        sociabilityImage.fillAmount = Mathf.Clamp01(model.SociabilityPercantage);
    }

    [Method] // optional if you use any inspector tool
    public void UpdateHeartValue(float value)
    {
        AnimateBar(
            image: heartImage,
            tweenRef: ref heartTween,
            newValue: value,
            baseColor: heartBaseColor
        );
    }

    [Method]
    public void UpdateCareerValue(float value)
    {
        AnimateBar(
            image: careerImage,
            tweenRef: ref careerTween,
            newValue: value,
            baseColor: careerBaseColor
        );
    }

    [Method]
    public void UpdateHappinessValue(float value)
    {
        AnimateBar(
            image: happinessImage,
            tweenRef: ref happinessTween,
            newValue: value,
            baseColor: happinessBaseColor
        );
    }

    [Method]
    public void UpdateSociabilityValue(float value)
    {
        AnimateBar(
            image: sociabilityImage,
            tweenRef: ref sociabilityTween,
            newValue: value,
            baseColor: sociabilityBaseColor
        );
    }

    // Core helper: flash color (green/red) during fill tween, then instantly restore base color
    private void AnimateBar(Image image, ref Tween tweenRef, float newValue, Color baseColor)
    {
        // Kill any previous animation on this bar
        tweenRef?.Kill(false);

        float current = image.fillAmount;
        float target = Mathf.Clamp01(newValue);

        // If no change, just ensure color is base and return
        if (Mathf.Approximately(current, target))
        {
            image.fillAmount = target;
            image.color = baseColor;
            return;
        }

        // Decide flash color: green on increase, red on decrease
        Color flashColor = (target > current) ? increaseColor : decreaseColor;

        // Build a sequence: set flash color -> animate fill -> restore base color
        Sequence seq = DOTween.Sequence().SetUpdate(useUnscaledTime);
        seq.AppendCallback(() => image.color = flashColor);
        seq.Join(image
            .DOFillAmount(target, duration)
            .SetEase(Ease.InOutSine));
        seq.AppendCallback(() => image.color = baseColor); // instant restore on complete

        tweenRef = seq;
    }

    public void ShowHeartPointer(bool show)
    {
        AnimatePointer(heartPointer, show, heartBaseScale, ref heartPtrTween);
        heartPtrTimer?.Kill(false);

        if (show)
        {
            heartPtrTimer = DOVirtual.DelayedCall(
                duration + pointerAutoHideDelay,
                () => AnimatePointer(heartPointer, false, heartBaseScale, ref heartPtrTween)
            ).SetUpdate(useUnscaledTime);
        }
    }

    public void ShowCareerPointer(bool show)
    {
        AnimatePointer(careerPointer, show, careerBaseScale, ref careerPtrTween);

        careerPtrTimer?.Kill(false);

        if (show)
        {
            careerPtrTimer = DOVirtual.DelayedCall(
                duration + pointerAutoHideDelay,
                () => AnimatePointer(careerPointer, false, careerBaseScale, ref careerPtrTween)
            ).SetUpdate(useUnscaledTime);
        }
    }


    public void ShowHappinessPointer(bool show)
    {
        AnimatePointer(happinessPointer, show, happinessBaseScale, ref happinessPtrTween);

        happinessPtrTimer?.Kill(false);

        if (show)
        {
            happinessPtrTimer = DOVirtual.DelayedCall(
                duration + pointerAutoHideDelay,
                () => AnimatePointer(happinessPointer, false, happinessBaseScale, ref happinessPtrTween)
            ).SetUpdate(useUnscaledTime);
        }
    }

    public void ShowSociabilityPointer(bool show)
    {
        // animate now
        AnimatePointer(sociabilityPointer, show, sociabilityBaseScale, ref sociabilityPtrTween);

        // cancel previous timer
        sociabilityPtrTimer?.Kill(false);

        // if showing, arm an auto-hide in case CancelPreview doesn't come
        if (show)
        {
            sociabilityPtrTimer = DOVirtual.DelayedCall(
                duration + pointerAutoHideDelay,
                () => AnimatePointer(sociabilityPointer, false, sociabilityBaseScale, ref sociabilityPtrTween)
            ).SetUpdate(useUnscaledTime);
        }
    }



    private void AnimatePointer(Image pointer, bool show, Vector3 baseScale, ref Tween tweenRef)
    {
        // Complete previous tween so its OnComplete runs (hides object if needed)
        if (tweenRef != null && tweenRef.IsActive())
            tweenRef.Kill(true); // <-- CHANGED: true (complete), not false

        pointer.gameObject.SetActive(true);

        float targetAlpha = show ? 1f : 0f;
        Vector3 targetScale = show ? baseScale : baseScale * 0.8f;

        var seq = DOTween.Sequence().SetUpdate(useUnscaledTime);
        seq.Join(pointer.DOFade(targetAlpha, duration).SetEase(Ease.InOutSine));
        seq.Join(pointer.rectTransform.DOScale(targetScale, duration).SetEase(show ? Ease.OutBack : Ease.InOutSine));
        seq.OnComplete(() => { if (!show) pointer.gameObject.SetActive(false); });

        tweenRef = seq;
    }



    [Method]
    public void UpdateAgeText(float age)
    {
        ageText.text = $"{Mathf.FloorToInt(age)}"; // Display age as an integer
    }

    public void AnimateAgeText(float targetAge)
    {
        Debug.Log($"Animating age text to {targetAge}");
        float startValue = 0f;

        DOTween.To(() => startValue, x =>
        {
            startValue = x;
            ageText.text = Mathf.FloorToInt(startValue).ToString();
        },
        targetAge, ageAnimationDuration)
        .SetEase(ageEaseCurve); // <-- Inspector’da curve olarak ayarlayabilirsin
    }

    [Method]
    public void AnimateFailAnimation()
    {
        Debug.Log("Animating age text fail animation");

        if (ageText == null) return;


        Color startColor = ageText.color;

        Color endColor = Color.red;

        // Tween başlat
        ageText.DOColor(endColor, 0.5f)   // 0.5 sn’de beyaza geç
               .SetEase(Ease.OutQuad)     // yumuşak geçiş
               .OnComplete(() =>
               {
                   Debug.Log("Age text reached black");
               });
        CardImage.DOColor(endColor, 0.5f).SetEase(Ease.OutQuad);
        CardBackImage.DOColor(endColor, 0.5f).SetEase(Ease.OutQuad);
        TopImage.DOColor(endColor, 0.5f).SetEase(Ease.OutQuad);
        BottomImage.DOColor(endColor, 0.5f).SetEase(Ease.OutQuad);
        mainCamera.DOColor(Color.white, 0.5f).SetEase(Ease.OutQuad);
    }
}
