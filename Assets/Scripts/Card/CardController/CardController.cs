using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class CardController : MonoBehaviour,
                              IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Wiring")]
    [SerializeField] private CardSO[] cardSOs;   // deck
    [SerializeField] private CardSO cardSO;      // current
    [SerializeField] private CardSO[] gameEndSOs;   
    [SerializeField] private CardView cardView;

    [Header("Swipe Params")]
    [SerializeField, Range(0.1f, 0.9f)] private float swipeThreshold = 0.4f;
    [SerializeField] private bool destroyOnSwipe = false; // <-- default false: we reuse the same object

    public CardModel Model { get; private set; }

    private Vector3 initialLocalPos;
    private float screenHalf;
    [SerializeField] private StatView statView;

    // Keep last index to avoid immediate repeats
    private int lastIndex = -1;

    private bool isGameOver;
    [SerializeField] private CardSoundPlayer soundPlayer;

    private void Awake()
    {
        if (!cardView) cardView = GetComponent<CardView>();
        cardSO = PickRandomSO();
        Model = new CardModel(cardSO);
        screenHalf = Screen.width * 0.5f;

        // 👉 CardMovement event
        var mover = GetComponent<CardMovement>();
        if (mover != null)
            mover.Swiped += OnSwipedByMovement;
    }

    private void Start()
    {
        cardView.SetContent(cardSO);
        cardView.CaptureInitial();
        initialLocalPos = cardView.RectT.localPosition;
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
        if (Model.IsLocked || isGameOver) return;
        StatModel.PreviewImpacts(cardSO);
        initialLocalPos = cardView.RectT.localPosition;
    }

    public void OnDrag(PointerEventData data)
    {
        if (Model.IsLocked || isGameOver) return;
        cardView.SetDragVisual(data.delta.x, screenHalf);

        float dx = cardView.RectT.localPosition.x - initialLocalPos.x;
        const float DEAD_ZONE_PX = 8f;

        if (Mathf.Abs(dx) <= DEAD_ZONE_PX)
        {
            cardView.SetAnswerText(cardView.LeftAnswerText,  "");
            cardView.SetAnswerText(cardView.RightAnswerText, "");
        }
        else if (dx > 0f)
        {
            // Swiping right -> show LEFT answer
            cardView.SetAnswerText(cardView.LeftAnswerText,  cardSO.leftAnswer);
            cardView.SetAnswerText(cardView.RightAnswerText, "");
        }
        else
        {
            // Swiping left -> show RIGHT answer
            cardView.SetAnswerText(cardView.LeftAnswerText,  "");
            cardView.SetAnswerText(cardView.RightAnswerText, cardSO.rightAnswer);
        }
    }

    public void OnEndDrag(PointerEventData _)
    {
        if (Model.IsLocked || isGameOver) return;

        float moved = Mathf.Abs(cardView.RectT.localPosition.x - initialLocalPos.x);
        float threshold = Screen.width * swipeThreshold;
        bool toLeft = cardView.RectT.localPosition.x < initialLocalPos.x;

        if (moved < threshold)
        {
            cardView.AnimateReturn().OnComplete(() => Model.RequestReset());
        }
        else
        {
            // Capture the current SO BEFORE any reload
            var decidedCard = cardSO;
            
            soundPlayer.PlaySwipeSound();
            
            cardView.AnimateSwipeOut(toLeft, Screen.width)
                .OnComplete(() =>
                {
                    Model.NotifySwiped(toLeft ? SwipeDirection.Left : SwipeDirection.Right);

                    // Apply stat changes of the card
                    StatModel.Instance.ApplyCard(decidedCard, toLeft ? SwipeDirection.Left : SwipeDirection.Right);

                    if (isGameOver) return;

                    // --- KEY PART: reload instead of destroy ---
                    if (destroyOnSwipe)
                    {
                        Destroy(gameObject); // old behavior (not recommended for single-card flow)
                    }
                    else
                    {
                        ReloadWithRandomCard(); // new behavior
                    }
                });
        }

        // Clear UI after drag ends
        cardView.SetAnswerText(cardView.LeftAnswerText, "");
        cardView.SetAnswerText(cardView.RightAnswerText, "");

        // Hide pointers
        statView.ShowHeartPointer(false);
        statView.ShowCareerPointer(false);
        statView.ShowHappinessPointer(false);
    }

    private void Update()
    {
        // For testing purposes
        //if(Input.GetKeyDown(KeyCode.E))
        //{
        //    Debug.Log("[CardController] setting end game card");
        //    SetEndGameCard();
        //}
    }
    public void SetEndGameCard(StatModel statModel)
    {
        Debug.Log("[CardController] SetEndGameCard() called");

        isGameOver = true;

        if (gameEndSOs == null || gameEndSOs.Length == 0)
        {
            Debug.LogError("[CardController] gameEndSO array is null or empty! Cannot set end game card.");
            return;
        }    

        int idx = Random.Range(0, gameEndSOs.Length);
        cardSO = gameEndSOs[idx];
        Model = new CardModel(cardSO);

        cardView.SetContent(cardSO);

        // --- UI reset ekle ---
        DOTween.Kill(cardView, complete: false);
        DOTween.Kill(cardView.gameObject, complete: false);
        ResetAllAlphas(cardView.gameObject);

        var rt = cardView.RectT;
        rt.localRotation = Quaternion.identity;
        rt.localPosition = initialLocalPos;
        rt.localScale    = Vector3.one * 0.9f;
        cardView.transform.SetAsLastSibling();

        var cg = cardView.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        Sequence show = DOTween.Sequence();
        if (cg != null) show.Join(cg.DOFade(1f, 0.2f));
        show.Join(rt.DOScale(1f, 0.2f).SetEase(Ease.OutSine));

        // yanıt yazılarını temizle
        cardView.SetAnswerText(cardView.LeftAnswerText,  "");
        cardView.SetAnswerText(cardView.RightAnswerText, "");
    }


    private void OnSwipedByMovement(bool toLeft)
    {
        if (isGameOver) return;

        Model.NotifySwiped(toLeft ? SwipeDirection.Left : SwipeDirection.Right);
        StatModel.Instance.ApplyCard(cardSO, toLeft ? SwipeDirection.Left : SwipeDirection.Right);
        
        if (!isGameOver)
            ReloadWithRandomCard();
    }

    public void Init(CardSO so)
    {
        this.cardSO = so;
        this.Model = new CardModel(cardSO);
        if (!cardView) cardView = GetComponent<CardView>();
        cardView.SetContent(cardSO);
        cardView.CaptureInitial();
    }

    // ---------- Helpers ----------

    // Pick a random CardSO different from the last one, if possible
    private CardSO PickRandomSO()
    {
        if (cardSOs == null || cardSOs.Length == 0) return cardSO; // safety
        if (cardSOs.Length == 1) { lastIndex = 0; return cardSOs[0]; }

        int idx;
        do { idx = Random.Range(0, cardSOs.Length); }
        while (idx == lastIndex);

        lastIndex = idx;
        return cardSOs[idx];
    }

    // Reuse the same GameObject and bind a fresh random CardSO
    private void ReloadWithRandomCard()
    {
        // --- 0) Debug: gerçekten buraya geliyor muyuz?
        Debug.Log("[CardController] ReloadWithRandomCard() CALLED");

        // --- 1) Yeni veri seç
        var next = PickRandomSO();
        if (next == null)
        {
            Debug.LogError("[CardController] cardSOs bos ya da null! Inspector’dan doldur.");
            return;
        }

        cardSO = next;
        Debug.Log($"[CardController] New cardSO = {cardSO.name}");

        // --- 2) Tweenleri durdur + tüm görselleri görünür yap
        DOTween.Kill(cardView, complete: false);
        DOTween.Kill(cardView.gameObject, complete: false);

        // CanvasGroup’ları ve tüm Graphics/TMP alpha’larını 1’e çek
        ResetAllAlphas(cardView.gameObject);

        // --- 3) Model ve Content güncelle
        Model = new CardModel(cardSO);
        cardView.SetContent(cardSO);  // <- CardView.SetContent sprite/text atamalı!

        // --- 4) Transform’u tam ortaya sıfırla
        var rt = cardView.RectT;
        rt.localRotation = Quaternion.identity;
        rt.localPosition = initialLocalPos;      // Start’ta yakaladığın merkez
        rt.localScale    = Vector3.one * 0.9f;   // küçükten büyüsün

        // En üstte dursun (UI z-sırası)
        cardView.transform.SetAsLastSibling();

        // --- 5) Minik giriş animasyonu
        var cg = cardView.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        Sequence show = DOTween.Sequence();
        if (cg != null) show.Join(cg.DOFade(1f, 0.2f));
        show.Join(rt.DOScale(1f, 0.2f).SetEase(Ease.OutSine));

        // --- 6) Yanıt yazılarını temizle
        cardView.SetAnswerText(cardView.LeftAnswerText,  "");
        cardView.SetAnswerText(cardView.RightAnswerText, "");

        Debug.Log("[CardController] ReloadWithRandomCard() DONE");
    }

    private void ResetAllAlphas(GameObject root)
    {
        // CanvasGroup
        var cgs = root.GetComponentsInChildren<CanvasGroup>(includeInactive: true);
        foreach (var c in cgs) c.alpha = 1f;

        // UI Graphics (Image, RawImage, vb.)
        var gfx = root.GetComponentsInChildren<Graphic>(includeInactive: true);
        foreach (var g in gfx)
        {
            var col = g.color;
            col.a = 1f;
            g.color = col;
        }

        // TMP yazılar
        var tmps = root.GetComponentsInChildren<TMP_Text>(includeInactive: true);
        foreach (var t in tmps) t.alpha = 1f;
    }

    // Smooth answer text swapping (already good)
    public void SetAnswerText(TMP_Text textComponent, string newText)
    {
        if (textComponent.text == newText) return;
        DOTween.Kill(textComponent);
        textComponent.DOFade(0f, 0.15f).OnComplete(() =>
        {
            textComponent.text = newText;
            if (!string.IsNullOrEmpty(newText))
                textComponent.DOFade(1f, 0.25f).SetEase(Ease.InOutSine);
        });
    }
}
