using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardInitialAnimation : MonoBehaviour
{
    [Header("Wiring (UI)")]
    [SerializeField] private RectTransform spawnPoint;     // Off-screen or corner start point
    [SerializeField] private RectTransform stackCenter;    // Where the pseudo cards pile up
    [SerializeField] private GameObject pseudoCardPrefab;  // A simple UI card (Image/Text)
    [SerializeField] private RectTransform realCard;       // The actual first card to show

    [Header("Animation Settings")]
    [SerializeField, Range(1, 10)] private int pseudoCount = 4; // How many pseudo cards to fly in
    [SerializeField] private float flyDuration = 0.45f;         // Each pseudo fly time
    [SerializeField] private float stagger = 0.08f;             // Delay between pseudo cards
    [SerializeField] private float verticalOffset = -6f;        // Small offset to look stacked
    [SerializeField] private float startScale = 0.92f;          // Pseudo start scale
    [SerializeField] private float endScale = 1.0f;             // Pseudo end scale
    [SerializeField] private Ease flyEase = Ease.OutCubic;

    [Header("Real Card Reveal")]
    [SerializeField] private float revealDelay = 0.12f;         // Wait after last pseudo lands
    [SerializeField] private float revealPunch = 0.15f;         // Punch scale intensity
    [SerializeField] private float revealPunchTime = 0.22f;

    // Internal
    private readonly System.Collections.Generic.List<RectTransform> spawnedPseudo = new();

    private void Awake()
    {
        // Hide real card initially (we reveal it at the end)
        if (realCard != null)
        {
            realCard.localScale = Vector3.zero; // hidden
            // Optional: ensure at center position from the start
            realCard.anchoredPosition = stackCenter.anchoredPosition;
        }

        AnimateInitialCards();
    }

    private void AnimateInitialCards()
    {
        // Safety checks
        if (spawnPoint == null || stackCenter == null || pseudoCardPrefab == null)
        {
            Debug.LogWarning("[CardInitialAnimation] Missing references.");
            return;
        }

        // Prepare a master sequence
        Sequence seq = DOTween.Sequence();

        // Spawn & animate pseudo cards
        for (int i = 0; i < pseudoCount; i++)
        {
            // Instantiate under the same canvas hierarchy
            var go = Instantiate(pseudoCardPrefab, stackCenter.parent); // same parent (Canvas child)
            var rt = go.GetComponent<RectTransform>();
            spawnedPseudo.Add(rt);

            // Reset visuals
            rt.anchoredPosition = spawnPoint.anchoredPosition;
            rt.localScale = Vector3.one * startScale;

            // Optional: give each a slightly different sibling to see pile ordering
            rt.SetSiblingIndex(stackCenter.GetSiblingIndex()); // near the center object

            // Target position with tiny vertical offset to look stacked
            Vector2 targetPos = stackCenter.anchoredPosition + new Vector2(0f, i * verticalOffset);
            float t = flyDuration;

            // Chain movement, scale, and maybe a slight rotation to add life
            // We use Insert to create a staggered flow
            seq.Insert(i * stagger, rt.DOAnchorPos(targetPos, t).SetEase(flyEase).SetLink(go));
            seq.Insert(i * stagger, rt.DOScale(endScale, t).SetEase(Ease.OutQuad).SetLink(go));

            // Optional: tiny rotation wiggle per card
            seq.Insert(i * stagger, rt.DOLocalRotate(new Vector3(0, 0, Random.Range(-4f, 4f)), t * 0.9f)
                                  .SetEase(Ease.OutQuad).SetLink(go));
        }

        // After the last pseudo lands, reveal the real card
        seq.AppendInterval(revealDelay);

        if (realCard != null)
        {
            // Scale from 0 to 1 quickly, then do a small punch
            seq.Append(realCard.DOScale(1f, 0.16f).SetEase(Ease.OutBack, 2f).SetLink(realCard.gameObject));
            //seq.Append(realCard.DOPunchScale(Vector3.one * revealPunch, revealPunchTime, 8, 0.9f)
            //                   .SetLink(realCard.gameObject));
        }

        // Clean up pseudo cards so only the real card remains
        seq.OnComplete(() =>
        {
            foreach (var rt in spawnedPseudo)
            {
                if (rt != null) Destroy(rt.gameObject);
            }
            spawnedPseudo.Clear();
        });

        // Optional: if you want this to run even when Time.timeScale = 0
        // seq.SetUpdate(true);

        // Start the sequence
        seq.Play();
    }
}
