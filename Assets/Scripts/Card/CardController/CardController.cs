using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;

public class CardController : MonoBehaviour,
                              IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Wiring")]
    [SerializeField] private CardSO[] cardSOs;            // default deck
    [SerializeField] private CardSO cardSO;               // current card
    [SerializeField] private CardSO[] gameEndSOs;
    [SerializeField] private CardView cardView;
    [SerializeField] private CardInitialAnimation cardInitialAnimation;
    [SerializeField] private CardSoundPlayer soundPlayer;
    [SerializeField] private StatView statView;

    [Header("Swipe Params")]
    [SerializeField, Range(0.1f, 0.9f)] private float swipeThreshold = 0.4f;
    [SerializeField] private bool destroyOnSwipe = false; // prefer reuse

    [Header("End Card Placement")]
    [SerializeField] private Vector2 endCardAnchoredPos = new Vector2(0f, -165.8f);

    public CardModel Model { get; private set; }

    private Vector3 initialLocalPos;
    private float screenHalf;
    private int lastIndex = -1;                 // avoid instant repeats
    private bool isGameOver;
    private SwipeDirection lastSwipeDir = SwipeDirection.Right;

    // internal: cached components
    private CardMovement movement;
    private CanvasGroup canvasGroup;

    [SerializeField] private List<CardSO> debugNextCards = new();
    [SerializeField] private Queue<CardSO> nextCards = new Queue<CardSO>();
    [SerializeField] private int PRELOAD_COUNT = 5;

    #if UNITY_EDITOR
    [Header("DEBUG (Editor Only)")]
    [SerializeField] private bool debugForceNextCard = false;
    [SerializeField] private CardSO debugNextCard;
    #endif

    [SerializeField] private RunController runController;

    #region Unity Lifecycle
    private void Awake()
    {
        // Ensure references
        if (!cardView) cardView = GetComponent<CardView>();
        movement = GetComponent<CardMovement>();
        canvasGroup = cardView ? cardView.GetComponent<CanvasGroup>() : null;

        screenHalf = Screen.width * 0.5f;

        // If no current SO set from outside, pick one
        if (!cardSO) cardSO = PickRandomSO();

        // Build model for current SO
        Model = new CardModel(cardSO);

        // Preload next cards
        PreloadNextCards();
    }

    private void OnEnable()
    {
        if (movement != null)
            movement.Swiped += OnSwipedByMovement;

        if (cardInitialAnimation != null)
            cardInitialAnimation.OnAnimationCompleted += OnInitialAnimationDone;
    }

    private void Start()
    {
        // Either the intro animation will call back to InitCard, or we init directly
        if (cardInitialAnimation == null) InitCard();
    }

    private void OnDisable()
    {
        if (movement != null)
            movement.Swiped -= OnSwipedByMovement;

        if (cardInitialAnimation != null)
            cardInitialAnimation.OnAnimationCompleted -= OnInitialAnimationDone;
    }

    #endregion

    #region Drag & Swipe

    public void OnBeginDrag(PointerEventData _)
    {
        if (Model.IsLocked || isGameOver) return;
        initialLocalPos = cardView.RectT.localPosition;

        // Optional: live preview of stat impacts
        StatModel.PreviewImpacts(cardSO);
    }

    public void OnDrag(PointerEventData data)
    {
        if (Model.IsLocked || isGameOver) return;

        cardView.SetDragVisual(data.delta.x, screenHalf);

        // Show/hide answer hints with a small dead-zone
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
            // Not enough -> return back
            cardView.AnimateReturn().OnComplete(() => Model.RequestReset());
        }
        else
        {
            lastSwipeDir = toLeft ? SwipeDirection.Left : SwipeDirection.Right;

            // Capture decided card BEFORE reloading
            var decidedCard = cardSO;

            if (soundPlayer != null)
                soundPlayer.PlaySwipeSound();

            cardView.AnimateSwipeOut(toLeft, Screen.width)
                    .OnComplete(() =>
                    {
                        StatModel.CancelPreview();

                        // Domain notify + apply impacts
                        Model.NotifySwiped(lastSwipeDir);
                        StatModel.Instance.ApplyCard(decidedCard, lastSwipeDir);

                        if (isGameOver) return;

                        if (destroyOnSwipe)
                        {
                            Destroy(gameObject);
                        }
                        else
                        {
                            var next = ChooseNextCard(decidedCard, lastSwipeDir);
                            ReloadWith(next);
                        }
                    });
        }

        // Clear UI hints
        cardView.SetAnswerText(cardView.LeftAnswerText, "");
        cardView.SetAnswerText(cardView.RightAnswerText, "");

        // Hide pointers if view exists
        if (statView != null)
        {
            statView.ShowHeartPointer(false);
            statView.ShowCareerPointer(false);
            statView.ShowHappinessPointer(false);
            statView.ShowSociabilityPointer(false);
        }
    }

    #endregion

    #region Intro / Init

    private void InitCard()
    {
        // Bind visuals to current SO and cache positions
        cardView.SetContent(cardSO);
        cardView.CaptureInitial();
        initialLocalPos = cardView.RectT.localPosition;
        runController?.OnCardDisplayed(cardSO);
    }

    private void OnInitialAnimationDone()
    {
        // After the pseudo-cards fly-in, ensure card is in a clean state
        InitCard();
    }

    /// <summary>
    /// One-off: reloads with "pseudo-cards fly-in" intro (CardInitialAnimation).
    /// Call this when you want a special dramatic entry for the next card.
    /// </summary>
    public void ReloadWithAnimated(CardSO next)
    {
        if (next == null)
        {
            Debug.LogWarning("[CardController] ReloadWithAnimated(null) -> ignored");
            return;
        }

        // Bind new data first
        cardSO = next;
        Model = new CardModel(cardSO);
        cardView.SetContent(cardSO);

        // Ensure visible; CardInitialAnimation reveals using scale, not alpha
        if (!canvasGroup) canvasGroup = cardView.GetComponent<CanvasGroup>();
        if (canvasGroup) canvasGroup.alpha = 1f;


        // Let the animation drive the transform (it sets scale/position)
        if (cardInitialAnimation != null)
        {
            cardInitialAnimation.StartAnimation();
        }
        else
        {
            // Fallback to normal reload if no animator wired
            ReloadWith(next);
        }
    }

    #endregion

    #region Movement Event (Alternative Swipe Path)

    private void OnSwipedByMovement(bool toLeft)
    {
        if (isGameOver) return;

        var dir = toLeft ? SwipeDirection.Left : SwipeDirection.Right;
        Model.NotifySwiped(dir);
        StatModel.Instance.ApplyCard(cardSO, dir);

        if (!isGameOver)
        {
            // CHANGED: was ReloadWith(GetChainedCardOrRandom());
            var next = ChooseNextCard(cardSO, dir);
            ReloadWith(next);
        }
    }

    #endregion

    #region Reload / Chain / Random

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

    /// <summary>
    /// Returns a chained next card based on the last swipe dir; falls back to random.
    /// </summary>
    private CardSO GetChainedCardOrRandom()
    {
        if (cardSO == null) return PickRandomSO();

        // direct chain
        CardSO next = (lastSwipeDir == SwipeDirection.Left) ? cardSO.nextOnLeft : cardSO.nextOnRight;
        if (next != null && next != cardSO) return next;

        // pool chain
        CardSO[] pool = (lastSwipeDir == SwipeDirection.Left) ? cardSO.nextPoolLeft : cardSO.nextPoolRight;
        if (pool != null && pool.Length > 0)
        {
            // Try a few random picks to avoid repeating current
            int tries = 5;
            while (tries-- > 0)
            {
                var candidate = pool[Random.Range(0, pool.Length)];
                if (candidate != null && candidate != cardSO) return candidate;
            }
            // Fallback to first non-null
            foreach (var c in pool) if (c != null) return c;
        }

        // random if no chain found
        return PickRandomSO();
    }

    /// <summary>
    /// Normal reload with clean-in fade, reusing the same card GameObject.
    /// </summary>
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

        // Update data
        cardSO = next;
        Model = new CardModel(cardSO);

        // Kill tweens safely for this card only
        DOTween.Kill(cardView, complete: false);
        DOTween.Kill(cardView.gameObject, complete: false);

        // Ensure all graphics visible (alpha = 1) before we do our own entry
        ResetAllAlphas(cardView.gameObject);

        // Rebind visuals
        cardView.SetContent(cardSO);

        runController?.OnCardDisplayed(cardSO);

        // Clean transform + show
        CleanAndShowCard();
    }

    /// <summary>
    /// Reset transform to initial center-ish state and play a quick fade/scale-in.
    /// </summary>
    private void CleanAndShowCard()
    {
        var rt = cardView.RectT;

        // Reset transform
        rt.localRotation = Quaternion.identity;
        rt.localPosition = initialLocalPos;
        rt.localScale = Vector3.one * 0.9f;
        cardView.transform.SetAsLastSibling();

        // CanvasGroup fade-in
        if (!canvasGroup) canvasGroup = cardView.GetComponent<CanvasGroup>();
        if (canvasGroup) canvasGroup.alpha = 0f;

        var show = DOTween.Sequence().SetLink(cardView.gameObject);
        if (canvasGroup) show.Join(canvasGroup.DOFade(1f, 0.2f));
        show.Join(rt.DOScale(1f, 0.2f).SetEase(Ease.OutSine));

        // Clear answers
        cardView.SetAnswerText(cardView.LeftAnswerText, "");
        cardView.SetAnswerText(cardView.RightAnswerText, "");
    }

    #endregion

    #region End Game

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

        // 1) Choose end-card
        cardSO = gameEndSOs[index];
        Model = new CardModel(cardSO);

        // 2) Apply visuals with movement disabled
        ApplyEndCardVisuals();
    }

    public void SetEndGameCardByIndices(int minInclusive, int maxExclusive)
    {
        if (gameEndSOs == null || gameEndSOs.Length == 0)
        {
            Debug.LogError("[CardController] gameEndSOs is null or empty!");
            return;
        }

        minInclusive = Mathf.Clamp(minInclusive, 0, gameEndSOs.Length - 1);
        maxExclusive = Mathf.Clamp(maxExclusive, minInclusive + 1, gameEndSOs.Length);

        int chosen = Random.Range(minInclusive, maxExclusive);
        SetEndGameCardByIndex(chosen);
    }

    private void ApplyEndCardVisuals()
    {
        var rt = cardView.RectT;

        // Kill tweens on this card safely (do not force-complete)
        DOTween.Kill(cardView, complete: false);
        DOTween.Kill(cardView.gameObject, complete: false);
        rt.DOKill(false);
        DOTween.Kill(rt, complete: false);

        if (!canvasGroup) canvasGroup = cardView.GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = cardView.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.DOKill(false);

        ResetAllAlphas(cardView.gameObject);

        // Update content
        cardView.SetContent(cardSO);

        // Position to initial and show
        rt.localRotation = Quaternion.identity;
        rt.localPosition = initialLocalPos;
        rt.localScale = Vector3.one * 0.9f;
        cardView.transform.SetAsLastSibling();

        canvasGroup.alpha = 0f;

        DOTween.Sequence()
               .SetLink(cardView.gameObject)
               .Join(canvasGroup.DOFade(1f, 0.2f))
               .Join(rt.DOScale(1f, 0.2f).SetEase(Ease.OutSine));

        // Disable movement on game over
        //if (movement) movement.enabled = false;

        // Clear answers
        cardView.SetAnswerText(cardView.LeftAnswerText, "");
        cardView.SetAnswerText(cardView.RightAnswerText, "");
    }

    public void ForceEndCardRotation(float zDeg = 0f)
    {
        var rt = cardView.RectT;

        rt.DOKill(false);
        DOTween.Kill(rt, complete: false);
        rt.localRotation = Quaternion.Euler(0f, 0f, zDeg);

        Debug.Log($"[CardController] Forced end-card rotation to Z={zDeg}, current={rt.localEulerAngles}");
    }

    #endregion

    #region Utils

    private void ResetAllAlphas(GameObject root)
    {
        // CanvasGroups
        var cgs = root.GetComponentsInChildren<CanvasGroup>(includeInactive: true);
        foreach (var c in cgs) c.alpha = 1f;

        // UI Graphics
        var gfx = root.GetComponentsInChildren<Graphic>(includeInactive: true);
        foreach (var g in gfx)
        {
            var col = g.color;
            col.a = 1f;
            g.color = col;
        }

        // TMP
        var tmps = root.GetComponentsInChildren<TMP_Text>(includeInactive: true);
        foreach (var t in tmps) t.alpha = 1f;
    }

    #endregion
    private void PreloadNextCards()
    {
        nextCards.Clear();
        for (int i = 0; i < PRELOAD_COUNT; i++)
        {
            var nextCard = PickRandomSO();
            nextCards.Enqueue(nextCard);
        }

        debugNextCards = new List<CardSO>(nextCards);
    }

    private CardSO GetNextFromQueue()
    {
        if (nextCards.Count == 0)
        {
            PreloadNextCards();
        }

        var next = nextCards.Dequeue();
        PreloadNextCards(); // yeniden 5’e tamamla
        return next;
    }

    private void ReloadWithNextCard()
    {
        CardSO next = GetNextFromQueue();
        ReloadWith(next);
    }

    private CardSO ChooseNextCard(CardSO current, SwipeDirection dir)
    {
    #if UNITY_EDITOR
        if (debugForceNextCard && debugNextCard != null)
        {
            var forced = debugNextCard;
            debugNextCard = null;
            debugForceNextCard = false;
            return forced;
        }
    #endif

        // 1) direct chain
        if (current != null)
        {
            CardSO next = (dir == SwipeDirection.Left) ? current.nextOnLeft : current.nextOnRight;
            if (next != null && next != current) return next;

            // 2) pool chain
            CardSO[] pool = (dir == SwipeDirection.Left) ? current.nextPoolLeft : current.nextPoolRight;
            if (pool != null && pool.Length > 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    var c = pool[Random.Range(0, pool.Length)];
                    if (c != null && c != current) return c;
                }
                foreach (var c in pool) if (c != null) return c;
            }
        }

        // 3) fallback: queued/random
        return GetNextFromQueue();
    }
}
