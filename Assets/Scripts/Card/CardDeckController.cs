using System.Collections.Generic;
using UnityEngine;

public class CardDeckController : MonoBehaviour
{
    [Header("Data Feed")]
    [SerializeField] private CardFeedSO feed;

    [Header("Factory/Pool")]
    [SerializeField] private CardFactory factory;

    private CardController front; // visible one when idle
    private CardController back;  // preloaded under front during drag

    private int index = 0;

    private void Start()
    {
        Debug.Log("[Deck] Start()");

        SpawnFront(); // exactly one card on scene initially
    }

    private void SpawnFront()
    {
        var next = GetNextSO();
        Debug.Log($"[Deck] SpawnFront() -> {next?.name}");
        if (next == null) return;

        front = factory.Create(next);
        front.transform.SetSiblingIndex(1); // keep on top when we later add 'back'
        front.BeginDragRequested += OnFrontBeginDrag;
        front.Model.Swiped += OnFrontSwiped;
    }

    private void OnFrontBeginDrag()
    {
        // If no back yet, preload immediately under the front
        if (back == null)
        {
            var next = GetNextSO();
            if (next == null) return;

            back = factory.Create(next);
            // Put back UNDER the front visually
            back.transform.SetSiblingIndex(0);
            // We do NOT subscribe to back's events yet; it will become new front after swipe
        }
    }

    private void OnFrontSwiped(CardModel m, SwipeDirection dir)
    {
        // 1) Remove current front
        front.BeginDragRequested -= OnFrontBeginDrag;
        front.Model.Swiped -= OnFrontSwiped;
        factory.Despawn(front);

        // 2) Promote back -> new front
        if (back != null)
        {
            front = back;
            back = null;

            // Now that it's the new front, wire its events
            front.BeginDragRequested += OnFrontBeginDrag;
            front.Model.Swiped += OnFrontSwiped;

            // Ensure order (only one card currently, so index 1 is fine)
            front.transform.SetSiblingIndex(1);
        }
        else
        {
            // No back existed (edge case): just spawn a new front
            SpawnFront();
        }
    }

    private CardSO GetNextSO()
    {
        if (feed == null || feed.items == null || feed.items.Length == 0)
            return null;

        switch (feed.mode)
        {
            case CardFeedSO.Mode.Loop:
                if (index >= feed.items.Length) index = 0;
                return feed.items[index++];

            case CardFeedSO.Mode.Shuffle:
                return feed.items[Random.Range(0, feed.items.Length)];

            case CardFeedSO.Mode.Once:
                if (index >= feed.items.Length) return null;
                return feed.items[index++];
        }
        return null;
    }
}
