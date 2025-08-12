// CardDeckController.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class CardDeckController : MonoBehaviour
{
    [Header("Data Feed")]
    [SerializeField] private CardFeedSO feed;

    [Header("Factory/Pool")]
    [SerializeField] private CardFactory factory;

    [Header("Layout")]
    [SerializeField] private int keepOnScreen = 1; // aynı anda kaç kart
    [SerializeField] private bool destroyOnSwipe = false; // pool kullanıyorsan false kalabilir

    private int index = 0;
    private List<CardController> liveCards = new();

    private void Start()
    {
        // Başlangıçta ekranda hedef sayıda kart üret
        for (int i = 0; i < keepOnScreen; i++)
            TrySpawnNext();
    }

    private void TrySpawnNext()
    {
        var next = GetNextSO();
        if (next == null) return;

        var card = factory.Create(next);
        // Kartın domain event’ine abone ol: swipe olunca yenisini üret
        card.Model.Swiped += OnCardSwiped;
        liveCards.Add(card);
    }

    private void OnCardSwiped(CardModel model, SwipeDirection dir)
    {
        // Model’den CardController’ı bul
        var ctrl = liveCards.Find(c => ReferenceEquals(c.Model, model));
        if (ctrl != null)
        {
            // Ekrandan aldıktan biraz sonra havuza döndür (animasyon bitince destroy ediliyorsa bu satır gerekmez)
            // Burada CardController içindeki destroyOnSwipe kapalı olmalı. Aksi halde pool yerine Destroy çalışır.
            factory.Despawn(ctrl);
            liveCards.Remove(ctrl);
        }

        // Boşalan slot için yenisini üret
        TrySpawnNext();
    }

    private CardSO GetNextSO()
    {
        if (feed == null || feed.items == null || feed.items.Length == 0) return null;

        switch (feed.mode)
        {
            case CardFeedSO.Mode.Loop:
                var so = feed.items[index % feed.items.Length];
                index++;
                return so;

            case CardFeedSO.Mode.Shuffle:
                int r = UnityEngine.Random.Range(0, feed.items.Length);
                return feed.items[r];

            case CardFeedSO.Mode.Once:
                if (index >= feed.items.Length) return null;
                var once = feed.items[index];
                index++;
                return once;
        }
        return null;
    }
}
