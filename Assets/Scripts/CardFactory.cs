// CardFactory.cs
using UnityEngine;

public class CardFactory : MonoBehaviour
{
    [SerializeField] private CardPool pool;

    public CardController Create(CardSO data)
    {
        var card = pool.Get();
        card.Init(data);
        return card;
    }

    public void Despawn(CardController card) => pool.Return(card);
}
