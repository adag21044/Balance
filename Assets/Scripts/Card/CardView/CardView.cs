// CardView.cs (visuals + tween animations)
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class CardView : MonoBehaviour
{
    [Header("Visual Refs")]
    [SerializeField] private Image artworkImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private float maxTiltAngle   = 12f;
    [SerializeField] private float swipeDuration  = 0.4f;
    [SerializeField] private float returnDuration = 0.25f;

    private RectTransform rt;
    private Vector3 initialLocalPos;

    public RectTransform RectT => rt;

    // en üste ekle
    private bool initialized = false;
    private void EnsureInit()
    {
        if (initialized) return;
        rt = GetComponent<RectTransform>();
        if (!canvasGroup)
            canvasGroup = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        initialized = true;
    }

    private void Awake() => EnsureInit();

    // --- Aşağıdaki public metotların başına EnsureInit() ekle ---
    public void SetContent(CardSO config)
    {
        EnsureInit();
        if (artworkImage) artworkImage.sprite = config.Artwork;
        canvasGroup.alpha = 1f;
    }

    public void CaptureInitial()
    {
        EnsureInit();
        initialLocalPos = rt.localPosition;
    }

    public void SetDragVisual(float deltaX, float screenHalfWidth)
    {
        EnsureInit();
        rt.localPosition += new Vector3(deltaX, 0f, 0f);
        float displacementX = rt.localPosition.x - initialLocalPos.x;
        float normalizedX   = Mathf.Clamp(displacementX / screenHalfWidth, -1f, 1f);
        float zAngle        = -normalizedX * maxTiltAngle;
        rt.localRotation    = Quaternion.Euler(0f, 0f, zAngle);
    }

    public Tween AnimateReturn()
    {
        EnsureInit();
        return DOTween.Sequence()
            .Append(rt.DOLocalMove(initialLocalPos, returnDuration).SetEase(Ease.OutBack))
            .Join(rt.DOLocalRotate(Vector3.zero, returnDuration).SetEase(Ease.OutBack));
    }

    public Tween AnimateSwipeOut(bool toLeft, float offscreenDistance)
    {
        EnsureInit();
        float targetX     = toLeft ? rt.localPosition.x - offscreenDistance
                                : rt.localPosition.x + offscreenDistance;
        float targetAngle = toLeft ? +maxTiltAngle * 2f : -maxTiltAngle * 2f;

        return DOTween.Sequence()
            .Append(rt.DOLocalMoveX(targetX, swipeDuration).SetEase(Ease.InQuad))
            .Join(rt.DOLocalRotate(new Vector3(0, 0, targetAngle), swipeDuration))
            .Join(canvasGroup.DOFade(0f, swipeDuration));
    }

    public void ResetVisuals()
    {
        EnsureInit();
        rt.localPosition = initialLocalPos;
        rt.localRotation = Quaternion.identity;
        canvasGroup.alpha = 1f;
    }

}
