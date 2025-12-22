using UnityEngine;
using DG.Tweening;
using TMPro;

public class StartScreenAnimator : MonoBehaviour
{
    [SerializeField] private float speed = 50f;
    [SerializeField] private TMP_Text touchToStartText;

    private Tween blinkTween;

    private void Awake()
    {
        // Başlangıçta text KAPALI
        touchToStartText.gameObject.SetActive(false);
    }

    private void Start()
    {
        Rotate();
        // ❌ Blink burada BAŞLAMAZ
    }

    private void Rotate()
    {
        transform
            .DORotate(new Vector3(0, 0, 360), speed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart)
            .SetSpeedBased();
    }

    // 🔓 Loading %100 olunca çağrılacak
    public void EnableBlinkText()
    {
        touchToStartText.gameObject.SetActive(true);

        blinkTween?.Kill();
        blinkTween = touchToStartText
            .DOFade(0, 1.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    // 🔒 Gerekirse tekrar kapatmak için
    public void DisableBlinkText()
    {
        blinkTween?.Kill();
        touchToStartText.alpha = 1f;
        touchToStartText.gameObject.SetActive(false);
    }
}
