using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardController : MonoBehaviour,
                              IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Wiring")]
    [SerializeField] private CardSO cardSO;
    [SerializeField] private CardView view;

    [Header("Swipe Params")]
    [SerializeField, Range(0.1f, 0.9f)] private float swipeThreshold = 0.4f; // % of screen width
    [SerializeField] private bool destroyOnSwipe = true;

    public CardModel Model { get; private set; }

    private Vector3 initialLocalPos;
    private float screenHalf;

    private void Awake()
    {
        if (!view) view = GetComponent<CardView>();
        Model = new CardModel(cardSO);

        screenHalf = Screen.width * 0.5f;
    }

    private void Start()
    {
        view.SetContent(cardSO);
        view.CaptureInitial();
        initialLocalPos = view.RectT.localPosition;
    }

    private void OnEnable()
    {
        if (view)
        {
            view.CaptureInitial();
            initialLocalPos = view.RectT.localPosition;
        }
    }

    public void OnBeginDrag(PointerEventData _)
    {
        if (Model.IsLocked) return;
        initialLocalPos = view.RectT.localPosition;
    }

    public void OnDrag(PointerEventData data)
    {
        if (Model.IsLocked) return;
        view.SetDragVisual(data.delta.x, screenHalf);
    }

    public void OnEndDrag(PointerEventData _)
    {
        if (Model.IsLocked) return;

        float moved = Mathf.Abs(view.RectT.localPosition.x - initialLocalPos.x);
        float threshold = Screen.width * swipeThreshold;

        if (moved < threshold)
        {
            view.AnimateReturn().OnComplete(() => Model.RequestReset());
            return;
        }

        bool toLeft = view.RectT.localPosition.x < initialLocalPos.x;

        view.AnimateSwipeOut(toLeft, Screen.width)
            .OnComplete(() =>
            {
                Model.NotifySwiped(toLeft ? SwipeDirection.Left : SwipeDirection.Right);
                if (destroyOnSwipe) Destroy(gameObject); // ya da pool’a iade
            });
    }

    // CardController.cs (küçük ek)
    // ...
    public void Init(CardSO so)
    {
        this.cardSO = so;
        this.Model = new CardModel(cardSO);
        if (!view) view = GetComponent<CardView>();

        view.SetContent(cardSO);   // sprite vs. bağla
        view.CaptureInitial();
    }

}
