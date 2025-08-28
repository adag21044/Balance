using System.Collections.Generic;
using UnityEngine;

public class CardDeckController : MonoBehaviour
{
    [Header("Data Feed")]
    [SerializeField] private CardFeedSO feed;

    [Header("Factory/Pool")]
    [SerializeField] private CardFactory factory;

    [Header("Layout")]
    [SerializeField] private int keepOnScreen = 3; 
    [SerializeField] private bool destroyOnSwipe = true; 

    private int index = 0;
    [SerializeField] private List<CardController> liveCards = new();

    private void Start()
    {
        for (int i = 0; i < keepOnScreen; i++)
            TrySpawnNext();
    }

    private void TrySpawnNext()
    {
        var next = GetNextSO();
        if (next == null) return;
    }

    private void OnCardSwiped(CardModel model, SwipeDirection dir)
    {
        
        var ctrl = liveCards.Find(c => ReferenceEquals(c.Model, model));
        if (ctrl != null)
        {
            liveCards.Remove(ctrl);
        }

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
