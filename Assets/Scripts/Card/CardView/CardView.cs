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
    [SerializeField] private float maxTiltAngle = 12f;
    [SerializeField] private float swipeDuration = 0.4f;
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

        if (config == null)
        {
            Debug.LogError("[CardView] SetContent(config=null)");
            
            if (QuoteText) QuoteText.text = "";
            if (NameText)  NameText.text  = "";
            if (LeftAnswerText)  LeftAnswerText.text  = "";
            if (RightAnswerText) RightAnswerText.text = "";
            if (artworkImage) { artworkImage.sprite = null; artworkImage.enabled = false; }
            canvasGroup.alpha = 1f;
            return;
        }

        // --- Artwork ---
        var sprite = config.Artwork; 
        if (artworkImage)
        {
            artworkImage.sprite = sprite;
            artworkImage.enabled = (sprite != null);
        }

        // --- Texts ---
        if (QuoteText) QuoteText.text = config.Description; 
        if (NameText)  NameText.text  = config.Title;       

        // --- Answers must start empty and fade in when set ---
        if (LeftAnswerText)  LeftAnswerText.text  = "";
        if (RightAnswerText) RightAnswerText.text = "";

        // Visuality
        canvasGroup.alpha = 1f;

        // Clean any ongoing tweens
        DOTween.Kill(QuoteText, complete: false);
        DOTween.Kill(NameText,  complete: false);
        DOTween.Kill(LeftAnswerText,  complete: false);
        DOTween.Kill(RightAnswerText, complete: false);
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
        float normalizedX = Mathf.Clamp(displacementX / screenHalfWidth, -1f, 1f);
        float zAngle = -normalizedX * maxTiltAngle;
        rt.localRotation = Quaternion.Euler(0f, 0f, zAngle);
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
        float targetX = toLeft ? rt.localPosition.x - offscreenDistance
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

    
    public void SetAnswerText(TMP_Text textComponent, string newText)
    {
        // if no change, do nothing
        if (textComponent.text == newText) return;

        // if empty, just set
        DOTween.Kill(textComponent); // kill ongoing tweens
        textComponent.DOFade(0f, 0.15f).OnComplete(() =>
        {
            textComponent.text = newText;

            if (!string.IsNullOrEmpty(newText))
            {
                textComponent.DOFade(1f, 0.25f)
                            .SetEase(Ease.InOutSine);
            }
        });
    }
}
