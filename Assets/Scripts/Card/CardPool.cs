using System.Collections.Generic;
using UnityEngine;

public class CardPool : MonoBehaviour
{
    [SerializeField] private CardController prefab;
    [SerializeField] private Transform parentForSpawn;

    private readonly Stack<CardController> pool = new();
    private static int createdCount = 0; // total new instantiations

    public CardController Get()
    {
        Debug.Log($"[CardPool:{name}] Get() called. pooled={pool.Count}");

        CardController card;
        if (pool.Count > 0)
        {
            card = pool.Pop();
            card.gameObject.SetActive(true);
            Debug.Log($"[CardPool:{name}] Reused from pool. pooled-now={pool.Count}");
        }
        else
        {
            card = Instantiate(prefab, parentForSpawn);
            createdCount++;
            Debug.Log($"[CardPool:{name}] Instantiated NEW card #{createdCount}. pooled={pool.Count}");
        }
        return card;
    }

    public void Return(CardController card)
    {
        if (card == null)
        {
            Debug.LogWarning($"[CardPool:{name}] Return(null) called.");
            return;
        }

        card.gameObject.SetActive(false);
        pool.Push(card);
        Debug.Log($"[CardPool:{name}] Returned to pool. pooled-now={pool.Count}");
    }
}
