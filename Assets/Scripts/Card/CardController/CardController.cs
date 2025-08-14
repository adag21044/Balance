using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

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
        initialLocalPos = cardView.RectT.localPosition;
    }

    public void OnDrag(PointerEventData data)
    {
        if (Model.IsLocked) return;
        cardView.SetDragVisual(data.delta.x, screenHalf);

        // Local position ile displacement hesapla
        float displacementX = cardView.RectT.localPosition.x - initialLocalPos.x;

        // Merkeze yakınsa yazıları temizle (tolerans: 10px)
        if (Mathf.Abs(displacementX) < 10f)
        {
            cardView.LeftAnswerText.text = "";
            cardView.RightAnswerText.text = "";
        }
        else if (displacementX > 0) // Swipe Right
        {
            cardView.LeftAnswerText.text = cardSO.leftAnswer;
            cardView.RightAnswerText.text = "";
        }
        else // Swipe Left
        {
            cardView.RightAnswerText.text = cardSO.rightAnswer;
            cardView.LeftAnswerText.text = "";
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
        cardView.LeftAnswerText.text = "";
        cardView.RightAnswerText.text = "";
    }

    public void Init(CardSO so)
    {
        this.cardSO = so;
        this.Model = new CardModel(cardSO);
        if (!cardView) cardView = GetComponent<CardView>();

        cardView.SetContent(cardSO);   // sprite vs. bağla
        cardView.CaptureInitial();
    }

}