// CardFactory.cs
using UnityEngine;

public class CardFactory : MonoBehaviour
{
    [SerializeField] private CardPool pool;

    public CardController Create(CardSO data)
    {
        Debug.Log($"[Factory:{name}] Create() for: {data?.name}");
        var card = pool.Get();
        card.Init(data);   // ScriptableObject datasını karta uygula
        return card;
    }

    public void Despawn(CardController card)
    {
        Debug.Log($"[Factory:{name}] Despawn()");
        pool.Return(card);
    }
}
