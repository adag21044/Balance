using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class CardController : MonoBehaviour,
                              IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Wiring")]
    [SerializeField] private CardSO cardSO;
    [SerializeField] private CardView cardView;

    [Header("Swipe Params")]
    [SerializeField, Range(0.1f, 0.9f)] private float swipeThreshold = 0.4f; // % of screen width
    [SerializeField] private bool destroyOnSwipe = true;

    public CardModel Model { get; private set; }

    private Vector3 initialLocalPos;
    private float screenHalf;
    [SerializeField] private StatView statView;

    private void Awake()
    {
        if (!cardView) cardView = GetComponent<CardView>();
        Model = new CardModel(cardSO);

        screenHalf = Screen.width * 0.5f;
    }

    private void Start()
    {
        cardView.SetContent(cardSO);
        cardView.CaptureInitial();
        initialLocalPos = cardView.RectT.localPosition;

        cardView.SetContent(cardSO);
    }

    private void OnEnable()
    {
        if (cardView)
        {
            cardView.CaptureInitial();
            initialLocalPos = cardView.RectT.localPosition;
        }
    }

    public void OnBeginDrag(PointerEventData _)
    {
        if (Model.IsLocked) return;

        // Pointer preview
        StatModel.PreviewImpacts(cardSO);

        initialLocalPos = cardView.RectT.localPosition;
    }

    public void OnDrag(PointerEventData data)
    {
        if (Model.IsLocked) return;
        cardView.SetDragVisual(data.delta.x, screenHalf);

        float dx = cardView.RectT.localPosition.x - initialLocalPos.x;
        const float DEAD_ZONE_PX = 8f;

        // near center → both hidden
        if (Mathf.Abs(dx) <= DEAD_ZONE_PX)
        {
            cardView.SetAnswerText(cardView.LeftAnswerText,  "");
            cardView.SetAnswerText(cardView.RightAnswerText, "");
        }
        else if (dx > 0f)
        {
            // turning right → LEFT visible, RIGHT hidden
            cardView.SetAnswerText(cardView.LeftAnswerText,  cardSO.leftAnswer);
            cardView.SetAnswerText(cardView.RightAnswerText, ""); // <-- fix here
        }
        else
        {
            // turning left → RIGHT visible, LEFT hidden
            cardView.SetAnswerText(cardView.LeftAnswerText,  ""); // <-- and fix here
            cardView.SetAnswerText(cardView.RightAnswerText, cardSO.rightAnswer);
        }
    }


    public void OnEndDrag(PointerEventData _)
    {
        if (Model.IsLocked) return;

        float moved = Mathf.Abs(cardView.RectT.localPosition.x - initialLocalPos.x);
        float threshold = Screen.width * swipeThreshold;

        // Drag kısa ise geri dön
        if (moved < threshold)
        {
            cardView.AnimateReturn().OnComplete(() => Model.RequestReset());
        }
        else
        {
            bool toLeft = cardView.RectT.localPosition.x < initialLocalPos.x;

            cardView.AnimateSwipeOut(toLeft, Screen.width)
                .OnComplete(() =>
                {
                    Model.NotifySwiped(toLeft ? SwipeDirection.Left : SwipeDirection.Right);
                    if (destroyOnSwipe) Destroy(gameObject); // pool’a iade edilebilir
                });
        }

        // Drag bitince yazıları temizle
        cardView.SetAnswerText(cardView.LeftAnswerText,  "");
        cardView.SetAnswerText(cardView.RightAnswerText, "");


        // Pointerları kapat
        statView.ShowHeartPointer(false);
        statView.ShowCareerPointer(false);
        statView.ShowHappinessPointer(false);

    }

    public void Init(CardSO so)
    {
        this.cardSO = so;
        this.Model = new CardModel(cardSO);
        if (!cardView) cardView = GetComponent<CardView>();

        cardView.SetContent(cardSO);   // sprite vs. bağla
        cardView.CaptureInitial();
    }
    
    // CardView.cs içine ekle
    public void SetAnswerText(TMP_Text textComponent, string newText)
    {
        // Eğer zaten aynı yazıysa direkt return
        if (textComponent.text == newText) return;

        // Önce varsa eski yazıyı fade-out yap
        DOTween.Kill(textComponent); // önceki tweenleri öldür
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