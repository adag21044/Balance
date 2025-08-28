using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardInitialAnimation : MonoBehaviour
{
    [Header("Wiring (UI)")]
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private RectTransform stackCenter;
    [SerializeField] private GameObject pseudoCardPrefab;
    [SerializeField] private RectTransform realCard;

    [Header("Animation Settings")]
    [SerializeField, Range(1, 10)] private int pseudoCount = 4;
    [SerializeField] private float flyDuration = 0.65f;   // Daha ağır
    [SerializeField] private float stagger = 0.2f;        // Kartlar arası bekleme
    [SerializeField] private float verticalOffset = -6f;
    [SerializeField] private float startScale = 0.92f;
    [SerializeField] private float endScale = 1.0f;
    [SerializeField] private Ease flyEase = Ease.OutExpo; // Daha dramatik

    [Header("Real Card Reveal")]
    [SerializeField] private float revealDelay = 0.3f;
    [SerializeField] private float revealPunch = 0.15f;
    [SerializeField] private float revealPunchTime = 0.22f;

    [Header("Audio")]  // 🔊 Ses ekleme
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cardFlyClip;

    private readonly System.Collections.Generic.List<RectTransform> spawnedPseudo = new();

    private void Awake()
    {
        if (realCard != null)
        {
            realCard.localScale = Vector3.zero;
            realCard.anchoredPosition = stackCenter.anchoredPosition;
        }

        AnimateInitialCards();
    }

    private void AnimateInitialCards()
    {
        if (spawnPoint == null || stackCenter == null || pseudoCardPrefab == null)
        {
            Debug.LogWarning("[CardInitialAnimation] Missing references.");
            return;
        }

        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < pseudoCount; i++)
        {
            var go = Instantiate(pseudoCardPrefab, stackCenter.parent);
            var rt = go.GetComponent<RectTransform>();
            spawnedPseudo.Add(rt);

            rt.anchoredPosition = spawnPoint.anchoredPosition;
            rt.localScale = Vector3.one * startScale;
            rt.SetSiblingIndex(stackCenter.GetSiblingIndex());

            Vector2 targetPos = stackCenter.anchoredPosition + new Vector2(0f, i * verticalOffset);
            float t = flyDuration;

            seq.Insert(i * stagger, rt.DOAnchorPos(targetPos, t).SetEase(flyEase).SetLink(go));
            seq.Insert(i * stagger, rt.DOScale(endScale, t).SetEase(Ease.OutQuad).SetLink(go));
            seq.Insert(i * stagger, rt.DOLocalRotate(new Vector3(0, 0, Random.Range(-4f, 4f)), t * 0.9f)
                                    .SetEase(Ease.OutQuad).SetLink(go));

            // 🔊 Ses çalma - kart uçarken veya indiğinde
            int captureIndex = i;
            seq.InsertCallback(i * stagger, () =>
            {
                if (audioSource != null && cardFlyClip != null)
                    audioSource.PlayOneShot(cardFlyClip);
            });
        }

        seq.AppendInterval(revealDelay);

        if (realCard != null)
        {
            seq.Append(realCard.DOScale(1f, 0.18f)
                               .SetEase(Ease.OutBack, 2f)
                               .SetLink(realCard.gameObject));
        }

        seq.OnComplete(() =>
        {
            foreach (var rt in spawnedPseudo)
            {
                if (rt != null) Destroy(rt.gameObject);
            }
            spawnedPseudo.Clear();
        });

        seq.Play();
    }
}
