// CardPool.cs
using System.Collections.Generic;
using UnityEngine;

public class CardPool : MonoBehaviour
{
    /*[SerializeField] private CardController prefab;
    [SerializeField] private Transform parentForSpawn;

    private readonly Stack<CardController> pool = new();

    public CardController Get()
    {
        if (pool.Count > 0)
        {
            var card = pool.Pop();
            card.gameObject.SetActive(true);
            return card;
        }
        return Instantiate(prefab, parentForSpawn);
    }

    public void Return(CardController card)
    {
        card.gameObject.SetActive(false);
        pool.Push(card);
    }*/
}
