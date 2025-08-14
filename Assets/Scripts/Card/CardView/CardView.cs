// CardView.cs (visuals + tween animations)
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class CardView : MonoBehaviour
{
    [Header("Visual Refs")]
    [SerializeField] private Image artworkImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text QuoteText;
    [SerializeField] private TMP_Text NameText;
    [SerializeField] public TMP_Text LeftAnswerText;
    [SerializeField] public TMP_Text RightAnswerText;

    [Header("Animation")]
    [SerializeField] private float maxTiltAngle   = 12f;
    [SerializeField] private float swipeDuration  = 0.4f;
    [SerializeField] private float returnDuration = 0.25f;

    private RectTransform rt;
    private Vector3 initialLocalPos;
    public RectTransform RectT => rt;
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

    public void SetContent(CardSO config)
    {
        EnsureInit();
        if (artworkImage) artworkImage.sprite = config.Artwork;
        canvasGroup.alpha = 1f;

        if (QuoteText) QuoteText.text = config.Description;
        if (NameText) NameText.text = config.Title;
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
        Vector3 overshootPos = initialLocalPos + new Vector3(10f, 0f, 0f); // 10 px ileri
        return DOTween.Sequence()
            // overshoot
            .Append(rt.DOLocalMove(overshootPos, returnDuration * 0.6f)
                .SetEase(Ease.OutQuad))
            
            .Append(rt.DOLocalMove(initialLocalPos, returnDuration * 0.4f)
                .SetEase(Ease.InOutSine))
            
            .Join(rt.DOLocalRotate(Vector3.zero, returnDuration)
                .SetEase(Ease.OutSine));
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
