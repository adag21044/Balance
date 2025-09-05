using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class StartScreenAnimator : MonoBehaviour
{
    [SerializeField] private float speed = 50f;
    [SerializeField] private TMP_Text touchToStartText;

    private void Start()
    {
        Rotate();
        BlinkText();
    }

    private void Rotate()
    {
        transform
        .DORotate(new Vector3(0, 0, 360), speed, RotateMode.FastBeyond360)
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Restart)
        .SetSpeedBased();
    }

    private void BlinkText()
    {
        touchToStartText
        .DOFade(0, 1.5f) // Fade to 0 over 1.5 seconds
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(Ease.InOutSine);
    }
}
