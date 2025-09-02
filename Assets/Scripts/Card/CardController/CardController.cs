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
    private SwipeDirection lastSwipeDir = SwipeDirection.Right;

    
    [Header("End Card Placement")]
    [SerializeField] private Vector2 endCardAnchoredPos = new Vector2(0f, -165.8f);


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
            cardView.SetAnswerText(cardView.LeftAnswerText, "");
            cardView.SetAnswerText(cardView.RightAnswerText, "");
        }
        else if (dx > 0f)
        {
            // Swiping right -> show LEFT answer
            cardView.SetAnswerText(cardView.LeftAnswerText, cardSO.leftAnswer);
            cardView.SetAnswerText(cardView.RightAnswerText, "");
        }
        else
        {
            // Swiping left -> show RIGHT answer
            cardView.SetAnswerText(cardView.LeftAnswerText, "");
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
            lastSwipeDir = toLeft ? SwipeDirection.Left : SwipeDirection.Right;

            // Capture the current SO BEFORE any reload
            var decidedCard = cardSO;

            soundPlayer.PlaySwipeSound();

            cardView.AnimateSwipeOut(toLeft, Screen.width)
                .OnComplete(() =>
                {
                    Model.NotifySwiped(lastSwipeDir);

                    // Apply stat changes of the card
                    StatModel.Instance.ApplyCard(decidedCard, lastSwipeDir);

                    if (isGameOver) return;

                    // --- KEY PART: reload instead of destroy ---
                    if (destroyOnSwipe)
                    {
                        Destroy(gameObject); // old behavior (not recommended for single-card flow)
                    }
                    else
                    {
                        ReloadWith(GetChainedCardOrRandom());
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
        rt.localScale = Vector3.one * 0.9f;
        cardView.transform.SetAsLastSibling();

        var cg = cardView.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        Sequence show = DOTween.Sequence();
        if (cg != null) show.Join(cg.DOFade(1f, 0.2f));
        show.Join(rt.DOScale(1f, 0.2f).SetEase(Ease.OutSine));

        // yanıt yazılarını temizle
        cardView.SetAnswerText(cardView.LeftAnswerText, "");
        cardView.SetAnswerText(cardView.RightAnswerText, "");
    }


    private void OnSwipedByMovement(bool toLeft)
    {
        if (isGameOver) return;

        Model.NotifySwiped(toLeft ? SwipeDirection.Left : SwipeDirection.Right);
        StatModel.Instance.ApplyCard(cardSO, toLeft ? SwipeDirection.Left : SwipeDirection.Right);

        if (!isGameOver)
            ReloadWith(GetChainedCardOrRandom());
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
        rt.localPosition = initialLocalPos;
        rt.localScale = Vector3.one * 0.9f;   // scale down a bit

        // set as last sibling to be on top of other cards
        cardView.transform.SetAsLastSibling();

        // animate in
        var cg = cardView.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        Sequence show = DOTween.Sequence();
        if (cg != null) show.Join(cg.DOFade(1f, 0.2f));
        show.Join(rt.DOScale(1f, 0.2f).SetEase(Ease.OutSine));

        cardView.SetAnswerText(cardView.LeftAnswerText, "");
        cardView.SetAnswerText(cardView.RightAnswerText, "");

        Debug.Log("[CardController] ReloadWithRandomCard() DONE");
    }

    // Get chained card, if exists; otherwise, pick random
    // Returns a chained card based on lastSwipeDir; falls back to random if none.
    // CardController içine ekle
    private CardSO GetChainedCardOrRandom()
    {
        if (cardSO == null) return PickRandomSO();

        CardSO next = (lastSwipeDir == SwipeDirection.Left)
            ? cardSO.nextOnLeft
            : cardSO.nextOnRight;

        if (next != null && next != cardSO)
            return next;

        // pool connections
        CardSO[] pool = (lastSwipeDir == SwipeDirection.Left)
            ? cardSO.nextPoolLeft
            : cardSO.nextPoolRight;

        if (pool != null && pool.Length > 0)
        {
            int tries = 5;
            while (tries-- > 0)
            {
                var candidate = pool[Random.Range(0, pool.Length)];
                if (candidate != null && candidate != cardSO) return candidate;
            }
            foreach (var c in pool) if (c != null) return c;
        }

        // random if no chain found
        return PickRandomSO();
    }


    // Same as ReloadWithRandomCard(), but takes an explicit CardSO
    private void ReloadWith(CardSO next)
    {
        if (next == null)
        {
            Debug.LogWarning("[CardController] ReloadWith(null) -> falling back to random");
            next = PickRandomSO();
            if (next == null)
            {
                Debug.LogError("[CardController] No card available!");
                return;
            }
        }

        cardSO = next;
        Debug.Log($"[CardController] ReloadWith() -> {cardSO.name}");

        DOTween.Kill(cardView, complete: false);
        DOTween.Kill(cardView.gameObject, complete: false);
        ResetAllAlphas(cardView.gameObject);

        Model = new CardModel(cardSO);
        cardView.SetContent(cardSO);

        var rt = cardView.RectT;
        rt.localRotation = Quaternion.identity;
        rt.localPosition = initialLocalPos;
        rt.localScale = Vector3.one * 0.9f;
        cardView.transform.SetAsLastSibling();

        var cg = cardView.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        var show = DOTween.Sequence();
        if (cg != null) show.Join(cg.DOFade(1f, 0.2f));
        show.Join(rt.DOScale(1f, 0.2f).SetEase(Ease.OutSine));

        cardView.SetAnswerText(cardView.LeftAnswerText, "");
        cardView.SetAnswerText(cardView.RightAnswerText, "");
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

        // TMP texts
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

    public void SetEndGameCardByIndex(int index)
    {
        if (gameEndSOs == null || gameEndSOs.Length == 0)
        {
            Debug.LogError("[CardController] gameEndSOs is null or empty!");
            return;
        }

        if (index < 0 || index >= gameEndSOs.Length)
        {
            Debug.LogError($"[CardController] End card index {index} is out of range!");
            index = Mathf.Clamp(index, 0, gameEndSOs.Length - 1);
        }

        isGameOver = true;

        // 1) ÖNCE kartı seç
        cardSO = gameEndSOs[index];
        Model = new CardModel(cardSO);

        // 2) SONRA görseli uygula
        ApplyEndCardVisuals();
    }


    public void SetEndGameCardByIndices(int minInclusive, int maxExclusive)
    {
        // Pick a random valid index from the provided list
        int chosen = UnityEngine.Random.Range(minInclusive, maxExclusive);
        SetEndGameCardByIndex(chosen);
    }

    private void ApplyEndCardVisuals()
    {
        DOTween.Kill(cardView, complete: false);
        DOTween.Kill(cardView.gameObject, complete: false);
        ResetAllAlphas(cardView.gameObject);

        // İçerik güncelle
        cardView.SetContent(cardSO);

        ResetCardPosition();

        var rt = cardView.RectT;

        // UI için güvenli anchor/pivot
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // Dönüş/ölçek reset
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one * 0.9f;

        // KRİTİK: son kart hedef konumu
        rt.anchoredPosition = endCardAnchoredPos;

        cardView.transform.SetAsLastSibling();

        var cg = cardView.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        var show = DOTween.Sequence();
        if (cg != null) show.Join(cg.DOFade(1f, 0.2f));
        show.Join(rt.DOScale(1f, 0.2f).SetEase(Ease.OutSine));

        cardView.SetAnswerText(cardView.LeftAnswerText, "");
        cardView.SetAnswerText(cardView.RightAnswerText, "");
    }

    private void ResetCardPosition()
    {
        var rt = cardView.RectT;

        // UI için güvenli anchor/pivot
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // Son kartta istenen sabit hedef
        rt.anchoredPosition = endCardAnchoredPos;

        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;
    }
}
